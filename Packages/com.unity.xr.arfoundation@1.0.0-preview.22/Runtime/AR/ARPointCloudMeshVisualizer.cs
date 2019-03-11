using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Renders an <see cref="ARPointCloud"/> as a <c>Mesh</c> with <c>MeshTopology.Points</c>.
    /// </summary>
    [RequireComponent(typeof(ARPointCloud))]
    public sealed class ARPointCloudMeshVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Get the <c>Mesh</c> that this visualizer creates and manages.
        /// </summary>
        public Mesh mesh { get; private set; }

        void OnPointCloudChanged(ARPointCloud pointCloud)
        {
            var points = s_Vertices;
            pointCloud.GetPoints(points, Space.Self);

            mesh.Clear();
            mesh.SetVertices(points);

            var indices = new int[points.Count];
            for (int i = 0; i < points.Count; ++i)
            {
                indices[i] = i;
            }

            mesh.SetIndices(indices, MeshTopology.Points, 0);

            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
                meshFilter.sharedMesh = mesh;
        }

        void Awake()
        {
            mesh = new Mesh();
            m_PointCloud = GetComponent<ARPointCloud>();
        }

        void OnEnable()
        {
            m_PointCloud.updated += OnPointCloudChanged;
            ARSubsystemManager.systemStateChanged += OnSystemStateChanged;
            UpdateVisibility();
        }

        void OnDisable()
        {
            m_PointCloud.updated -= OnPointCloudChanged;
            ARSubsystemManager.systemStateChanged -= OnSystemStateChanged;
            UpdateVisibility();
        }

        void OnSystemStateChanged(ARSystemStateChangedEventArgs eventArgs)
        {
            UpdateVisibility();
        }

        void UpdateVisibility()
        {
            var visible = enabled &&
                (ARSubsystemManager.systemState == ARSystemState.SessionTracking);

            SetVisible(visible);
        }

        void SetVisible(bool visible)
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = visible;
        }

        ARPointCloud m_PointCloud;

        static List<Vector3> s_Vertices = new List<Vector3>();
    }
}
