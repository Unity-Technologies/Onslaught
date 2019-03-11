using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Holds data relevant to plane added events.
    /// </summary>
    public struct ARPlaneAddedEventArgs : IEquatable<ARPlaneAddedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARPlane"/> component that was added.
        /// </summary>
        public ARPlane plane { get; private set; }

        /// <summary>
        /// Constructor invoked by the <see cref="ARPlaneManager"/> which triggered the event.
        /// </summary>
        /// <param name="plane">The <see cref="ARPlane"/> component that was added.</param>
        public ARPlaneAddedEventArgs(ARPlane plane)
        {
            if (plane == null)
                throw new ArgumentNullException("plane");

            this.plane = plane;
        }

        public override int GetHashCode()
        {
            return plane == null ? 0 : plane.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARPlaneAddedEventArgs))
                return false;

            return Equals((ARPlaneAddedEventArgs)obj);
        }

        public override string ToString()
        {
            return plane.boundedPlane.Id.ToString();
        }

        public bool Equals(ARPlaneAddedEventArgs other)
        {
            return plane.Equals(other.plane);
        }

        public static bool operator ==(ARPlaneAddedEventArgs lhs, ARPlaneAddedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARPlaneAddedEventArgs lhs, ARPlaneAddedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

    /// <summary>
    /// Holds data relevant to plane updated events.
    /// </summary>
    public struct ARPlaneUpdatedEventArgs : IEquatable<ARPlaneUpdatedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARPlane"/> component that was updated.
        /// </summary>
        public ARPlane plane { get; private set; }

        /// <summary>
        /// Constructor invoked by the <see cref="ARPlaneManager"/> which triggered the event.
        /// </summary>
        /// <param name="plane">The <see cref="ARPlane"/> component that was updated.</param>
        public ARPlaneUpdatedEventArgs(ARPlane plane)
        {
            if (plane == null)
                throw new ArgumentNullException("plane");

            this.plane = plane;
        }

        public override int GetHashCode()
        {
            return plane == null ? 0 : plane.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARPlaneUpdatedEventArgs))
                return false;

            return Equals((ARPlaneUpdatedEventArgs)obj);
        }
        public override string ToString()
        {
            return plane.boundedPlane.Id.ToString();
        }

        public bool Equals(ARPlaneUpdatedEventArgs other)
        {
            return plane.Equals(other.plane);
        }

        public static bool operator ==(ARPlaneUpdatedEventArgs lhs, ARPlaneUpdatedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARPlaneUpdatedEventArgs lhs, ARPlaneUpdatedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

    /// <summary>
    /// Holds data relevant to plane removed events.
    /// </summary>
    public struct ARPlaneRemovedEventArgs : IEquatable<ARPlaneRemovedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARPlane"/> component that was removed.
        /// </summary>
        public ARPlane plane { get; private set; }

        /// <summary>
        /// Constructor invoked by the <see cref="ARPlaneManager"/> which triggered the event.
        /// </summary>
        /// <param name="plane">The <see cref="ARPlane"/> component that was removed.</param>
        public ARPlaneRemovedEventArgs(ARPlane plane)
        {
            if (plane == null)
                throw new ArgumentNullException("plane");

            this.plane = plane;
        }

        public override int GetHashCode()
        {
            return plane == null ? 0 : plane.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARPlaneRemovedEventArgs))
                return false;

            return Equals((ARPlaneRemovedEventArgs)obj);
        }

        public override string ToString()
        {
            return plane.boundedPlane.Id.ToString();
        }

        public bool Equals(ARPlaneRemovedEventArgs other)
        {
            return plane.Equals(other.plane);
        }

        public static bool operator ==(ARPlaneRemovedEventArgs lhs, ARPlaneRemovedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARPlaneRemovedEventArgs lhs, ARPlaneRemovedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

    /// <summary>
    /// Data associated with an <see cref="ARPlane.boundaryChanged" /> event.
    /// </summary>
    public struct ARPlaneBoundaryChangedEventArgs : IEquatable<ARPlaneBoundaryChangedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARPlane" /> which triggered the event.
        /// </summary>
        public ARPlane plane { get; set; }

        /// <summary>
        /// The center of the <see cref="ARPlane" />, in plane-relative space.
        /// </summary>
        public Vector3 center { get; set; }

        /// <summary>
        /// The normal of the <see cref="ARPlane" />, in plane-relative space.
        /// </summary>
        public Vector3 normal { get; set; }

        /// <summary>
        /// The boundary points of the <see cref="ARPlane" />, in plane-relative space.
        /// </summary>
        /// <remarks>
        /// The boundary points are always convex. 
        /// </remarks>
        public List<Vector3> convexBoundary { get; private set; }

        /// <summary>
        /// Constructor for plane changed events.
        /// This is normally only used by the <see cref="ARPlane"/> component for <see cref="ARPlane.boundaryChanged"/> events.
        /// </summary>
        /// <param name="plane">The <see cref="ARPlane"/> that triggered the event.</param>
        /// <param name="center">The center of the plane, in plane-relative space.</param>
        /// <param name="normal">The normal of the plane, in plane-relative space.</param>
        /// <param name="convexBoundary">The convex boundary points, in plane-relative space. This may not be <c>null</c>.</param>
        public ARPlaneBoundaryChangedEventArgs(ARPlane plane, Vector3 center, Vector3 normal, List<Vector3> convexBoundary)
        {
            if (convexBoundary == null)
                throw new ArgumentNullException("convexBoundary");

            this.plane = plane;
            this.center = center;
            this.normal = normal;
            this.convexBoundary = convexBoundary;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (((plane == null ? 0 : plane.GetHashCode()) * 486187739 + center.GetHashCode()) * 486187739 + normal.GetHashCode()) * 486187739 + convexBoundary.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARPlaneBoundaryChangedEventArgs))
                return false;

            return Equals((ARPlaneBoundaryChangedEventArgs)obj);
        }

        public override string ToString()
        {
            return plane.boundedPlane.Id.ToString();
        }

        public bool Equals(ARPlaneBoundaryChangedEventArgs other)
        {
            if ((plane != other.plane) || (center != other.center) || (normal != other.normal))
                return false;

            if (convexBoundary == other.convexBoundary)
                return true;

            // One is null and the other is not
            if ((convexBoundary == null) || (other.convexBoundary == null))
                return false;
    
            // Everything else is equal and both boundaries are non-null
            if (convexBoundary.Count != other.convexBoundary.Count)
                return false;

            // Everything the same so far. Check each vertex.
            var count = convexBoundary.Count;
            bool allEqual = true;
            for (int i = 0; i < count; ++i)
            {
                allEqual &= (convexBoundary[i] == other.convexBoundary[i]);
            }

            return allEqual;
        }

        public static bool operator ==(ARPlaneBoundaryChangedEventArgs lhs, ARPlaneBoundaryChangedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARPlaneBoundaryChangedEventArgs lhs, ARPlaneBoundaryChangedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
