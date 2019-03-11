using System.Collections.Generic;
using UnityEngine.Experimental.XR;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Generates a mesh for an <see cref="ARFace"/>.
    /// </summary>
    /// <remarks>
    /// If this <c>GameObject</c> has a <c>MeshFilter</c> and/or <c>MeshCollider</c>,
    /// this component will generate a mesh from the underlying <c>XRFace</c>.
    /// </remarks>
    [RequireComponent(typeof(ARFace))]
    public sealed class ARFaceMeshVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Get the <c>Mesh</c> that this visualizer creates and manages.
        /// </summary>
        public Mesh mesh { get; private set; }

        void SetVisible(bool visible)
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
            if (m_MeshRenderer == null)
            {
                return;
            }

            //if it is getting visible after being invisible for a while, set its topology
            if (visible && !m_MeshRenderer.enabled)
            {
                SetMeshTopology();
            }

            m_MeshRenderer.enabled = visible;
        }

        void SetMeshTopology()
        {
            if (mesh == null)
            {
                return;
            }

            mesh.Clear();

            if (m_Face.TryGetFaceMeshVertices(s_Vertices) && m_Face.TryGetFaceMeshIndices(s_Indices))
            {
                mesh.SetVertices(s_Vertices);
                mesh.SetTriangles(s_Indices, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
            }

            if (m_Face.TryGetFaceMeshUVs(s_UVs))
            {
                mesh.SetUVs(0, s_UVs);
            }
            
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = mesh;
            }

            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = mesh;
            }

            m_TopologyUpdatedThisFrame = true;
        }

        void UpdateVisibility()
        {
            var visible = enabled &&
                (m_Face.trackingState != TrackingState.Unavailable) &&
                (ARSubsystemManager.systemState > ARSystemState.Ready);

            SetVisible(visible);
        }

        void OnUpdated(ARFace face)
        {
             UpdateVisibility();
            if (!m_TopologyUpdatedThisFrame)
            {
                SetMeshTopology();
            }
            m_TopologyUpdatedThisFrame = false;
        }

        void OnSystemStateChanged(ARSystemStateChangedEventArgs eventArgs)
        {
            UpdateVisibility();
        }

        void Awake()
        {
            mesh = new Mesh();
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_Face = GetComponent<ARFace>();
            s_Indices = new List<int>();
            s_Vertices = new List<Vector3>();
            s_UVs = new List<Vector2>();
        }

        void OnEnable()
        {
            m_Face.updated += OnUpdated;
            ARSubsystemManager.systemStateChanged += OnSystemStateChanged;
            UpdateVisibility();
        }

        void OnDisable()
        {
            m_Face.updated -= OnUpdated;
            ARSubsystemManager.systemStateChanged -= OnSystemStateChanged;
        }

        ARFace m_Face;
        MeshRenderer m_MeshRenderer;
        bool m_TopologyUpdatedThisFrame;
        static List<Vector3> s_Vertices;
        static List<Vector2> s_UVs;
        static List<int> s_Indices;
    }
}
