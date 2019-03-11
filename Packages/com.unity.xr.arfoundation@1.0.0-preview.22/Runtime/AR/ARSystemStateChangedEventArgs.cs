using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Container for SystemState event arguments. Used by the <see cref="ARSubsystemManager"/>.
    /// </summary>
    public struct ARSystemStateChangedEventArgs : IEquatable<ARSystemStateChangedEventArgs>
    {
        /// <summary>
        /// The new session state.
        /// </summary>
        public ARSystemState state { get; private set; }

        /// <summary>
        /// Constructor for these event arguments.
        /// </summary>
        /// <param name="state">The new session state.</param>
        public ARSystemStateChangedEventArgs(ARSystemState state)
        {
            this.state = state;
        }

        public override int GetHashCode()
        {
            return state.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARSystemStateChangedEventArgs))
                return false;

            return Equals((ARSystemStateChangedEventArgs)obj);
        }

        public override string ToString()
        {
            return state.ToString();
        }

        public bool Equals(ARSystemStateChangedEventArgs other)
        {
            return state == other.state;
        }

        public static bool operator ==(ARSystemStateChangedEventArgs lhs, ARSystemStateChangedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARSystemStateChangedEventArgs lhs, ARSystemStateChangedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
