using System;
using UnityEngine.Experimental.XR;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Holds data relevant to reference point updated events.
    /// </summary>
    /// <remarks>
    /// The <see cref="ARReferencePointManager"/> uses this struct to pass data to
    /// subscribers of its <see cref="ARReferencePointManager.referencePointUpdated"/> event.
    /// </remarks>
    public struct ARReferencePointUpdatedEventArgs : IEquatable<ARReferencePointUpdatedEventArgs>
    {
        /// <summary>
        /// The reference point component which was updated.
        /// </summary>
        public ARReferencePoint referencePoint { get; private set; }

        /// <summary>
        /// The previous tracking state of the reference point, prior to this update.
        /// </summary>
        public TrackingState previousTrackingState { get; private set; }

        /// <summary>
        /// The pose of the reference point prior to this update, in local (session) space.
        /// </summary>
        public Pose previousSessionRelativePose { get; private set; }

        /// <summary>
        /// The pose of the reference point prior to this update, in Unity world space.
        /// </summary>
        public Pose previousPose
        {
            get
            {
                var parentTransform = referencePoint.transform.parent;
                if (parentTransform == null)
                    return previousSessionRelativePose;

                return parentTransform.TransformPose(previousSessionRelativePose);
            }
        }

        /// <summary>
        /// Constructor invoked by the <see cref="ARReferencePointManager"/> which triggered this event.
        /// </summary>
        /// <param name="referencePoint">The reference point component that was updated.</param>
        /// <param name="previousTrackingState">The tracking state prior to this update.</param>
        /// <param name="previousPose">The session-space pose prior to this update.</param>
        public ARReferencePointUpdatedEventArgs(
            ARReferencePoint referencePoint,
            TrackingState previousTrackingState,
            Pose previousPose)
        {
            if (referencePoint == null)
                throw new ArgumentNullException("referencePoint");

            this.referencePoint = referencePoint;
            this.previousTrackingState = previousTrackingState;
            previousSessionRelativePose = previousPose;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((referencePoint == null ? 0 : referencePoint.GetHashCode()) * 486187739 + previousTrackingState.GetHashCode()) * 486187739 + previousSessionRelativePose.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARReferencePointUpdatedEventArgs))
                return false;

            return Equals((ARReferencePointUpdatedEventArgs)obj);
        }

        public override string ToString()
        {
            return string.Format("(ReferencePoint {0}: Transform: {1}, Tracking State Change: {2} => {3})",
                referencePoint.sessionRelativeData.Id,
                referencePoint.transform,
                previousTrackingState,
                referencePoint.sessionRelativeData.TrackingState
                );
        }

        public bool Equals(ARReferencePointUpdatedEventArgs other)
        {
            return
                (referencePoint.Equals(other.referencePoint)) &&
                (previousTrackingState == other.previousTrackingState) &&
                (previousSessionRelativePose.Equals(other.previousSessionRelativePose));
        }

        public static bool operator ==(ARReferencePointUpdatedEventArgs lhs, ARReferencePointUpdatedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARReferencePointUpdatedEventArgs lhs, ARReferencePointUpdatedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
