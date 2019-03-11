using System;
using System.Collections.Generic;
using UnityEngine.Experimental.XR;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Represents a detected point cloud, aka feature points.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/api/UnityEngine.XR.ARFoundation.ARPointCloud.html")]
    public class ARPointCloud : MonoBehaviour
    {
        /// <summary>
        /// Invoked when the point cloud is updated.
        /// </summary>
        public event Action<ARPointCloud> updated;

        /// <summary>
        /// The last frame during which the point cloud was updated.
        /// </summary>
        /// <remarks>
        /// This is consistent with the value you get from <c>Time.frameCount</c>
        /// </remarks>
        public int lastUpdatedFrame
        {
            get
            {
                return depthSubsystem.LastUpdatedFrame;
            }
        }

        /// <summary>
        /// Replaces the contents of <paramref name="points"/> with the feature points in Unity world space.
        /// </summary>
        /// <param name="points">A <c>List</c> of <c>Vector3</c>s. The contents are replaced with the current point cloud.</param>
        /// <param name="space">Which coordinate system to use. <c>Space.Self</c> refers to session space,
        /// while <c>Space.World</c> refers to Unity world space. The default is <c>Space.World</c>.</param>
        public void GetPoints(List<Vector3> points, Space space = Space.World)
        {
            depthSubsystem.GetPoints(points);

            if (space == Space.World)
                transform.parent.TransformPointList(points);
        }

        /// <summary>
        /// Gets the confidence values for each point in the point cloud.
        /// </summary>
        /// <param name="confidence">A <c>List</c> of <c>float</c>s representing the confidence values for each point
        /// in the point cloud. The contents are replaced with the current confidence values.</param>
        public void GetConfidence(List<float> confidence)
        {
            depthSubsystem.GetConfidence(confidence);
        }

        /// <summary>
        /// The XR Subsystem providing the point cloud data.
        /// </summary>
        XRDepthSubsystem depthSubsystem
        {
            get { return ARSubsystemManager.depthSubsystem; }
        }

        internal void OnUpdated()
        {
            if (updated != null)
                updated(this);
        }
    }
}
