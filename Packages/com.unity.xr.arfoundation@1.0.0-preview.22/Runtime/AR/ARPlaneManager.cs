using System;
using System.Collections.Generic;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARExtensions;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Creates, updates, and removes <c>GameObject</c>s with <see cref="ARPlane"/> components under the <see cref="ARSessionOrigin"/>'s <see cref="ARSessionOrigin.trackablesParent"/>.
    /// </summary>
    /// <remarks>
    /// When enabled, this component subscribes to <see cref="ARSubsystemManager.planeAdded"/>,
    /// <see cref="ARSubsystemManager.planeUpdated"/>, and <see cref="ARSubsystemManager.planeRemoved"/>.
    /// If this component is disabled, and there are no other subscribers to those events,
    /// plane detection will be disabled on the device.
    /// </remarks>
    [RequireComponent(typeof(ARSessionOrigin))]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/api/UnityEngine.XR.ARFoundation.ARPlaneManager.html")]
    public sealed class ARPlaneManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each created plane.")]
        GameObject m_PlanePrefab;

        /// <summary>
        /// Getter/setter for the Plane Prefab.
        /// </summary>
        public GameObject planePrefab
        {
            get { return m_PlanePrefab; }
            set { m_PlanePrefab = value; }
        }

        [SerializeField, PlaneDetectionFlagsMask]
        [Tooltip("Specifies the types of planes to detect.")]
        PlaneDetectionFlags m_DetectionFlags = k_PlaneDetectionFlagEverything;

        /// <summary>
        /// Get or set the <c>PlaneDetectionFlags</c> to use for plane detection.
        /// </summary>
        public PlaneDetectionFlags detectionFlags
        {
            get
            {
                if (m_DetectionFlags == k_PlaneDetectionFlagEverything)
                    return PlaneDetectionFlags.Horizontal | PlaneDetectionFlags.Vertical;

                return m_DetectionFlags;
            }
            set
            {
                m_DetectionFlags = value;

                if (enabled)
                    ARSubsystemManager.planeDetectionFlags = detectionFlags;
            }
        }

        /// <summary>
        /// Raised for each new <see cref="ARPlane"/> detected in the environment.
        /// </summary>
        public event Action<ARPlaneAddedEventArgs> planeAdded;

        /// <summary>
        /// Raised for each <see cref="ARPlane"/> every time it updates.
        /// </summary>
        public event Action<ARPlaneUpdatedEventArgs> planeUpdated;

        /// <summary>
        /// Raised whenever an <see cref="ARPlane"/> is removed.
        /// </summary>
        public event Action<ARPlaneRemovedEventArgs> planeRemoved;

        /// <summary>
        /// Get the number of planes managed by this manager.
        /// </summary>
        public int planeCount
        {
            get { return m_Planes.Count; }
        }

        /// <summary>
        /// Get all currently tracked <see cref="ARPlane"/>s.
        /// </summary>
        /// <param name="planes">Replaces the contents with the current list of planes.</param>
        public void GetAllPlanes(List<ARPlane> planes)
        {
            if (planes == null)
                throw new ArgumentNullException("planes");

            planes.Clear();
            foreach (var kvp in m_Planes)
            {
                planes.Add(kvp.Value);
            }
        }

        /// <summary>
        /// Attempts to retrieve an <see cref="ARPlane"/>.
        /// </summary>
        /// <param name="planeId">The <c>TrackableId</c> associated with the <see cref="ARPlane"/>.</param>
        /// <returns>The <see cref="ARPlane"/>if found. <c>null</c> otherwise.</returns>
        public ARPlane TryGetPlane(TrackableId planeId)
        {
            ARPlane plane;
            m_Planes.TryGetValue(planeId, out plane);

            return plane;
        }

        void Awake()
        {
            m_SessionOrigin = GetComponent<ARSessionOrigin>();
        }

        void OnEnable()
        {
            SyncPlanes();
            ARSubsystemManager.planeDetectionFlags = detectionFlags;
            ARSubsystemManager.planeAdded += OnPlaneAdded;
            ARSubsystemManager.planeUpdated += OnPlaneUpdated;
            ARSubsystemManager.planeRemoved += OnPlaneRemoved;
            ARSubsystemManager.sessionDestroyed += OnSessionDestroyed;
        }

        void OnDisable()
        {
            ARSubsystemManager.planeAdded -= OnPlaneAdded;
            ARSubsystemManager.planeUpdated -= OnPlaneUpdated;
            ARSubsystemManager.planeRemoved -= OnPlaneRemoved;
            ARSubsystemManager.sessionDestroyed -= OnSessionDestroyed;
        }

        void OnSessionDestroyed()
        {
            if (planeRemoved != null)
            {
                foreach (var kvp in m_Planes)
                {
                    var plane = kvp.Value;
                    planeRemoved(new ARPlaneRemovedEventArgs(plane));
                    plane.OnRemove();
                }
            }
            else
            {
                foreach (var kvp in m_Planes)
                    kvp.Value.OnRemove();
            }

            m_Planes.Clear();
        }

        void SyncPlanes()
        {
            var planeSubsystem = ARSubsystemManager.planeSubsystem;
            if (planeSubsystem == null)
                return;

            s_BoundedPlanes.Clear();
            planeSubsystem.GetAllPlanes(s_BoundedPlanes);

            // Check for added/updated planes
            s_TrackableIds.Clear();
            foreach (var boundedPlane in s_BoundedPlanes)
            {
                var planeId = boundedPlane.Id;

                ARPlane plane;
                if (m_Planes.TryGetValue(planeId, out plane))
                {
                    plane.boundedPlane = boundedPlane;
                    if (planeUpdated != null)
                        planeUpdated(new ARPlaneUpdatedEventArgs(plane));
                }
                else
                {
                    plane = AddPlane(boundedPlane);
                    plane.boundedPlane = boundedPlane;
                    if (planeAdded != null)
                        planeAdded(new ARPlaneAddedEventArgs(plane));
                }

                s_TrackableIds.Add(planeId);
            }

            // Check for removed planes
            s_PlanesToRemove.Clear();
            foreach (var kvp in m_Planes)
            {
                var planeId = kvp.Key;
                if (!s_TrackableIds.Contains(planeId))
                    s_PlanesToRemove.Add(planeId);
            }

            foreach (var id in s_PlanesToRemove)
                RemovePlane(m_Planes[id]);
        }

        GameObject CreateGameObject()
        {
            if (planePrefab != null)
                return Instantiate(planePrefab, m_SessionOrigin.trackablesParent);

            var go = new GameObject();
            go.transform.SetParent(m_SessionOrigin.trackablesParent, false);
            go.layer = gameObject.layer;
            return go;
        }

        ARPlane AddPlane(BoundedPlane boundedPlane)
        {
            var go = CreateGameObject();
            var plane = go.GetComponent<ARPlane>();
            if (plane == null)
                plane = go.AddComponent<ARPlane>();

            m_Planes.Add(boundedPlane.Id, plane);

            return plane;
        }

        void OnPlaneAdded(PlaneAddedEventArgs eventArgs)
        {
            var boundedPlane = eventArgs.Plane;
            var plane = AddPlane(boundedPlane);
            plane.boundedPlane = boundedPlane;

            if (planeAdded != null)
                planeAdded(new ARPlaneAddedEventArgs(plane));
        }

        void OnPlaneUpdated(PlaneUpdatedEventArgs eventArgs)
        {
            var boundedPlane = eventArgs.Plane;
            var plane = TryGetPlane(boundedPlane.Id);
            if (plane == null)
                plane = AddPlane(boundedPlane);

            plane.boundedPlane = boundedPlane;

            if (planeUpdated != null)
                planeUpdated(new ARPlaneUpdatedEventArgs(plane));
        }

        void OnPlaneRemoved(PlaneRemovedEventArgs eventArgs)
        {
            var boundedPlane = eventArgs.Plane;
            var plane = TryGetPlane(boundedPlane.Id);

            if (plane == null)
                return;

            RemovePlane(plane);
        }

        void RemovePlane(ARPlane plane)
        {
            if (planeRemoved != null)
                planeRemoved(new ARPlaneRemovedEventArgs(plane));

            plane.OnRemove();
            m_Planes.Remove(plane.boundedPlane.Id);
        }

        Dictionary<TrackableId, ARPlane> m_Planes = new Dictionary<TrackableId, ARPlane>();

        ARSessionOrigin m_SessionOrigin;

        static List<BoundedPlane> s_BoundedPlanes = new List<BoundedPlane>();

        static HashSet<TrackableId> s_TrackableIds = new HashSet<TrackableId>();

        static List<TrackableId> s_PlanesToRemove = new List<TrackableId>();

        const PlaneDetectionFlags k_PlaneDetectionFlagEverything = (PlaneDetectionFlags)(-1);
    }
}
