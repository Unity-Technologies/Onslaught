using System;
using System.Collections.Generic;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.FaceSubsystem;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Creates, updates, and removes <c>GameObject</c>s with <see cref="ARFace"/> components under the <see cref="ARSessionOrigin"/>'s <see cref="ARSessionOrigin.trackablesParent"/>.
    /// </summary>
    /// <remarks>
    /// When enabled, this component subscribes to <see cref="ARSubsystemManager.faceAdded"/>,
    /// <see cref="ARSubsystemManager.faceUpdated"/>, and <see cref="ARSubsystemManager.faceRemoved"/>.
    /// If this component is disabled, and there are no other subscribers to those events,
    /// face detection will be disabled on the device.
    /// </remarks>
    [RequireComponent(typeof(ARSessionOrigin))]
    [DisallowMultipleComponent]
    public sealed class ARFaceManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each created face.")]
        GameObject m_FacePrefab;

        /// <summary>
        /// Getter/setter for the Face Prefab.
        /// </summary>
        public GameObject facePrefab
        {
            get { return m_FacePrefab; }
            set { m_FacePrefab = value; }
        }

        /// <summary>
        /// Raised for each new <see cref="ARFace"/> detected in the environment.
        /// </summary>
        public event Action<ARFaceAddedEventArgs> faceAdded;

        /// <summary>
        /// Raised for each <see cref="ARFace"/> every time it updates.
        /// </summary>
        public event Action<ARFaceUpdatedEventArgs> faceUpdated;

        /// <summary>
        /// Raised whenever an <see cref="ARFace"/> is removed.
        /// </summary>
        public event Action<ARFaceRemovedEventArgs> faceRemoved;

        /// <summary>
        /// Get the number of faces managed by this manager.
        /// </summary>
        public int faceCount
        {
            get { return m_Faces.Count; }
        }

        /// <summary>
        /// Get all currently tracked <see cref="ARFace"/>s.
        /// </summary>
        /// <param name="faces">Replaces the contents with the current list of faces.</param>
        public void GetAllFaces(List<ARFace> faces)
        {
            if (faces == null)
                throw new ArgumentNullException("faces");

            faces.Clear();
            foreach (var kvp in m_Faces)
            {
                faces.Add(kvp.Value);
            }
        }

        /// <summary>
        /// Attempts to retrieve an <see cref="ARFace"/>.
        /// </summary>
        /// <param name="faceId">The <c>TrackableId</c> associated with the <see cref="ARFace"/>.</param>
        /// <returns>The <see cref="ARFace"/>if found. <c>null</c> otherwise.</returns>
        public ARFace TryGetFace(TrackableId faceId)
        {
            ARFace face;
            m_Faces.TryGetValue(faceId, out face);

            return face;
        }

        void Awake()
        {
            m_SessionOrigin = GetComponent<ARSessionOrigin>();
            m_Faces = new Dictionary<TrackableId, ARFace>();
        }

        void OnEnable()
        {
            SyncFaces();
            ARSubsystemManager.faceAdded += OnFaceAdded;
            ARSubsystemManager.faceUpdated += OnFaceUpdated;
            ARSubsystemManager.faceRemoved += OnFaceRemoved;
        }

        void OnDisable()
        {
            ARSubsystemManager.faceAdded -= OnFaceAdded;
            ARSubsystemManager.faceUpdated -= OnFaceUpdated;
            ARSubsystemManager.faceRemoved -= OnFaceRemoved;
        }

        void SyncFaces()
        {
            var faceSubsystem = ARSubsystemManager.faceSubsystem;
            if (faceSubsystem == null)
                return;

            s_XRFaces.Clear();

            if (!faceSubsystem.TryGetAllFaces(s_XRFaces))
            {
                return;
            }

            // Check for added/updated faces
            s_TrackableIds.Clear();
            foreach (var xrFace in s_XRFaces)
            {
                var faceId = xrFace.trackableId;

                ARFace face;
                if (m_Faces.TryGetValue(faceId, out face))
                {
                    face.xrFace = xrFace;
                    if (faceUpdated != null)
                        faceUpdated(new ARFaceUpdatedEventArgs(face));
                }
                else
                {
                    face = AddFace(xrFace);
                    face.xrFace = xrFace;
                    if (faceAdded != null)
                        faceAdded(new ARFaceAddedEventArgs(face));
                }

                s_TrackableIds.Add(faceId);
            }

            // Check for removed faces
            s_FacesToRemove.Clear();
            foreach (var kvp in m_Faces)
            {
                var faceId = kvp.Key;
                if (!s_TrackableIds.Contains(faceId))
                    s_FacesToRemove.Add(faceId);
            }

            foreach (var id in s_FacesToRemove)
                RemoveFace(m_Faces[id]);
        }

        GameObject CreateGameObject()
        {
            if (facePrefab != null)
                return Instantiate(facePrefab, m_SessionOrigin.trackablesParent);

            var go = new GameObject();
            go.transform.SetParent(m_SessionOrigin.trackablesParent, false);
            return go;
        }

        ARFace AddFace(XRFace xrFace)
        {
            var go = CreateGameObject();
            go.layer = gameObject.layer;
            var face = go.GetComponent<ARFace>();
            if (face == null)
                face = go.AddComponent<ARFace>();

            m_Faces.Add(xrFace.trackableId, face);

            return face;
        }

        void OnFaceAdded(FaceAddedEventArgs eventArgs)
        {
            var xrFace = eventArgs.xrFace;
            var face = AddFace(xrFace);
            face.xrFace = xrFace;

            if (faceAdded != null)
                faceAdded(new ARFaceAddedEventArgs(face));
        }

        void OnFaceUpdated(FaceUpdatedEventArgs eventArgs)
        {
            var xrFace = eventArgs.xrFace;
            var face = TryGetFace(xrFace.trackableId);
            if (face == null)
                face = AddFace(xrFace);

            face.xrFace = xrFace;

            if (faceUpdated != null)
                faceUpdated(new ARFaceUpdatedEventArgs(face));
        }

        void OnFaceRemoved(FaceRemovedEventArgs eventArgs)
        {
            var xrFace = eventArgs.xrFace;
            var face = TryGetFace(xrFace.trackableId);

            if (face == null)
                return;

            RemoveFace(face);
        }

        void RemoveFace(ARFace face)
        {
            if (faceRemoved != null)
                faceRemoved(new ARFaceRemovedEventArgs(face));

            face.OnRemove();
            m_Faces.Remove(face.xrFace.trackableId);
        }

        Dictionary<TrackableId, ARFace> m_Faces;

        ARSessionOrigin m_SessionOrigin;

        static List<XRFace> s_XRFaces = new List<XRFace>();

        static HashSet<TrackableId> s_TrackableIds = new HashSet<TrackableId>();

        static List<TrackableId> s_FacesToRemove = new List<TrackableId>();
    }
}
