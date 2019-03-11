using UnityEngine.XR.ARExtensions;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Add this component to a <c>Camera</c> to set configuration options for the physical camera used by an AR device.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/api/UnityEngine.XR.ARFoundation.ARCameraOptions.html")]
    public sealed class ARCameraOptions : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The focus mode to request on the (physical) AR camera.")]
        CameraFocusMode m_FocusMode = CameraFocusMode.Auto;

        /// <summary>
        /// Get or set the <c>CameraFocusMode</c> to use on the camera
        /// </summary>
        public CameraFocusMode focusMode
        {
            get { return m_FocusMode; }
            set
            {
                m_FocusMode = value;
                if (enabled)
                    ARSubsystemManager.cameraFocusMode = focusMode;
            }
        }

        [SerializeField]
        [Tooltip("When enabled, requests that light estimation information be made available.")]
        bool m_LightEstimation = false;

        /// <summary>
        /// Get or set whether light estimation information be made available (if possible).
        /// </summary>
        public bool lightEstimation
        {
            get { return m_LightEstimation; }
            set
            {
                m_LightEstimation = value;
                if (enabled)
                    ARSubsystemManager.lightEstimationRequested = value;
            }
        }

        void OnEnable()
        {
            ARSubsystemManager.cameraFocusMode = focusMode;
            ARSubsystemManager.lightEstimationRequested = lightEstimation;
        }

        void OnDisable()
        {
            ARSubsystemManager.lightEstimationRequested = false;
        }
    }
}
