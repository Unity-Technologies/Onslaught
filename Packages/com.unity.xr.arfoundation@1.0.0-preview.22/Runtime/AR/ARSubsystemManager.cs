using System;
using System.Collections;
using UnityEngine.Experimental;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARExtensions;
using UnityEngine.XR.FaceSubsystem;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Manages the lifecycle of multiple XR Subsystems specific to AR.
    /// </summary>
    /// <remarks>
    /// The XR Subsystems provide direct access to the underlying data providers for a specific device.
    /// ARFoundation provides higher level abstractions and utilities on top of the low-level XR Subsystems,
    /// so, in general, you don't need to interact directly with the XR Subsystems.
    ///
    /// A typical AR session may involve the following subsystems:
    /// <list type="number">
    /// <item><description><c>XRSessionSubsystem</c></description></item>
    /// <item><description><c>XRInputSubsystem</c></description></item>
    /// <item><description><c>XRCameraSubsystem</c></description></item>
    /// <item><description><c>XRDepthSubsystem</c></description></item>
    /// <item><description><c>XRPlaneSubsystem</c></description></item>
    /// <item><description><c>XRReferencePointSubsystem</c></description></item>
    /// <item><description><c>XRRaycastSubsystem</c></description></item>
    /// </list>
    /// Since there can only be a single AR session (and usually the associated
    /// subsystems), this class is a singleton.
    /// </remarks>
    public static class ARSubsystemManager
    {
        /// <summary>
        /// Gets the <c>XRSessionSubsystem</c>. This controls the lifecycle of the AR Session.
        /// </summary>
        public static XRSessionSubsystem sessionSubsystem { get; private set; }

        /// <summary>
        /// Gets the <c>XRInputSubsystem</c>. This allows <c>Pose</c> data from the device to be fed to the <c>TrackedPoseDriver</c>.
        /// </summary>
        public static XRInputSubsystem inputSubsystem { get; private set; }

        /// <summary>
        /// Gets the <c>XRCameraSubsystem</c>. This subsystem provides access to camera data, such as ligt estimation information and the camera texture.
        /// </summary>
        public static XRCameraSubsystem cameraSubsystem { get; private set; }

        /// <summary>
        /// Gets the <c>XRDepthSubsystem</c>. This subsystem provides access to depth data, such as features points (aka point cloud).
        /// </summary>
        public static XRDepthSubsystem depthSubsystem { get; private set; }

        /// <summary>
        /// Gets the <c>XRPlaneSubsystem</c>. This subsystem provides access to plane data, such as horizontal surfaces.
        /// </summary>
        public static XRPlaneSubsystem planeSubsystem { get; private set; }

        /// <summary>
        /// Gets the <c>XRReferencePointSubsystem</c>. This subystem provides access to reference points, aka anchors.
        /// </summary>
        public static XRReferencePointSubsystem referencePointSubsystem { get; private set; }

        /// <summary>
        /// Gets the <c>XRRaycastSubsystem</c>. This subsystem provides access to the raycast interface.
        /// </summary>
        public static XRRaycastSubsystem raycastSubsystem { get; private set; }

        /// <summary>
        /// Gets the <c>XRFaceSubsystem</c>. This subsystem provides access to the face tracking interface.
        /// </summary>
        public static XRFaceSubsystem faceSubsystem { get; private set; }
        /// <summary>
        /// Allows to filter subsystem ids and initialize first one containing specified string.
        /// </summary>
        public static string subsystemFilter { get; set; }

        /// <summary>
        /// This event is invoked whenever a new camera frame is provided by the device.
        /// </summary>
        public static event Action<ARCameraFrameEventArgs> cameraFrameReceived
        {
            add
            {
                s_CameraFrameReceived += value;
                UpdateCameraSubsystem();
            }

            remove
            {
                s_CameraFrameReceived -= value;
                UpdateCameraSubsystem();
            }
        }

        /// <summary>
        /// This event is invoked whenever the <see cref="sessionSubsystem"/> is destroyed.
        /// </summary>
        public static event Action sessionDestroyed;

        /// <summary>
        /// This event is invoked whenever the <see cref="systemState"/> changes.
        /// </summary>
        public static event Action<ARSystemStateChangedEventArgs> systemStateChanged;

        /// <summary>
        /// This event is invoked whenever a plane is added.
        /// </summary>
        /// <remarks>
        /// This is the low-level XR interface, and the data is in session space.
        /// Consider instead subscribing to the more useful <see cref="ARPlaneManager.planeAdded"/>.
        ///
        /// Plane detection is disabled if there are no subscribers to at least one
        /// of <see cref="planeAdded"/>, <see cref="planeUpdated"/>, or <see cref="planeRemoved"/>.
        /// </remarks>
        public static event Action<PlaneAddedEventArgs> planeAdded
        {
            add
            {
                s_PlaneAdded += value;
                UpdatePlaneDetection();
            }

            remove
            {
                s_PlaneAdded -= value;
                UpdatePlaneDetection();
            }
        }


        /// <summary>
        /// This event is invoked whenever an existing plane is updated.
        /// </summary>
        /// <remarks>
        /// This is the low-level XR interface, and the data is in session space.
        /// Consider instead subscribing to the more useful <see cref="ARPlaneManager.planeUpdated"/>.
        ///
        /// Plane detection is disabled if there are no subscribers to at least one
        /// of <see cref="planeAdded"/>, <see cref="planeUpdated"/>, or <see cref="planeRemoved"/>.
        /// </remarks>
        public static event Action<PlaneUpdatedEventArgs> planeUpdated
        {
            add
            {
                s_PlaneUpdated += value;
                UpdatePlaneDetection();
            }

            remove
            {
                s_PlaneUpdated -= value;
                UpdatePlaneDetection();
            }
        }

        /// <summary>
        /// This event is invoked whenever an existing plane is removed.
        /// </summary>
        /// <remarks>
        /// This is the low-level XR interface, and the data is in session space.
        /// Consider instead subscribing to the more useful <see cref="ARPlaneManager.planeRemoved"/>.
        ///
        /// Plane detection is disabled if there are no subscribers to at least one
        /// of <see cref="planeAdded"/>, <see cref="planeUpdated"/>, or <see cref="planeRemoved"/>.
        /// </remarks>
        public static event Action<PlaneRemovedEventArgs> planeRemoved
        {
            add
            {
                s_PlaneRemoved += value;
                UpdatePlaneDetection();
            }

            remove
            {
                s_PlaneRemoved -= value;
                UpdatePlaneDetection();
            }
        }

        /// <summary>
        /// Get or set the plane detection flags, used to specify which
        /// type of plane detection to enable, e.g., horizontal, vertical, or both.
        /// </summary>
        public static PlaneDetectionFlags planeDetectionFlags
        {
            get { return s_PlaneDetectionFlags; }
            set
            {
                if (s_PlaneDetectionFlags == value)
                    return;

                s_PlaneDetectionFlags = value;
                if (planeSubsystem != null)
                    planeSubsystem.TrySetPlaneDetectionFlags(s_PlaneDetectionFlags);
            }
        }

        /// <summary>
        /// This event is invoked whenever the point cloud has changed.
        /// </summary>
        /// <remarks>
        /// This is the low-level XR interface, and the data is in session space.
        /// Consider instead subscribing to the more useful <see cref="ARPointCloudManager.pointCloudUpdated"/>.
        ///
        /// Point clouds are disabled if there are no subscribes to this event.
        /// </remarks>
        public static event Action<PointCloudUpdatedEventArgs> pointCloudUpdated
        {
            add
            {
                s_PointCloudUpdated += value;
                SetRunning(depthSubsystem, depthDataRequested);
            }

            remove
            {
                s_PointCloudUpdated -= value;
                SetRunning(depthSubsystem, depthDataRequested);
            }
        }

        /// <summary>
        /// This event is invoked whenever a reference point changes.
        /// </summary>
        /// <remarks>
        /// This is the low-level XR interface, and the data is in session space.
        /// Consider instead subscribing to the more useful <see cref="ARReferencePointManager.referencePointUpdated"/>.
        /// </remarks>
        public static event Action<ReferencePointUpdatedEventArgs> referencePointUpdated;

        /// <summary>
        /// This event is invoked whenever a face is added.
        /// </summary>
        /// <remarks>
        /// This is the low-level XR interface, and the data is in session space.
        /// Consider instead subscribing to the more useful <see cref="ARFaceManager.faceAdded"/>.
        ///
        /// Face tracking is disabled if there are no subscribers to at least one
        /// of <see cref="faceAdded"/>, <see cref="faceUpdated"/>, or <see cref="faceRemoved"/>.
        /// </remarks>
        public static event Action<FaceAddedEventArgs> faceAdded
        {
            add
            {
                s_FaceAdded += value;
                SetRunning(faceSubsystem, faceDetectionRequested);
            }

            remove
            {
                s_FaceAdded -= value;
                SetRunning(faceSubsystem, faceDetectionRequested);
            }
        }

        /// <summary>
        /// This event is invoked whenever an existing face is updated.
        /// </summary>
        /// <remarks>
        /// This is the low-level XR interface, and the data is in session space.
        /// Consider instead subscribing to the more useful <see cref="ARFaceManager.faceUpdated"/>.
        ///
        /// Face tracking is disabled if there are no subscribers to at least one
        /// of <see cref="faceAdded"/>, <see cref="faceUpdated"/>, or <see cref="faceRemoved"/>.
        /// </remarks>
        public static event Action<FaceUpdatedEventArgs> faceUpdated
        {
            add
            {
                s_FaceUpdated += value;
                SetRunning(faceSubsystem, faceDetectionRequested);
            }

            remove
            {
                s_FaceUpdated -= value;
                SetRunning(faceSubsystem, faceDetectionRequested);
            }
        }

        /// <summary>
        /// This event is invoked whenever an existing face is removed.
        /// </summary>
        /// <remarks>
        /// This is the low-level XR interface, and the data is in session space.
        /// Consider instead subscribing to the more useful <see cref="ARFaceManager.faceRemoved"/>.
        ///
        /// Face tracking is disabled if there are no subscribers to at least one
        /// of <see cref="faceAdded"/>, <see cref="faceUpdated"/>, or <see cref="faceRemoved"/>.
        /// </remarks>
        public static event Action<FaceRemovedEventArgs> faceRemoved
        {
            add
            {
                s_FaceRemoved += value;
                SetRunning(faceSubsystem, faceDetectionRequested);
            }

            remove
            {
                s_FaceRemoved -= value;
                SetRunning(faceSubsystem, faceDetectionRequested);
            }
        }


        /// <summary>
        /// The state of the entire system. Use this to determine the status of AR availability and installation.
        /// </summary>
        public static ARSystemState systemState
        {
            get
            {
                return s_SystemState;
            }

            private set
            {
                if (s_SystemState == value)
                    return;

                s_SystemState = value;
                RaiseSystemStateChangedEvent();
            }
        }

        /// <summary>
        /// Get or set whether light estimation should be enabled.
        /// </summary>
        /// <remarks>
        /// Note: You can only request light estimation. The underlying provider may not support light estimation.
        /// </remarks>
        public static bool lightEstimationRequested
        {
            get { return s_LightEstimationRequested; }
            set
            {
                if (lightEstimationRequested == value)
                    return;

                s_LightEstimationRequested = value;

                if (cameraSubsystem != null)
                    cameraSubsystem.LightEstimationRequested = value;

                UpdateCameraSubsystem();
            }
        }

        /// <summary>
        /// Get or set the <c>CameraFocusMode</c> to use on the physica AR camera.
        /// </summary>
        public static CameraFocusMode cameraFocusMode
        {
            get { return s_CameraFocusMode; }
            set
            {
                if (s_CameraFocusMode == value)
                    return;

                s_CameraFocusMode = value;
                if (cameraSubsystem != null)
                    cameraSubsystem.TrySetFocusMode(value);
            }
        }

        /// <summary>
        /// Start checking the availability of AR on the current device.
        /// </summary>
        /// <remarks>
        /// The availability check may be asynchronous, so this is implemented as a coroutine.
        /// It is safe to call this multiple times; if called a second time while an availability
        /// check is being made, it returns a new coroutine which waits on the first.
        /// </remarks>
        /// <returns>An <c>IEnumerator</c> used for a coroutine.</returns>
        public static IEnumerator CheckAvailability()
        {
            if (systemState == ARSystemState.CheckingAvailability)
                yield return new WaitWhile(() => { return systemState == ARSystemState.CheckingAvailability; });

            // Availability has already been determined if we make it here and the state is not None.
            if (systemState != ARSystemState.None)
                yield break;

            if (sessionSubsystem == null)
            {
                systemState = ARSystemState.Unsupported;
            }
            else if (systemState == ARSystemState.None)
            {
                systemState = ARSystemState.CheckingAvailability;
                var availabilityPromise = sessionSubsystem.GetAvailabilityAsync();
                yield return availabilityPromise;
                s_Availability = availabilityPromise.result;

                if (s_Availability.IsSupported() && s_Availability.IsInstalled())
                {
                    systemState = ARSystemState.Ready;
                }
                else if (s_Availability.IsSupported() && !s_Availability.IsInstalled())
                {
                    systemState = ARSystemState.NeedsInstall;
                }
                else
                {
                    systemState = ARSystemState.Unsupported;
                }
            }
        }

        /// <summary>
        /// Begin installing AR software on the current device (if supported).
        /// </summary>
        /// <remarks>
        /// Installation may be asynchronous, so this is implemented as a coroutine.
        /// It is safe to call this multiple times; if called a second time while an installation
        /// is in process, it returns a new coroutine which waits on the first.
        /// </remarks>
        /// <returns>An <c>IEnumerator</c> used for a coroutine.</returns>
        public static IEnumerator Install()
        {
            if (systemState == ARSystemState.Installing)
                yield return new WaitWhile(() => { return systemState == ARSystemState.Installing; });

            if ((sessionSubsystem == null) || (systemState != ARSystemState.NeedsInstall))
                yield break;

            systemState = ARSystemState.Installing;
            var installPromise = sessionSubsystem.InstallAsync();
            yield return installPromise;
            var installStatus = installPromise.result;

            if (installStatus == SessionInstallationStatus.Success)
            {
                systemState = ARSystemState.Ready;
                s_Availability = (s_Availability | SessionAvailability.Installed);
            }
            else if (installStatus == SessionInstallationStatus.ErrorUserDeclined)
            {
                systemState = ARSystemState.NeedsInstall;
            }
            else
            {
                systemState = ARSystemState.Unsupported;
            }
        }

        /// <summary>
        /// Creates each XR Subsystem associated with an AR session.
        /// </summary>
        public static void CreateSubsystems()
        {
            // Find and initialize each subsystem
            sessionSubsystem = ARSubsystemUtil.CreateSessionSubsystem(subsystemFilter);
            cameraSubsystem = ARSubsystemUtil.CreateCameraSubsystem(subsystemFilter);
            inputSubsystem = ARSubsystemUtil.CreateInputSubsystem(subsystemFilter);
            depthSubsystem = ARSubsystemUtil.CreateDepthSubsystem(subsystemFilter);
            planeSubsystem = ARSubsystemUtil.CreatePlaneSubsystem(subsystemFilter);
            referencePointSubsystem = ARSubsystemUtil.CreateReferencePointSubsystem(subsystemFilter);
            raycastSubsystem = ARSubsystemUtil.CreateRaycastSubsystem(subsystemFilter);

            faceSubsystem = ARSubsystemUtil.CreateFaceSubsystem(subsystemFilter);

            if (planeSubsystem != null)
            {
                planeSubsystem.PlaneAdded -= OnPlaneAdded;
                planeSubsystem.PlaneAdded += OnPlaneAdded;
                planeSubsystem.PlaneUpdated -= OnPlaneUpdated;
                planeSubsystem.PlaneUpdated += OnPlaneUpdated;
                planeSubsystem.PlaneRemoved -= OnPlaneRemoved;
                planeSubsystem.PlaneRemoved += OnPlaneRemoved;
            }

            if (faceSubsystem != null)
            {
                faceSubsystem.faceAdded -= OnFaceAdded;
                faceSubsystem.faceAdded += OnFaceAdded;
                faceSubsystem.faceUpdated -= OnFaceUpdated;
                faceSubsystem.faceUpdated += OnFaceUpdated;
                faceSubsystem.faceRemoved -= OnFaceRemoved;
                faceSubsystem.faceRemoved += OnFaceRemoved;
            }


            if (depthSubsystem != null)
            {
                depthSubsystem.PointCloudUpdated -= OnPointCloudUpdated;
                depthSubsystem.PointCloudUpdated += OnPointCloudUpdated;
            }

            if (referencePointSubsystem != null)
            {
                referencePointSubsystem.ReferencePointUpdated -= OnReferencePointUpdated;
                referencePointSubsystem.ReferencePointUpdated += OnReferencePointUpdated;
            }

            if (cameraSubsystem != null)
            {
                cameraSubsystem.FrameReceived -= OnFrameReceived;
                cameraSubsystem.FrameReceived += OnFrameReceived;
                cameraSubsystem.LightEstimationRequested = lightEstimationRequested;
            }

            if (sessionSubsystem != null)
            {
                sessionSubsystem.TrackingStateChanged -= OnTrackingStateChanged;
                sessionSubsystem.TrackingStateChanged += OnTrackingStateChanged;
            }
        }

        /// <summary>
        /// Destroys each XR Subsystem associated with an AR session.
        /// </summary>
        public static void DestroySubsystems()
        {
            DestroySubsystem(sessionSubsystem);
            if (sessionSubsystem != null)
                RaiseSessionDestroyedEvent();

            DestroySubsystem(cameraSubsystem);
            DestroySubsystem(inputSubsystem);
            DestroySubsystem(depthSubsystem);
            DestroySubsystem(planeSubsystem);
            DestroySubsystem(referencePointSubsystem);
            DestroySubsystem(raycastSubsystem);
            DestroySubsystem(faceSubsystem);

            sessionSubsystem = null;
            cameraSubsystem = null;
            inputSubsystem = null;
            depthSubsystem = null;
            planeSubsystem = null;
            referencePointSubsystem = null;
            raycastSubsystem = null;

            // Only set back to ready if we were previously running
            if (systemState > ARSystemState.Ready)
                systemState = ARSystemState.Ready;
        }

        /// <summary>
        /// Starts each of the XR Subsystems associated with AR session according to the requested options.
        /// </summary>
        /// <remarks>
        /// Throws <c>InvalidOperationException</c> if there is no <c>XRSessionSubsystem</c>.
        /// </remarks>
        public static void StartSubsystems()
        {
            // Early out if we're already running.
            if ((systemState == ARSystemState.SessionInitializing) || (systemState == ARSystemState.SessionTracking))
                return;

            if (sessionSubsystem == null)
                throw new InvalidOperationException("Cannot start AR session because there is no session subsystem.");

            if (systemState < ARSystemState.Ready)
                throw new InvalidOperationException("StartSubsystems called before being Ready.");

            SetRunning(raycastSubsystem, true);
            SetRunning(referencePointSubsystem, true);
            SetRunning(inputSubsystem, true);
            UpdatePlaneDetection();
            SetRunning(depthSubsystem, depthDataRequested);
            UpdateCameraSubsystem();
            SetRunning(faceSubsystem, faceDetectionRequested);

            sessionSubsystem.Start();

            // Don't set to initializing if Start actually starts immediately
            if (systemState == ARSystemState.Ready)
                systemState = ARSystemState.SessionInitializing;
        }

        /// <summary>
        /// Stops all the XR Subsystems associated with the AR session.
        /// </summary>
        /// <remarks>
        /// "Stopping" an AR session does not destroy the session. A call to <see cref="StopSubsystems"/> followed by <see cref="StartSubsystems"/>
        /// is similar to "pause" and "resume". To completely destroy the current AR session and begin a new one, you must first
        /// <see cref="DestroySubsystems"/>.
        /// </remarks>
        public static void StopSubsystems()
        {
            // Nothing to stop if we aren't running
            if (systemState < ARSystemState.SessionInitializing)
                return;

            SetRunning(raycastSubsystem, false);
            SetRunning(referencePointSubsystem, false);
            SetRunning(inputSubsystem, false);
            SetRunning(planeSubsystem, false);
            SetRunning(depthSubsystem, false);
            SetRunning(cameraSubsystem, false);
            SetRunning(faceSubsystem, false);
            sessionSubsystem.Stop();
            systemState = ARSystemState.Ready;
        }

        static void RaiseSessionDestroyedEvent()
        {
            if (sessionDestroyed != null)
                sessionDestroyed();
        }

        static void RaiseSystemStateChangedEvent()
        {
            if (systemStateChanged != null)
                systemStateChanged(new ARSystemStateChangedEventArgs(systemState));
        }

        static void OnTrackingStateChanged(SessionTrackingStateChangedEventArgs eventArgs)
        {
            switch (eventArgs.NewState)
            {
                case TrackingState.Unknown:
                case TrackingState.Unavailable:
                    systemState = ARSystemState.SessionInitializing;
                    break;
                case TrackingState.Tracking:
                    systemState = ARSystemState.SessionTracking;
                    break;
            }
        }

        static void OnFrameReceived(FrameReceivedEventArgs eventArgs)
        {
            if (s_CameraFrameReceived != null)
                RaiseFrameReceivedEvent();
        }

        static void RaiseFrameReceivedEvent()
        {
            var lightEstimation = new LightEstimationData();

            float data = 0F;
            if (cameraSubsystem.TryGetAverageBrightness(ref data))
                lightEstimation.averageBrightness = data;
            if (cameraSubsystem.TryGetAverageColorTemperature(ref data))
                lightEstimation.averageColorTemperature = data;

            Color colorCorrection;
            if (cameraSubsystem.TryGetColorCorrection(out colorCorrection))
                lightEstimation.colorCorrection = colorCorrection;

            float? timestampSeconds = null;
            Int64 timestampNs = 0;
            if (cameraSubsystem.TryGetTimestamp(ref timestampNs))
                timestampSeconds = timestampNs * 1e-9F;

            s_CameraFrameReceived(new ARCameraFrameEventArgs(lightEstimation, timestampSeconds));
        }

        static void OnPlaneAdded(PlaneAddedEventArgs eventArgs)
        {
            if (s_PlaneAdded != null)
                s_PlaneAdded(eventArgs);
        }

        static void OnPlaneUpdated(PlaneUpdatedEventArgs eventArgs)
        {
            if (s_PlaneUpdated != null)
                s_PlaneUpdated(eventArgs);
        }

        static void OnPlaneRemoved(PlaneRemovedEventArgs eventArgs)
        {
            if (s_PlaneRemoved != null)
                s_PlaneRemoved(eventArgs);
        }

        static void OnPointCloudUpdated(PointCloudUpdatedEventArgs eventArgs)
        {
            if (s_PointCloudUpdated != null)
                s_PointCloudUpdated(eventArgs);
        }

        static void OnReferencePointUpdated(ReferencePointUpdatedEventArgs eventArgs)
        {
            if (referencePointUpdated != null)
                referencePointUpdated(eventArgs);
        }

        static void OnFaceAdded(FaceAddedEventArgs eventArgs)
        {
            if (s_FaceAdded != null)
                s_FaceAdded(eventArgs);
        }

        static void OnFaceUpdated(FaceUpdatedEventArgs eventArgs)
        {
            if (s_FaceUpdated != null)
                s_FaceUpdated(eventArgs);
        }

        static void OnFaceRemoved(FaceRemovedEventArgs eventArgs)
        {
            if (s_FaceRemoved != null)
                s_FaceRemoved(eventArgs);
        }

#if UNITY_2018_3_OR_NEWER
        static void DestroySubsystem(ISubsystem subsystem)
#else
        static void DestroySubsystem(Subsystem subsystem)
#endif
        {
            if (subsystem != null)
                subsystem.Destroy();
        }

#if UNITY_2018_3_OR_NEWER
        static void SetRunning(ISubsystem subsystem, bool shouldBeRunning)
#else
        static void SetRunning(Subsystem subsystem, bool shouldBeRunning)
#endif
        {
            if (subsystem == null)
                return;

            if (shouldBeRunning)
                subsystem.Start();
            else
                subsystem.Stop();
        }

        static void UpdatePlaneDetection()
        {
            SetRunning(planeSubsystem, planeDetectionRequested);
            if (planeDetectionRequested && planeSubsystem != null)
                planeSubsystem.TrySetPlaneDetectionFlags(s_PlaneDetectionFlags);
        }

        static void UpdateCameraSubsystem()
        {
            SetRunning(cameraSubsystem, cameraSubsystemRequested);
            if (cameraSubsystem != null)
                cameraSubsystem.TrySetFocusMode(s_CameraFocusMode);
        }

        static ARSystemState s_SystemState;

        // We don't expose this publicly because it is inferred from
        // 1. lighting estimation requested
        // 2. camera event subscriptions
        static bool cameraSubsystemRequested
        {
            get
            {
                return
                    lightEstimationRequested ||
                    (s_CameraFrameReceived != null);
            }
        }

        static Action<PlaneAddedEventArgs> s_PlaneAdded;

        static Action<PlaneUpdatedEventArgs> s_PlaneUpdated;

        static Action<PlaneRemovedEventArgs> s_PlaneRemoved;

        static bool planeDetectionRequested
        {
            get
            {
                return
                    (s_PlaneAdded != null) ||
                    (s_PlaneUpdated != null) ||
                    (s_PlaneRemoved != null);
            }
        }

        static SessionAvailability s_Availability;

        static Action<FaceAddedEventArgs> s_FaceAdded;

        static Action<FaceUpdatedEventArgs> s_FaceUpdated;

        static Action<FaceRemovedEventArgs> s_FaceRemoved;

        static bool faceDetectionRequested
        {
            get
            {
                return
                    (s_FaceAdded != null) ||
                    (s_FaceUpdated != null) ||
                    (s_FaceRemoved != null);
            }
        }

        static PlaneDetectionFlags s_PlaneDetectionFlags = PlaneDetectionFlags.Horizontal | PlaneDetectionFlags.Vertical;

        static CameraFocusMode s_CameraFocusMode = CameraFocusMode.Fixed;

        static Action<PointCloudUpdatedEventArgs> s_PointCloudUpdated;

        static Action<ARCameraFrameEventArgs> s_CameraFrameReceived;

        static bool depthDataRequested
        {
            get
            {
                return (s_PointCloudUpdated != null);
            }
        }

        static bool s_LightEstimationRequested;
    }
}
