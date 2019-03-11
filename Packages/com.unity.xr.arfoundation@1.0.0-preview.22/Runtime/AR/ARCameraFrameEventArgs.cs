using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A structure for camera-related information pertaining to a particular frame.
    /// This is used to communicate information in the <see cref="ARSubsystemManager.cameraFrameReceived" /> event.
    /// </summary>
    public struct ARCameraFrameEventArgs : IEquatable<ARCameraFrameEventArgs>
    {
        /// <summary>
        /// The <see cref="LightEstimationData" /> associated with this frame.
        /// </summary>
        public LightEstimationData lightEstimation { get; private set; }

        /// <summary>
        /// The time, in seconds, associated with this frame.
        /// Use <c>time.HasValue</c> to determine if this data is available.
        /// </summary>
        public float? time { get; private set; }

        /// <summary>
        /// Constructs an <see cref="ARCameraFrameEventArgs" />.
        /// </summary>
        /// <param name="lightEstimation">The <see cref="LightEstimationData" /> for the frame.</param>
        /// <param name="time">The time, in seconds, for the frame.</param>
        public ARCameraFrameEventArgs(LightEstimationData lightEstimation, float? time)
        {
            this.lightEstimation = lightEstimation;
            this.time = time;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return lightEstimation.GetHashCode() * 486187739 + time.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARCameraFrameEventArgs))
                return false;

            return Equals((ARCameraFrameEventArgs)obj);
        }

        public override string ToString()
        {
            return string.Format("(Light Estimation: {0}, Time: {1})", lightEstimation.ToString(), time);
        }

        public bool Equals(ARCameraFrameEventArgs other)
        {
            return
                (lightEstimation.Equals(other.lightEstimation)) &&
                (time == other.time);
        }

        public static bool operator ==(ARCameraFrameEventArgs lhs, ARCameraFrameEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARCameraFrameEventArgs lhs, ARCameraFrameEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
