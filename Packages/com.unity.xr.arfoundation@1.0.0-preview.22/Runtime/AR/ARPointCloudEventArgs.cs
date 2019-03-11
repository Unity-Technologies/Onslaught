using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Arguments for the <see cref="ARPointCloudManager.pointCloudUpdated"/> event.
    /// </summary>
    public struct ARPointCloudUpdatedEventArgs : IEquatable<ARPointCloudUpdatedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARPointCloud"/> whose update triggered this event.
        /// </summary>
        public ARPointCloud pointCloud { get; private set; }

        /// <summary>
        /// Constructor for the <see cref="ARPointCloudUpdatedEventArgs"/>. This is normally only used by an <see cref="ARSessionOrigin"/>.
        /// </summary>
        /// <param name="pointCloud">The <see cref="ARPointCloud"/> whose update triggered this event.</param>
        public ARPointCloudUpdatedEventArgs(ARPointCloud pointCloud)
        {
            this.pointCloud = pointCloud;
        }

        public override int GetHashCode()
        {
            if (pointCloud == null)
                return 0;

            return pointCloud.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARPointCloudUpdatedEventArgs))
                return false;

            return Equals((ARPointCloudUpdatedEventArgs)obj);
        }

        public override string ToString()
        {
            return pointCloud.GetInstanceID().ToString();
        }

        public bool Equals(ARPointCloudUpdatedEventArgs other)
        {
            return pointCloud.Equals(other.pointCloud);
        }

        public static bool operator ==(ARPointCloudUpdatedEventArgs lhs, ARPointCloudUpdatedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARPointCloudUpdatedEventArgs lhs, ARPointCloudUpdatedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
