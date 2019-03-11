using System;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Add this component to a <c>Camera</c> to copy the color camera's texture onto the background.
    /// </summary>
    /// <remarks>
    /// This is the component-ized version of <c>UnityEngine.XR.ARBackgroundRenderer</c>.
    /// </remarks>
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/api/UnityEngine.XR.ARFoundation.ARCameraBackground.html")]
    public sealed class ARCameraBackground : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("m_OverrideMaterial")]
        bool m_UseCustomMaterial;

        /// <summary>
        /// When <c>false</c>, a material is generated automatically from the shader included in the platform-specific package.
        /// When <c>true</c>, <see cref="customMaterial"/> is used instead, overriding the automatically generated one.
        /// This is not necessary for most AR experiences.
        /// </summary>
        public bool useCustomMaterial
        {
            get { return m_UseCustomMaterial; }
            set
            {
                m_UseCustomMaterial = value;
                UpdateMaterial();
            }
        }

        [SerializeField, FormerlySerializedAs("m_Material")]
        Material m_CustomMaterial;

        /// <summary>
        /// If <see cref="useCustomMaterial"/> is <c>true</c>, this <c>Material</c> will be used
        /// instead of the one included with the platform-specific AR package.
        /// </summary>
        public Material customMaterial
        {
            get { return m_CustomMaterial; }
            set
            {
                m_CustomMaterial = value;
                UpdateMaterial();
            }
        }

        /// <summary>
        /// The current <c>Material</c> used for background rendering.
        /// </summary>
        public Material material
        {
            get
            {
                return backgroundRenderer.backgroundMaterial;
            }
            private set
            {
                backgroundRenderer.backgroundMaterial = value;
                if (ARSubsystemManager.cameraSubsystem != null)
                    ARSubsystemManager.cameraSubsystem.Material = value;
            }
        }

        [SerializeField]
        bool m_UseCustomRendererAsset;

        /// <summary>
        /// Whether to use a <see cref="ARBackgroundRendererAsset"/>. This can assist with
        /// usage of the light weight render pipeline.
        /// </summary>
        public bool useCustomRendererAsset
        {
            get { return m_UseCustomRendererAsset; }
            set
            {
                m_UseCustomRendererAsset = value;
                SetupBackgroundRenderer();
            }
        }

        [SerializeField] 
        ARBackgroundRendererAsset m_CustomRendererAsset;

        /// <summary>
        /// Get the custom <see cref="ARBackgroundRendererAsset "/> to use. This can
        /// assist with usage of the light weight render pipeline.
        /// </summary>
        public ARBackgroundRendererAsset customRendererAsset
        {
            get { return m_CustomRendererAsset; }
            set
            {
                m_CustomRendererAsset = value;
                SetupBackgroundRenderer();
            }
        }

        ARFoundationBackgroundRenderer backgroundRenderer { get; set; }

        Material CreateMaterialFromSubsystemShader()
        {
            var cameraSubsystem = ARSubsystemManager.cameraSubsystem;
            if (m_CameraSetupThrewException || (cameraSubsystem == null))
                return null;

            // Try to create a material from the plugin's provided shader.
            string shaderName = "";
            if (!cameraSubsystem.TryGetShaderName(ref shaderName))
                return null;

            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                // If an exception is thrown, then something is irrecoverably wrong.
                // Set this flag so we don't try to do this every frame.
                m_CameraSetupThrewException = true;

                throw new InvalidOperationException(string.Format(
                    "Could not find shader named \"{0}\" required for video overlay on camera subsystem named \"{1}\".",
                    shaderName,
                    cameraSubsystem.SubsystemDescriptor.id));
            }

            return new Material(shader);
        }

        void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            ARSubsystemManager.cameraSubsystem.Camera = m_Camera;
            UpdateMaterial();
            mode = ARRenderMode.MaterialAsBackground;
        }

        void SetupBackgroundRenderer()
        {
            if (useRenderPipeline)
            {
                if (m_LwrpBackgroundRenderer == null)
                {
                    m_LwrpBackgroundRenderer = m_CustomRendererAsset.CreateARBackgroundRenderer();
                    m_CustomRendererAsset.CreateHelperComponents(gameObject);
                }

                backgroundRenderer = m_LwrpBackgroundRenderer;
            }
            else
            {
                if (m_LegacyBackgroundRenderer == null)
                    m_LegacyBackgroundRenderer = new ARFoundationBackgroundRenderer();

                backgroundRenderer = m_LegacyBackgroundRenderer;
            }

            backgroundRenderer.mode = mode;
            backgroundRenderer.camera = m_Camera;
        }

        void Awake()
        {
            m_Camera = GetComponent<Camera>();
            SetupBackgroundRenderer();
        }

        void OnEnable()
        {
            UpdateMaterial();
            if (ARSubsystemManager.cameraSubsystem != null)
                ARSubsystemManager.cameraSubsystem.Camera = m_Camera;
            ARSubsystemManager.cameraFrameReceived += OnCameraFrameReceived;
            ARSubsystemManager.systemStateChanged += OnSystemStateChanged;
        }

        void OnDisable()
        {
            mode = ARRenderMode.StandardBackground;
            ARSubsystemManager.cameraFrameReceived -= OnCameraFrameReceived;
            ARSubsystemManager.systemStateChanged -= OnSystemStateChanged;
            m_CameraSetupThrewException = false;

            // Tell the camera subsystem to stop doing work if we are still the active camera
            var cameraSubsystem = ARSubsystemManager.cameraSubsystem;
            if ((cameraSubsystem != null) && (cameraSubsystem.Camera == m_Camera))
            {
                cameraSubsystem.Camera = null;
                cameraSubsystem.Material = null;
            }

            // We are no longer setting the projection matrix
            // so tell the camera to resume its normal projection
            // matrix calculations.
            m_Camera.ResetProjectionMatrix();
        }

        void OnSystemStateChanged(ARSystemStateChangedEventArgs eventArgs)
        {
            // If the session goes away then return to using standard background mode
            if (eventArgs.state < ARSystemState.SessionInitializing && backgroundRenderer != null)
                mode = ARRenderMode.StandardBackground;
        }

        void UpdateMaterial()
        {
            if (useRenderPipeline)
            {
                material = lwrpMaterial;
            }
            else
            {
                material = m_UseCustomMaterial ? m_CustomMaterial : subsystemMaterial;
            }
        }

        bool m_CameraSetupThrewException;

        Camera m_Camera;

        Material m_SubsystemMaterial;

        private Material subsystemMaterial
        {
            get
            {
                if (m_SubsystemMaterial == null)
                    m_SubsystemMaterial = CreateMaterialFromSubsystemShader();

                return m_SubsystemMaterial;
            }
        }

        Material m_LwrpMaterial;

        Material lwrpMaterial
        {
            get
            {
                if (m_LwrpMaterial != null)
                    return m_LwrpMaterial;

                if (m_UseCustomRendererAsset && m_CustomRendererAsset != null)
                {
                    m_LwrpMaterial = m_CustomRendererAsset.CreateCustomMaterial();
                }

                return m_LwrpMaterial;
            }
        }

        ARFoundationBackgroundRenderer m_LegacyBackgroundRenderer;

        ARFoundationBackgroundRenderer m_LwrpBackgroundRenderer;

        ARRenderMode m_Mode;

        ARRenderMode mode
        {
            get { return m_Mode; }
            set
            {
                m_Mode = value;
                if (m_LwrpBackgroundRenderer != null)
                    m_LwrpBackgroundRenderer.mode = m_Mode;
                if (m_LegacyBackgroundRenderer != null)
                    m_LegacyBackgroundRenderer.mode = m_Mode;
            }
        }

        bool useRenderPipeline
        {
            get
            {
                return
                    m_UseCustomRendererAsset &&
                    (m_CustomRendererAsset != null) &&
                    (GraphicsSettings.renderPipelineAsset != null);
            }
        }
    }
}
