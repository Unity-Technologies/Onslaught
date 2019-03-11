using System.Collections;
using UnityEngine.SceneManagement;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Controls the lifecycle and configuration options for an AR session. There
    /// is only one active session. If you have multiple <see cref="ARSession"/> components,
    /// they all talk to the same session and will conflict with each other.
    /// 
    /// Enabling or disabling the <see cref="ARSession"/> will start or stop the session,
    /// respectively.
    /// </summary>
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/api/UnityEngine.XR.ARFoundation.ARSession.html")]
    public class ARSession : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If enabled, the session will attempt to update a supported device if its AR software is out of date.")]
        bool m_AttemptUpdate = true;

        /// <summary>
        /// If the device supports AR but does not have the necessary software, some platforms
        /// allow prompting the user to install or update the software. If <see cref="attemptUpdate"/>
        /// is <c>true</c>, a software update will be attempted. If the appropriate software is not installed
        /// or out of date, and <see cref="attemptUpdate"/> is <c>false</c>, then AR will not be available.
        /// </summary>
        public bool attemptUpdate
        {
            get { return m_AttemptUpdate; }
            set { m_AttemptUpdate = value; }
        }

        /// <summary>
        /// Resets the AR Session. This destroys the current session, including all trackables, and
        /// then establishes a new session.
        /// </summary>
        public void Reset()
        {
            if (ARSubsystemManager.systemState < ARSystemState.Ready)
                return;

            ARSubsystemManager.StopSubsystems();
            ARSubsystemManager.DestroySubsystems();
            ARSubsystemManager.CreateSubsystems();
            ARSubsystemManager.StartSubsystems();
        }

        /// <summary>
        /// Emits a warning in the console if more than one active <see cref="ARSession"/>
        /// component is active. There is only a single, global AR Session; this
        /// component controls that session. If two or more <see cref="ARSession"/>s are
        /// simultaneously active, then they both issue commands to the same session.
        /// Although this can cause unintended behavior, it is not expressly forbidden.
        ///
        /// This method is expensive and should not be called frequently.
        /// </summary>
        void WarnIfMultipleARSessions()
        {
            var sessions = FindObjectsOfType<ARSession>();
            if (sessions.Length > 1)
            {
                // Compile a list of session names
                string sessionNames = "";
                foreach (var session in sessions)
                {
                    sessionNames += string.Format("\t{0}\n", session.name);
                }

                Debug.LogWarningFormat(
                    "Multiple active AR Sessions found. " +
                    "These will conflict with each other, so " +
                    "you should only have one active ARSession at a time. " +
                    "Found these active sessions:\n{0}", sessionNames);
            }
        }

        void OnEnable()
        {
#if DEBUG
            WarnIfMultipleARSessions();
#endif
            ARSubsystemManager.CreateSubsystems();
            StartCoroutine(Initialize());
        }

        IEnumerator Initialize()
        {
            // Make sure we've checked for availability
            if (ARSubsystemManager.systemState <= ARSystemState.CheckingAvailability)
                yield return ARSubsystemManager.CheckAvailability();

            // Make sure we didn't get disabled while checking for availability
            if (!enabled)
                yield break;

            // Complete install if necessary
            if (((ARSubsystemManager.systemState == ARSystemState.NeedsInstall) && attemptUpdate) ||
                (ARSubsystemManager.systemState == ARSystemState.Installing))
            {
                yield return ARSubsystemManager.Install();
            }

            // If we're still enabled and everything is ready, then start.
            if (ARSubsystemManager.systemState == ARSystemState.Ready && enabled)
            {
                ARSubsystemManager.StartSubsystems();
            }
            else
            {
                enabled = false;
            }
        }

        void OnDisable()
        {
            ARSubsystemManager.StopSubsystems();
        }

        void OnDestroy()
        {
            ARSubsystemManager.DestroySubsystems();
        }
    }
}
