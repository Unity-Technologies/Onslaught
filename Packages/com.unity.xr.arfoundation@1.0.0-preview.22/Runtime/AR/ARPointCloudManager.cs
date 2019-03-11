using System;
using UnityEngine.Experimental.XR;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Creates and updates a <c>GameObject</c> under the <see cref="ARSessionOrigin"/>'s TrackablesParent
    /// to represent a point cloud.
    /// 
    /// When enabled, this component subscribes to <see cref="ARSubsystemManager.pointCloudUpdated"/> event.
    /// If this component is disabled, and there are no other subscribers to that event,
    /// point clouds will be disabled.
    /// </summary>
    [RequireComponent(typeof(ARSessionOrigin))]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/api/UnityEngine.XR.ARFoundation.ARPointCloudManager.html")]
    public sealed class ARPointCloudManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for the created point cloud.")]
        GameObject m_PointCloudPrefab;

        /// <summary>
        /// Getter/setter for the Point Cloud Prefab.
        /// </summary>
        public GameObject pointCloudPrefab
        {
            get { return m_PointCloudPrefab; }
            set { m_PointCloudPrefab = value; }
        }

        /// <summary>
        /// Getter for the instantiated <seealso cref="ARPointCloud"/>.
        /// </summary>
        public ARPointCloud pointCloud { get; private set; }

        /// <summary>
        /// Raised each time the <see cref="ARPointCloud"/> is updated.
        /// </summary>
        public event Action<ARPointCloudUpdatedEventArgs> pointCloudUpdated;

        void Awake()
        {
            m_SessionOrigin = GetComponent<ARSessionOrigin>();
        }

        void OnEnable()
        {
            ARSubsystemManager.pointCloudUpdated += OnPointCloudUpdated;
            ARSubsystemManager.sessionDestroyed += OnSessionDestroyed;
        }

        void OnDisable()
        {
            ARSubsystemManager.pointCloudUpdated -= OnPointCloudUpdated;
            ARSubsystemManager.sessionDestroyed -= OnSessionDestroyed;
        }

        void OnSessionDestroyed()
        {
            if (pointCloud != null)
            {
                Destroy(pointCloud.gameObject);
                pointCloud = null;
            }
        }

        void OnPointCloudUpdated(PointCloudUpdatedEventArgs eventArgs)
        {
            if (pointCloud == null)
            {
                GameObject newGameObject;
                if (pointCloudPrefab != null)
                {
                    newGameObject = Instantiate(pointCloudPrefab, m_SessionOrigin.trackablesParent);
                    newGameObject.transform.localPosition = Vector3.zero;
                    newGameObject.transform.localRotation = Quaternion.identity;
                    newGameObject.transform.localScale = Vector3.one;
                }
                else
                {
                    newGameObject = new GameObject();
                    newGameObject.transform.SetParent(m_SessionOrigin.trackablesParent, false);
                    newGameObject.layer = gameObject.layer;
                }

                pointCloud = newGameObject.GetComponent<ARPointCloud>();
                if (pointCloud == null)
                    pointCloud = newGameObject.AddComponent<ARPointCloud>();
            }

            pointCloud.OnUpdated();

            if (pointCloudUpdated != null)
                pointCloudUpdated(new ARPointCloudUpdatedEventArgs(pointCloud));
        }

        ARSessionOrigin m_SessionOrigin;
    }
}
