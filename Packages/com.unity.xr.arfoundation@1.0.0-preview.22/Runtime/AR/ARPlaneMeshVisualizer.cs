using System;
using System.Collections.Generic;
using UnityEngine.Experimental.XR;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Generates a mesh for an <see cref="ARPlane"/>.
    /// </summary>
    /// <remarks>
    /// If this <c>GameObject</c> has a <c>MeshFilter</c> and/or <c>MeshCollider</c>,
    /// this component will generate a mesh from the underlying <c>BoundedPlane</c>.
    /// 
    /// It will also update a <c>LineRenderer</c> with the boundary points, if present.
    /// </remarks>
    [RequireComponent(typeof(ARPlane))]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/api/UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer.html")]
    public sealed class ARPlaneMeshVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Get the <c>Mesh</c> that this visualizer creates and manages.
        /// </summary>
        public Mesh mesh { get; private set; }

        /// <summary>
        /// Invoked after <seealso cref="mesh"/> has been regenerated, but before
        /// any components have been updated to use the new <c>Mesh</c>.
        /// </summary>
        public event Action<ARPlaneMeshVisualizer> meshUpdated;

        void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs eventArgs)
        {
            // Ignore subsumed planes
            if (m_Plane.boundedPlane.SubsumedById != TrackableId.InvalidId)
            {
                DisableComponents();
                return;
            }

            var center = eventArgs.center;
            var normal = eventArgs.normal;
            var polygon = eventArgs.convexBoundary;

            if (!ARPlaneMeshGenerators.GenerateMesh(mesh, m_Plane.boundedPlane.Pose, center, normal, polygon))
                return;

            if (meshUpdated != null)
                meshUpdated(this);

            var lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = polygon.Count;
                for (int i = 0; i < polygon.Count; ++i)
                {
                    lineRenderer.SetPosition(i, polygon[i]);
                }
            }

            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
                meshFilter.sharedMesh = mesh;

            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.sharedMesh = mesh;
        }

        void DisableComponents()
        {
            enabled = false;

            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.enabled = false;

            UpdateVisibility();
        }

        void SetVisible(bool visible)
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = visible;

            var lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer != null)
                lineRenderer.enabled = visible;
        }

        void UpdateVisibility()
        {
            var visible = enabled &&
                (m_Plane.trackingState != TrackingState.Unavailable) &&
                (ARSubsystemManager.systemState > ARSystemState.Ready);

            SetVisible(visible);
        }

        void OnUpdated(ARPlane plane)
        {
            UpdateVisibility();
        }

        void OnSystemStateChanged(ARSystemStateChangedEventArgs eventArgs)
        {
            UpdateVisibility();
        }

        void Awake()
        {
            mesh = new Mesh();
            m_Plane = GetComponent<ARPlane>();
        }

        void OnEnable()
        {
            m_Plane.boundaryChanged += OnBoundaryChanged;
            m_Plane.updated += OnUpdated;
            ARSubsystemManager.systemStateChanged += OnSystemStateChanged;
            UpdateVisibility();
        }

        void OnDisable()
        {
            m_Plane.boundaryChanged -= OnBoundaryChanged;
            m_Plane.updated -= OnUpdated;
            ARSubsystemManager.systemStateChanged -= OnSystemStateChanged;
            UpdateVisibility();
        }

        void Update()
        {
            if (transform.hasChanged)
            {
                var lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    if (!m_InitialLineWidthMultiplier.HasValue)
                        m_InitialLineWidthMultiplier = lineRenderer.widthMultiplier;

                    lineRenderer.widthMultiplier = m_InitialLineWidthMultiplier.Value * transform.lossyScale.x;
                }
                else
                {
                    m_InitialLineWidthMultiplier = null;
                }

                transform.hasChanged = false;
            }
        }

        float? m_InitialLineWidthMultiplier;

        ARPlane m_Plane;
    }
}
