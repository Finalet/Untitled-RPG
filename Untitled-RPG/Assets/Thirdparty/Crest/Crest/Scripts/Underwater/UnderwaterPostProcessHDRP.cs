// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#if CREST_HDRP

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Crest
{
    [VolumeComponentMenu("Crest/Underwater")]
    public class UnderwaterPostProcessHDRP : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        /// <summary>
        /// The version of this asset. Can be used to migrate across versions. This value should
        /// only be changed when the editor upgrades the version.
        /// </summary>
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _version = 0;
#pragma warning restore 414

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforeTAA;

        private Material _underwaterPostProcessMaterial;
        private PropertyWrapperMaterial _underwaterPostProcessMaterialWrapper;

        private const string SHADER = "Hidden/Crest/Underwater/Underwater Effect HDRP";

        bool _firstRender = true;
        public BoolParameter _enable = new BoolParameter(false);
        [Tooltip("Copying params each frame ensures underwater appearance stays consistent with ocean material params. Has a small overhead so should be disabled if not needed.")]
        public BoolParameter _copyOceanMaterialParamsEachFrame = new BoolParameter(true);
        // This adds an offset to the cascade index when sampling ocean data, in effect smoothing/blurring it. Default
        // to shifting the maximum amount (shift from lod 0 to penultimate lod - dont use last lod as it cross-fades
        // data in/out), as more filtering was better in testing.
        [Tooltip("How much to smooth ocean data such as water depth, light scattering, shadowing. Helps to smooth flickering that can occur under camera motion.")]
        public ClampedIntParameter _filterOceanData = new ClampedIntParameter(LodDataMgr.MAX_LOD_COUNT - 2, 0, LodDataMgr.MAX_LOD_COUNT - 2);

        [Tooltip("Add a meniscus to the boundary between water and air.")]
        public BoolParameter _meniscus = new BoolParameter(true);

        [Header("Debug Options")]
        public BoolParameter _viewOceanMask = new BoolParameter(false);
        public BoolParameter _disableOceanMask = new BoolParameter(false);

        [Tooltip("Adjusts the far plane for horizon line calculation. Helps with horizon line issue. (Experimental)")]
        public ClampedFloatParameter _farPlaneMultiplier = new ClampedFloatParameter(0.68f, 0f, 1f);

        [Tooltip("Use the old horizon safety margin multiplier to fix horizon line issues instead of the new experimental far plane multiplier.")]
        public BoolParameter _useHorizonSafetyMarginMultiplier = new BoolParameter(false);

        [Tooltip("A safety margin multiplier to adjust horizon line based on camera position to avoid minor artifacts caused by floating point precision issues, the default value has been chosen based on careful experimentation.")]
        public ClampedFloatParameter _horizonSafetyMarginMultiplier = new ClampedFloatParameter(0.01f, 0f, 1f);
        public BoolParameter _scaleSafetyMarginWithDynamicResolution = new BoolParameter(true);

        UnderwaterRenderer.UnderwaterSphericalHarmonicsData _sphericalHarmonicsData = new UnderwaterRenderer.UnderwaterSphericalHarmonicsData();
        Camera _camera;

        static int s_xrPassIndex = -1;

        public static UnderwaterPostProcessHDRP Instance { get; private set; }

        public bool IsActive()
        {
            var isActive = Application.isPlaying;
            isActive = isActive && OceanRenderer.Instance != null;
            isActive = isActive && OceanRenderer.Instance.ViewerHeightAboveWater < 2f;
            isActive = isActive && _enable.value;

            // Do not disable if UnderwaterRenderer is active or exceptions.
            if (!isActive && UnderwaterRenderer.Instance == null)
            {
                UnderwaterMaskPassHDRP.Disable();
            }

            return isActive;
        }

        public override void Setup()
        {
#if UNITY_EDITOR

            Debug.LogWarning(
                "Crest: <i>Underwater Post-Process</i> has been deprecated and has been succeeded by the <i>Underwater Renderer</i>. " +
                $"Please visit the documentation for more information: {Internal.Constants.HELP_URL_BASE_USER}underwater.html?rp=\"hdrp\""
            );

            // We do not want both running at the same time so warn the user.
            if (_enable.value)
            {
                var underwaterRenderer = Object.FindObjectOfType<UnderwaterRenderer>();
                if (underwaterRenderer != null && underwaterRenderer.isActiveAndEnabled)
                {
                    Debug.LogError(
                        "UnderwaterPostProcessHDRP: An active <i>Underwater Renderer</i> component has been found. " +
                        "Please read the documentation on the differences and disable one of them: " +
                        $"{Internal.Constants.HELP_URL_BASE_USER}underwater.html?rp=\"hdrp\"",
                        underwaterRenderer
                    );
                }
            }
#endif
            Instance = this;

            _underwaterPostProcessMaterial = CoreUtils.CreateEngineMaterial(SHADER);
            _underwaterPostProcessMaterialWrapper = new PropertyWrapperMaterial(_underwaterPostProcessMaterial);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_underwaterPostProcessMaterial);

            // Guard against setup/clean-up overlap.
            if (Instance == this)
            {
                Instance = null;

                // Do not disable if UnderwaterRenderer is active or exceptions.
                if (UnderwaterRenderer.Instance == null)
                {
                    UnderwaterMaskPassHDRP.Disable();
                }
            }
        }

        public override void Render(CommandBuffer commandBuffer, HDCamera camera, RTHandle source, RTHandle destination)
        {
            // Used for cleanup
            _camera = camera.camera;

            // TODO: Put somewhere else?
            XRHelpers.Update(_camera);
            XRHelpers.UpdatePassIndex(ref s_xrPassIndex);

            if (OceanRenderer.Instance == null)
            {
                HDUtils.BlitCameraTexture(commandBuffer, source, destination);
                return;
            }

            // Post-process execute for every camera. We only support one camera for now.
            if (!ReferenceEquals(_camera, OceanRenderer.Instance.ViewCamera) || _camera.cameraType != CameraType.Game)
            {
                HDUtils.BlitCameraTexture(commandBuffer, source, destination);
                return;
            }

            // Call this every frame to ensure it is active just in case the user switches. All it does is set the
            // custom pass to active.
            UnderwaterMaskPassHDRP.Enable(null);

            // Dynamic resolution will cause the horizon gap to widen. For extreme values, the gap can be several pixels
            // thick. It can be disabled as it will require thorough testing across different hardware to get it 100%
            // right. This doesn't appear to be an issue for the built-in renderer.
            var horizonSafetyMarginMultiplier = _horizonSafetyMarginMultiplier.value;
            if (_scaleSafetyMarginWithDynamicResolution.value && DynamicResolutionHandler.instance.DynamicResolutionEnabled())
            {
                // 100 is a magic number from testing. Works well with default horizonSafetyMarginMultiplier of 0.01.
                horizonSafetyMarginMultiplier = Mathf.Lerp(horizonSafetyMarginMultiplier * 100,
                    horizonSafetyMarginMultiplier, DynamicResolutionHandler.instance.GetCurrentScale());
            }

            UnderwaterRenderer.UpdatePostProcessMaterial(
                camera.camera,
                _underwaterPostProcessMaterialWrapper,
                _sphericalHarmonicsData,
                _meniscus.value,
                _firstRender || _copyOceanMaterialParamsEachFrame.value,
                _viewOceanMask.value,
                // horizonSafetyMarginMultiplier is added to the horizon, so no-op is zero.
                _useHorizonSafetyMarginMultiplier.value ? horizonSafetyMarginMultiplier : 0f,
                // farPlaneMultiplier is multiplied to the far plane, so no-op is one.
                _useHorizonSafetyMarginMultiplier.value ? 1f : _farPlaneMultiplier.value,
                _filterOceanData.value,
                s_xrPassIndex
            );

            _underwaterPostProcessMaterial.SetTexture(UnderwaterRenderer.sp_CrestCameraColorTexture, source);
            // Shader pass 0 is post processing.
            HDUtils.DrawFullScreen(commandBuffer, _underwaterPostProcessMaterial, destination, properties: null, shaderPassId: 0);

            _firstRender = false;
        }
    }
}

#endif // CREST_HDRP
