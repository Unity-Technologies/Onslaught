using System;
using UnityEngine.Experimental.XR;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Represents a Reference Point (aka anchor) tracked by an AR device.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/api/UnityEngine.XR.ARFoundation.ARReferencePoint.html")]
    public sealed class ARReferencePoint : MonoBehaviour
    {
        /// <summary>
        /// Invoked whenever this reference point is updated.
        /// </summary>
        public event Action<ARReferencePoint> updated;

        /// <summary>
        /// The raw data associated with the reference point.
        /// </summary>
        public ReferencePoint sessionRelativeData
        {
            get { return m_Data; }
            internal set { SetData(value); }
        }

        /// <summary>
        /// The last frame on which this reference point was updated.
        /// </summary>
        public int lastUpdatedFrame { get; private set; }

        void SetData(ReferencePoint referencePointData)
        {
            m_Data = referencePointData;
            var pose = referencePointData.Pose;
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
            lastUpdatedFrame = Time.frameCount;

            if (updated != null)
                updated(this);
        }

        ReferencePoint m_Data;
    }
}
