﻿// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Crest
{
    /// <summary>
    /// Renders terrain height / ocean depth once into a render target to cache this off and avoid rendering it every frame.
    /// This should be used for static geometry, dynamic objects should be tagged with the Render Ocean Depth component.
    /// </summary>
    [ExecuteAlways]
    [HelpURL("https://github.com/wave-harmonic/crest/blob/master/USERGUIDE.md#shorelines-and-shallow-water")]
    public partial class OceanDepthCache : MonoBehaviour
    {
        public enum OceanDepthCacheType
        {
            Realtime,
            Baked,
        }

        public enum OceanDepthCacheRefreshMode
        {
            OnStart,
            OnDemand,
        }

        [Tooltip("Realtime = cache will be dynamic in accordance to refresh mode, Baked = cache will use the provided texture."), SerializeField]
        OceanDepthCacheType _type = OceanDepthCacheType.Realtime;
        public OceanDepthCacheType Type => _type;

        [Tooltip("Ignored if baked. On Start = cache will populate in Start(), On Demand = call PopulateCache() manually via scripting."), SerializeField]
        OceanDepthCacheRefreshMode _refreshMode = OceanDepthCacheRefreshMode.OnStart;
        public OceanDepthCacheRefreshMode RefreshMode => _refreshMode;

        [Tooltip("The layers to render into the depth cache.")]
        public string[] _layerNames = new string[0];

        [Tooltip("The resolution of the cached depth - lower will be more efficient.")]
        public int _resolution = 512;

        // A big hill will still want to write its height into the depth texture
        [Tooltip("The 'near plane' for the depth cache camera (top down)."), SerializeField]
        float _cameraMaxTerrainHeight = 100f;

        [Tooltip("Will render into the cache every frame. Intended for debugging, will generate garbage."), SerializeField]
#pragma warning disable 414
        bool _forceAlwaysUpdateDebug = false;
#pragma warning restore 414

        [Tooltip("Hides the depth cache camera, for cleanliness. Disable to make it visible in the Hierarchy."), SerializeField]
        bool _hideDepthCacheCam = true;

        [Tooltip("Baked depth cache. Baking button available in play mode."), SerializeField]
#pragma warning disable 649
        Texture2D _savedCache;
#pragma warning restore 649
        public Texture2D SavedCache => _savedCache;

#pragma warning disable 414
        [Tooltip("Editor only: run validation checks on Start() to check for issues."), SerializeField]
        bool _runValidationOnStart = true;
#pragma warning restore 414

        RenderTexture _cacheTexture;
        public RenderTexture CacheTexture => _cacheTexture;

        GameObject _drawCacheQuad;
        Camera _camDepthCache;
        Material _copyDepthMaterial;

        void Start()
        {
            if (OceanRenderer.Instance == null)
            {
                enabled = false;
                return;
            }

#if UNITY_EDITOR
            if (EditorApplication.isPlaying && _runValidationOnStart)
            {
                Validate(OceanRenderer.Instance, ValidatedHelper.DebugLog);
            }
#endif

            if (_type == OceanDepthCacheType.Baked)
            {
                InitCacheQuad();
            }
            else if (_type == OceanDepthCacheType.Realtime && _refreshMode == OceanDepthCacheRefreshMode.OnStart)
            {
                PopulateCache();
            }
        }

#if UNITY_EDITOR
        void Update()
        {
            if (_forceAlwaysUpdateDebug)
            {
                PopulateCache();
            }
        }
#endif

        RenderTexture MakeRT(bool depthStencilTarget)
        {
            RenderTextureFormat fmt;

            if (depthStencilTarget)
            {
                fmt = RenderTextureFormat.Depth;
            }
            else
            {
#if UNITY_EDITOR_WIN
                fmt = RenderTextureFormat.DefaultHDR;
#else
                fmt = RenderTextureFormat.RHalf;
#endif
            }

            Debug.Assert(SystemInfo.SupportsRenderTextureFormat(fmt), "The graphics device does not support the render texture format " + fmt.ToString());
            var result = new RenderTexture(_resolution, _resolution, depthStencilTarget ? 24 : 0);
            result.name = gameObject.name + "_oceanDepth_" + (depthStencilTarget ? "DepthOnly" : "Cache");
            result.format = fmt;
            result.useMipMap = false;
            result.anisoLevel = 0;
            return result;
        }

        bool InitObjects()
        {
            if (_cacheTexture == null)
            {
                _cacheTexture = MakeRT(false);
            }

            if (_camDepthCache == null)
            {
                var errorShown = false;
                var layerMask = 0;
                foreach (var layer in _layerNames)
                {
                    if (string.IsNullOrEmpty(layer))
                    {
                        Debug.LogError("OceanDepthCache: An empty layer name was provided. Please provide a valid layer name. Click this message to highlight the cache in question.", this);
                        errorShown = true;
                        continue;
                    }

                    int layerIdx = LayerMask.NameToLayer(layer);
                    if (layerIdx == -1)
                    {
                        Debug.LogError("OceanDepthCache: Invalid layer specified: \"" + layer +
                            "\". Please add this layer to the project by putting the name in an empty layer slot in Edit/Project Settings/Tags and Layers. Click this message to highlight the cache in question.", this);

                        errorShown = true;
                    }
                    else
                    {
                        layerMask = layerMask | (1 << layerIdx);
                    }
                }

                if (layerMask == 0)
                {
                    if (!errorShown)
                    {
                        Debug.LogError("No valid layers for populating depth cache, aborting.", this);
                    }

                    return false;
                }

                _camDepthCache = new GameObject("DepthCacheCam").AddComponent<Camera>();
                _camDepthCache.gameObject.hideFlags = _hideDepthCacheCam ? HideFlags.HideAndDontSave : HideFlags.DontSave;
                _camDepthCache.transform.position = transform.position + Vector3.up * _cameraMaxTerrainHeight;
                _camDepthCache.transform.parent = transform;
                _camDepthCache.transform.localEulerAngles = 90f * Vector3.right;
                _camDepthCache.orthographic = true;
                _camDepthCache.orthographicSize = Mathf.Max(transform.lossyScale.x / 2f, transform.lossyScale.z / 2f);
                _camDepthCache.cullingMask = layerMask;
                _camDepthCache.clearFlags = CameraClearFlags.SolidColor;
                // Clear to 'very deep'
                _camDepthCache.backgroundColor = Color.white * 1000f;
                _camDepthCache.enabled = false;
                _camDepthCache.allowMSAA = false;
                _camDepthCache.allowDynamicResolution = false;
                
                // Stops behaviour from changing in VR. I tried disabling XR before/after camera render but it makes the editor
                // go bonkers with split windows.
                _camDepthCache.cameraType = CameraType.Reflection;
                // I'd prefer to destroy the camera object, but I found sometimes (on first start of editor) it will fail to render.
                _camDepthCache.gameObject.SetActive(false);

                var additionalCameraData = _camDepthCache.gameObject.AddComponent<UniversalAdditionalCameraData>();
                additionalCameraData.renderShadows = false;
                additionalCameraData.requiresColorTexture = false;
                additionalCameraData.requiresDepthTexture = false;
                additionalCameraData.renderPostProcessing = false;
            }

            if (_camDepthCache.targetTexture == null)
            {
                _camDepthCache.targetTexture = MakeRT(true);
            }

            InitCacheQuad();

            return true;
        }

        void InitCacheQuad()
        {
            Renderer qr;

            if (_drawCacheQuad == null)
            {
                _drawCacheQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _drawCacheQuad.hideFlags = HideFlags.DontSave;
#if UNITY_EDITOR
                DestroyImmediate(_drawCacheQuad.GetComponent<Collider>());
#else
                Destroy(_drawCacheQuad.GetComponent<Collider>());
#endif
                _drawCacheQuad.name = "DepthCache_" + gameObject.name + "_NOSAVE";
                _drawCacheQuad.transform.SetParent(transform, false);
                _drawCacheQuad.transform.localEulerAngles = 90f * Vector3.right;
                _drawCacheQuad.AddComponent<RegisterSeaFloorDepthInput>()._assignOceanDepthMaterial = false;
                qr = _drawCacheQuad.GetComponent<Renderer>();
                qr.sharedMaterial = new Material(Shader.Find(LodDataMgrSeaFloorDepth.ShaderName));
            }
            else
            {
                qr = _drawCacheQuad.GetComponent<Renderer>();
            }

            if (_type == OceanDepthCacheType.Baked)
            {
                qr.sharedMaterial.mainTexture = _savedCache;
            }
            else
            {
                qr.sharedMaterial.mainTexture = _cacheTexture;
            }

            qr.enabled = false;
        }

        public void PopulateCache()
        {
            // Make sure we have required objects
            if (!InitObjects())
            {
                return;
            }

            // Render scene, saving depths in depth buffer.
            _camDepthCache.Render();

            if (_copyDepthMaterial == null)
            {
                _copyDepthMaterial = new Material(Shader.Find("Crest/Copy Depth Buffer Into Cache"));
            }

            // Shader needs sea level to determine water depth. Ocean instance might not be available in prefabs.
            var centerPoint = Vector3.zero;
            centerPoint.y = OceanRenderer.Instance != null
                ? OceanRenderer.Instance.Root.position.y : transform.position.y;

            _copyDepthMaterial.SetVector("_OceanCenterPosWorld", centerPoint);

            _copyDepthMaterial.SetTexture("_CamDepthBuffer", _camDepthCache.targetTexture);

            // Zbuffer params
            //float4 _ZBufferParams;            // x: 1-far/near,     y: far/near, z: x/far,     w: y/far
            float near = _camDepthCache.nearClipPlane, far = _camDepthCache.farClipPlane;
            _copyDepthMaterial.SetVector("_CustomZBufferParams", new Vector4(1f - far / near, far / near, (1f - far / near) / far, (far / near) / far));

            // Altitudes for near and far planes
            float ymax = _camDepthCache.transform.position.y - near;
            float ymin = ymax - far;
            _copyDepthMaterial.SetVector("_HeightNearHeightFar", new Vector2(ymax, ymin));

            // Copy from depth buffer into the cache
            Graphics.Blit(null, _cacheTexture, _copyDepthMaterial);

            var leaveEnabled = _forceAlwaysUpdateDebug;

            if (!leaveEnabled)
            {
                _camDepthCache.targetTexture = null;

#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
#endif
                {
                    // Only disable component when in play mode, otherwise it changes authoring data
                    enabled = false;
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 0f, 1f));

            if (_type == OceanDepthCacheType.Realtime)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
                Gizmos.DrawCube(Vector3.up * _cameraMaxTerrainHeight / transform.lossyScale.y, new Vector3(1f, 0f, 1f));
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OceanDepthCache))]
    public class OceanDepthCacheEditor : ValidatedEditor
    {
        readonly string[] _propertiesToExclude = new string[] { "m_Script", "_type", "_refreshMode", "_savedCache", "_layerNames", "_resolution", "_cameraMaxTerrainHeight", "_forceAlwaysUpdateDebug" };

        public override void OnInspectorGUI()
        {
            // We won't just use default inspector because we want to show some of the params conditionally based on cache type

            // First show standard 'Script' field
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;

            // Next expose cache type and refresh mode

            var typeProp = serializedObject.FindProperty("_type");
            EditorGUILayout.PropertyField(typeProp);

            var cacheType = (OceanDepthCache.OceanDepthCacheType)typeProp.intValue;

            if (cacheType == OceanDepthCache.OceanDepthCacheType.Realtime)
            {
                // Only expose the following if real-time cache type
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_refreshMode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_layerNames"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_resolution"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_cameraMaxTerrainHeight"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_forceAlwaysUpdateDebug"));
            }
            else
            {
                // Only expose saved cache if non-real-time
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_savedCache"));
            }

            // Draw rest of inspector fields
            DrawPropertiesExcluding(serializedObject, _propertiesToExclude);

            // Apply inspector changes
            serializedObject.ApplyModifiedProperties();

            var playing = EditorApplication.isPlaying;

            var dc = target as OceanDepthCache;
            var isOnDemand = cacheType == OceanDepthCache.OceanDepthCacheType.Realtime &&
                dc.RefreshMode == OceanDepthCache.OceanDepthCacheRefreshMode.OnDemand;
            var isBakeable = cacheType == OceanDepthCache.OceanDepthCacheType.Realtime &&
                (!isOnDemand || dc.CacheTexture != null);

            if ((!playing || isOnDemand) && dc.Type != OceanDepthCache.OceanDepthCacheType.Baked && GUILayout.Button("Populate cache"))
            {
                dc.PopulateCache();
            }

            if (isBakeable && GUILayout.Button("Save cache to file"))
            {
                var rt = dc.CacheTexture;
                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                RenderTexture.active = null;

                byte[] bytes;
                bytes = tex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);

                string path = dc.SavedCache ?
                    AssetDatabase.GetAssetPath(dc.SavedCache) : $"Assets/OceanDepthCache_{Guid.NewGuid()}.exr";
                System.IO.File.WriteAllBytes(path, bytes);
                AssetDatabase.ImportAsset(path);

                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                ti.textureType = TextureImporterType.SingleChannel;
                ti.sRGBTexture = false;
                ti.alphaSource = TextureImporterAlphaSource.None;
                ti.alphaIsTransparency = false;
                ti.SaveAndReimport();

                Debug.Log("Cache saved to " + path, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
            }

            ShowValidationMessages();
        }
    }

    public partial class OceanDepthCache : IValidated
    {
        public bool Validate(OceanRenderer ocean, ValidatedHelper.ShowMessage showMessage)
        {
            var isValid = true;

            if (_type == OceanDepthCacheType.Baked)
            {
                if (_savedCache == null)
                {
                    showMessage
                    (
                        "Depth cache type is 'Saved Cache' but no saved cache data is provided.",
                        ValidatedHelper.MessageType.Error, this
                    );

                    isValid = false;
                }
            }
            else
            {
                if (_layerNames.Length == 0)
                {
                    showMessage
                    (
                        "No layers specified for rendering into depth cache, and no geometries manually provided.",
                        ValidatedHelper.MessageType.Error, this
                    );

                    isValid = false;
                }

                if (_forceAlwaysUpdateDebug)
                {
                    showMessage
                    (
                        $"<i>Force Always Update Debug</i> option is enabled on depth cache <i>{gameObject.name}</i>, which means it will render every frame instead of running from the cache.",
                        ValidatedHelper.MessageType.Warning, this
                    );

                    isValid = false;
                }

                foreach (var layerName in _layerNames)
                {
                    if (string.IsNullOrEmpty(layerName))
                    {
                        showMessage
                        (
                            "An empty layer name was provided. Please provide a valid layer name.",
                            ValidatedHelper.MessageType.Error, this
                        );

                        isValid = false;
                        continue;
                    }

                    var layer = LayerMask.NameToLayer(layerName);
                    if (layer == -1)
                    {
                        showMessage
                        (
                            $"Invalid layer specified for objects/geometry providing the ocean depth: <i>{layerName}</i>. Please add this layer to the project by putting the name in an empty layer slot in <i>Edit/Project Settings/Tags and Layers</i>?",
                            ValidatedHelper.MessageType.Error, this
                        );

                        isValid = false;
                    }
                }

                if (_resolution < 4)
                {
                    showMessage
                    (
                        $"Cache resolution {_resolution} is very low. Is this intentional?",
                        ValidatedHelper.MessageType.Error, this
                    );

                    isValid = false;
                }

                // We used to test if nothing is present that would render into the cache, but these could probably come from other scenes, and AssignLayer means
                // objects can be tagged up at run-time.
            }

            if (transform.lossyScale.magnitude < 5f)
            {
                showMessage
                (
                    "Ocean depth cache transform scale is small and will capture a small area of the world. The scale sets the size of the area that will be cached, and this cache is set to render a very small area.",
                    ValidatedHelper.MessageType.Warning, this
                );

                isValid = false;
            }

            if (transform.lossyScale.y < 0.001f || transform.localScale.y < 0.01f)
            {
                showMessage
                (
                    $"Ocean depth cache scale Y should be set to 1.0. Its current scale in the hierarchy is {transform.lossyScale.y}.",
                    ValidatedHelper.MessageType.Error, this
                );

                isValid = false;
            }

            if (ocean != null && ocean.Root != null && Mathf.Abs(transform.position.y - ocean.Root.position.y) > 0.00001f)
            {
                showMessage
                (
                    "It is recommended that the cache is placed at the same height (y component of position) as the ocean, i.e. at the sea level. If the cache is created before the ocean is present, the cache height will inform the sea level.",
                    ValidatedHelper.MessageType.Warning, this
                );

                isValid = false;
            }

            // Check that there are no renderers in descendants.
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > (Application.isPlaying ? 1 : 0))
            {
                Renderer quadRenderer = _drawCacheQuad ? _drawCacheQuad.GetComponent<Renderer>() : null;

                foreach (var renderer in renderers)
                {
                    if (ReferenceEquals(renderer, quadRenderer)) continue;

                    showMessage
                    (
                        "It is not expected that a depth cache object has a Renderer component in its hierarchy." +
                        "The cache is typically attached to an empty GameObject. Please refer to the example content.",
                        ValidatedHelper.MessageType.Warning, renderer
                    );

                    // Reporting only one renderer at a time will be enough to avoid overwhelming user and UI.
                    break;
                }

                isValid = false;
            }

            return isValid;
        }
    }
#endif
}
