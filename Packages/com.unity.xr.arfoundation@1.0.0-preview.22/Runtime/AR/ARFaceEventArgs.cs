using System;
using System.Collections.Generic;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Holds data relevant to face added events.
    /// </summary>
    public struct ARFaceAddedEventArgs : IEquatable<ARFaceAddedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARFace"/> component that was added.
        /// </summary>
        public ARFace face { get; private set; }

        /// <summary>
        /// Constructor invoked by the <see cref="ARFaceManager"/> which triggered the event.
        /// </summary>
        /// <param name="face">The <see cref="ARFace"/> component that was added.</param>
        public ARFaceAddedEventArgs(ARFace face)
        {
            if (face == null)
                throw new ArgumentNullException("face");

            this.face = face;
        }

        public override int GetHashCode()
        {
            return face.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARFaceAddedEventArgs))
                return false;

            return Equals((ARFaceAddedEventArgs)obj);
        }

        public bool Equals(ARFaceAddedEventArgs other)
        {
            return face.Equals(other.face);
        }

        public static bool operator==(ARFaceAddedEventArgs lhs, ARFaceAddedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(ARFaceAddedEventArgs lhs, ARFaceAddedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

    /// <summary>
    /// Holds data relevant to face updated events.
    /// </summary>
    public struct ARFaceUpdatedEventArgs : IEquatable<ARFaceUpdatedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARFace"/> component that was updated.
        /// </summary>
        public ARFace face { get; private set; }

        /// <summary>
        /// Constructor invoked by the <see cref="ARFaceManager"/> which triggered the event.
        /// </summary>
        /// <param name="face">The <see cref="ARFace"/> component that was updated.</param>
        public ARFaceUpdatedEventArgs(ARFace face)
        {
            if (face == null)
                throw new ArgumentNullException("face");

            this.face = face;
        }

        public override int GetHashCode()
        {
            return face.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARFaceUpdatedEventArgs))
                return false;

            return Equals((ARFaceUpdatedEventArgs)obj);
        }

        public bool Equals(ARFaceUpdatedEventArgs other)
        {
            return face.Equals(other.face);
        }

        public static bool operator==(ARFaceUpdatedEventArgs lhs, ARFaceUpdatedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(ARFaceUpdatedEventArgs lhs, ARFaceUpdatedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }

    /// <summary>
    /// Holds data relevant to face removed events.
    /// </summary>
    public struct ARFaceRemovedEventArgs : IEquatable<ARFaceRemovedEventArgs>
    {
        /// <summary>
        /// The <see cref="ARFace"/> component that was removed.
        /// </summary>
        public ARFace face { get; private set; }

        /// <summary>
        /// Constructor invoked by the <see cref="ARFaceManager"/> which triggered the event.
        /// </summary>
        /// <param name="face">The <see cref="ARFace"/> component that was removed.</param>
        public ARFaceRemovedEventArgs(ARFace face)
        {
            if (face == null)
                throw new ArgumentNullException("face");

            this.face = face;
        }

        public override int GetHashCode()
        {
            return face.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARFaceRemovedEventArgs))
                return false;

            return Equals((ARFaceRemovedEventArgs)obj);
        }

        public bool Equals(ARFaceRemovedEventArgs other)
        {
            return face.Equals(other.face);
        }

        public static bool operator==(ARFaceRemovedEventArgs lhs, ARFaceRemovedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(ARFaceRemovedEventArgs lhs, ARFaceRemovedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
