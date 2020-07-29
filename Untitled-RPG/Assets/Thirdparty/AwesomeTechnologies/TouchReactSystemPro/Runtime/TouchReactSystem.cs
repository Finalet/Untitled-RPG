using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AwesomeTechnologies.TouchReact
{
    [Serializable]
    public class CapsuleColliderInfo
    {
        public float Radius;
        public float Height;
        public Mesh CapsuleColliderMesh;
    }

    [Serializable]
    public class TouchColliderInfo
    {
        public Collider Collider;
        public float Scale = 1;
    }

    public enum TouchReactQuality
    {
        Low = 0,
        Normal = 1,
        High = 2
    }

    [HelpURL("http://www.awesometech.no/index.php/home/vegetation-studio/components/touch-bend-system")]
    [ExecuteInEditMode]
    public class TouchReactSystem : MonoBehaviour
    {
        public static TouchReactSystem Instance;

        public int InvisibleLayer = 29;
        public Camera TouchReactCamera;
        public Camera SelectedCamera;
        public bool AutoselectCamera = true;
        public float CameraYPosition = -1000f;
        private Material _touchReactMaterial;
        private  Material _touchreactMaterialInstanced;
        private MaterialPropertyBlock _touchreactMaterialPropertyBlock;
        public TouchReactQuality TouchReactQuality = TouchReactQuality.Normal;
        public int OrthographicSize = 50;

        public bool ShowDebugColliders;
        public bool HideTouchReactCamera = true;

        private Mesh _sphereColliderMesh;
        private Mesh _boxColliderMesh;


        public List<TouchColliderInfo> ColliderList = new List<TouchColliderInfo>();
        public List<MeshFilter> MeshFilterList = new List<MeshFilter>();

        private readonly List<CapsuleColliderInfo> _capsuleColliderMeshList = new List<CapsuleColliderInfo>();

        // ReSharper disable once UnusedMember.Local
        void Awake()
        {
            if (Instance == null) Instance = this;

            ColliderList.Clear();
            MeshFilterList.Clear();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            CreateColliderPrimitives();
            Init();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            if (TouchReactCamera) TouchReactCamera.enabled = false;

            if (TouchReactCamera)
            {
                RenderTexture rt = TouchReactCamera.targetTexture;
                TouchReactCamera.targetTexture = null;
                DestroyImmediate(rt);
            }
        }

        private void CreateColliderPrimitives()
        {
            _capsuleColliderMeshList.Clear();

            _sphereColliderMesh = CreateSphereMesh();
            _boxColliderMesh = CreateBoxMesh();
        }

        public int GetTouchReactQualityPixelResolution(TouchReactQuality touchReactQuality)
        {
            switch (touchReactQuality)
            {
                case TouchReactQuality.Low:
                    return 512;
                case TouchReactQuality.Normal:
                    return 1024;
                case TouchReactQuality.High:
                    return 2048;
                default:
                    return 1024;
            }
        }

        public void UpdateCamera()
        {
            if (TouchReactCamera)
            {
                TouchReactCamera.cullingMask = 1 << InvisibleLayer;
                TouchReactCamera.orthographicSize = OrthographicSize;

                int textureResolution = GetTouchReactQualityPixelResolution(TouchReactQuality);
                RenderTexture rt =
                    new RenderTexture(textureResolution, textureResolution, 24, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear)
                    {
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Point,
                        autoGenerateMips = false,
                        hideFlags = HideFlags.DontSave
                    };

                RenderTexture oldRenderTexture = TouchReactCamera.targetTexture;
                TouchReactCamera.targetTexture = rt;

                if (oldRenderTexture)
                {
                    DestroyImmediate(oldRenderTexture);
                }
            }
        }

        private Mesh GetCapsuleColliderMesh(float radius, float height)
        {
            for (int i = 0; i <= _capsuleColliderMeshList.Count - 1; i++)
                if (Math.Abs(_capsuleColliderMeshList[i].Radius - radius) < 0.01f &&
                    Math.Abs(_capsuleColliderMeshList[i].Height - height) < 0.01f)
                    return _capsuleColliderMeshList[i].CapsuleColliderMesh;

            CapsuleColliderInfo newCapsuleColliderInfo = new CapsuleColliderInfo
            {
                Radius = radius,
                Height = height,
                CapsuleColliderMesh = CreateCapsuleMesh(radius, height)
            };

            _capsuleColliderMeshList.Add(newCapsuleColliderInfo);
            return newCapsuleColliderInfo.CapsuleColliderMesh;
        }

        public static Mesh CreateBoxMesh(float length = 1f,float width = 1f, float height = 1f)
        {
            Mesh mesh = new Mesh();
            mesh.Clear();

            //const float length = 1f;
            //const float width = 1f;
            //const float height = 1f;

            #region Vertices

            Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
            Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
            Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
            Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);

            Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
            Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
            Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
            Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

            Vector3[] vertices =
            {
                // Bottom
                p0, p1, p2, p3,

                // Left
                p7, p4, p0, p3,

                // Front
                p4, p5, p1, p0,

                // Back
                p6, p7, p3, p2,

                // Right
                p5, p6, p2, p1,

                // Top
                p7, p6, p5, p4
            };

            #endregion

            #region Normales

            Vector3 up = Vector3.up;
            Vector3 down = Vector3.down;
            Vector3 front = Vector3.forward;
            Vector3 back = Vector3.back;
            Vector3 left = Vector3.left;
            Vector3 right = Vector3.right;

            Vector3[] normales =
            {
                // Bottom
                down, down, down, down,

                // Left
                left, left, left, left,

                // Front
                front, front, front, front,

                // Back
                back, back, back, back,

                // Right
                right, right, right, right,

                // Top
                up, up, up, up
            };

            #endregion

            #region UVs

            Vector2 _00 = new Vector2(0f, 0f);
            Vector2 _10 = new Vector2(1f, 0f);
            Vector2 _01 = new Vector2(0f, 1f);
            Vector2 _11 = new Vector2(1f, 1f);

            UnityEngine.Vector2[] uvs =
            {
                // Bottom
                _11, _01, _00, _10,

                // Left
                _11, _01, _00, _10,

                // Front
                _11, _01, _00, _10,

                // Back
                _11, _01, _00, _10,

                // Right
                _11, _01, _00, _10,

                // Top
                _11, _01, _00, _10
            };

            #endregion

            #region Triangles

            int[] triangles =
            {
                // Bottom
                3, 1, 0,
                3, 2, 1,

                // Left
                3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

                // Front
                3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

                // Back
                3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

                // Right
                3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

                // Top
                3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5
            };

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateCapsuleMesh(float radius, float height)
        {
            int segments = 24;


            if (segments % 2 != 0)
                segments++;

            // extra vertex on the seam
            int points = segments + 1;

            // calculate points around a circle
            float[] pX = new float[points];
            float[] pZ = new float[points];
            float[] pY = new float[points];
            float[] pR = new float[points];

            float calcH = 0f;
            float calcV = 0f;

            for (int i = 0; i < points; i++)
            {
                pX[i] = Mathf.Sin(calcH * Mathf.Deg2Rad);
                pZ[i] = Mathf.Cos(calcH * Mathf.Deg2Rad);
                pY[i] = Mathf.Cos(calcV * Mathf.Deg2Rad);
                pR[i] = Mathf.Sin(calcV * Mathf.Deg2Rad);

                calcH += 360f / segments;
                calcV += 180f / segments;
            }


            // - Vertices and UVs -

            Vector3[] vertices = new Vector3[points * (points + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            int ind = 0;

            // Y-offset is half the height minus the diameter
            float yOff = (height - radius * 2f) * 0.5f;
            if (yOff < 0)
                yOff = 0;

            // uv calculations
            float stepX = 1f / (points - 1);
            float uvX, uvY;

            // Top Hemisphere
            int top = Mathf.CeilToInt(points * 0.5f);

            for (int y = 0; y < top; y++)
            for (int x = 0; x < points; x++)
            {
                vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                vertices[ind].y = yOff + vertices[ind].y;

                uvX = 1f - stepX * x;
                uvY = (vertices[ind].y + height * 0.5f) / height;
                uvs[ind] = new Vector2(uvX, uvY);

                ind++;
            }

            // Bottom Hemisphere
            int btm = Mathf.FloorToInt(points * 0.5f);

            for (int y = btm; y < points; y++)
            for (int x = 0; x < points; x++)
            {
                vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                vertices[ind].y = -yOff + vertices[ind].y;

                uvX = 1f - stepX * x;
                uvY = (vertices[ind].y + height * 0.5f) / height;
                uvs[ind] = new UnityEngine.Vector2(uvX, uvY);

                ind++;
            }


            // - Triangles -

            int[] triangles = new int[segments * (segments + 1) * 2 * 3];

            for (int y = 0, t = 0; y < segments + 1; y++)
            for (int x = 0; x < segments; x++, t += 6)
            {
                triangles[t + 0] = (y + 0) * (segments + 1) + x + 0;
                triangles[t + 1] = (y + 1) * (segments + 1) + x + 0;
                triangles[t + 2] = (y + 1) * (segments + 1) + x + 1;

                triangles[t + 3] = (y + 0) * (segments + 1) + x + 1;
                triangles[t + 4] = (y + 0) * (segments + 1) + x + 0;
                triangles[t + 5] = (y + 1) * (segments + 1) + x + 1;
            }

            Mesh mesh = new Mesh();
            mesh.Clear();

            mesh.name = "ProceduralCapsule";

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
            //_capsuleColliderMesh = mesh;
        }

        public static Mesh CreateSphereMesh(float radius = 1f)
        {
            Mesh mesh = new Mesh();
            mesh.Clear();

            //float radius = 1f;
            // Longitude |||
            int nbLong = 24;
            // Latitude ---
            int nbLat = 16;

            #region Vertices

            Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
            float _pi = Mathf.PI;
            var _2Pi = _pi * 2f;

            vertices[0] = Vector3.up * radius;
            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2Pi * (lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
                }
            }
            vertices[vertices.Length - 1] = Vector3.up * -radius;

            #endregion

            #region Normales		

            Vector3[] normales = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
                normales[n] = vertices[n].normalized;

            #endregion

            #region UVs

            UnityEngine.Vector2[] uvs = new UnityEngine.Vector2[vertices.Length];
            uvs[0] = UnityEngine.Vector2.up;
            uvs[uvs.Length - 1] = UnityEngine.Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
            for (int lon = 0; lon <= nbLong; lon++)
                uvs[lon + lat * (nbLong + 1) + 1] =
                    new UnityEngine.Vector2((float) lon / nbLong, 1f - (float) (lat + 1) / (nbLat + 1));

            #endregion

            #region Triangles

            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            //Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
            for (int lon = 0; lon < nbLong; lon++)
            {
                int current = lon + lat * (nbLong + 1) + 1;
                int next = current + nbLong + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }

            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            return mesh;
        }

        private void Update()
        {
            if (TouchReactCamera && SelectedCamera)
            {
                Vector3 tempCameraPosition = GetCameraPosition();
                var pos = tempCameraPosition;

                pos.x = SnapToPixel(pos.x, TouchReactCamera.targetTexture.width, TouchReactCamera.orthographicSize);
                pos.y = 0;
                pos.z = SnapToPixel(pos.z, TouchReactCamera.targetTexture.height, TouchReactCamera.orthographicSize);

                TouchReactCamera.transform.position = pos;
            }
            DrawColliders();
            DrawMeshfilters();
            UpdateShaders();
        }

        private void UpdateShaders()
        {
            Shader.SetGlobalTexture("_TouchReact_Buffer", TouchReactCamera.targetTexture);
            Vector4 pos = TouchReactCamera.transform.position;
            pos.z = -pos.z;
            pos.w = TouchReactCamera.orthographicSize * 2;
            pos.x -= TouchReactCamera.orthographicSize;
            pos.z -= TouchReactCamera.orthographicSize;
            Shader.SetGlobalVector("_TouchReact_Pos", pos);
        }

        private float SnapToPixel(float v, int textureSize, float orthoSize)
        {
            float worldPixel = orthoSize * 2 / textureSize;

            v = (int) (v / worldPixel);
            v *= worldPixel;

            return v;
        }

        private void DrawColliders()
        {
                for (int i = 0; i <= ColliderList.Count - 1; i++)
                {
                    Collider tempCollider = ColliderList[i].Collider;
                    if (tempCollider is MeshCollider)
                        DrawMeshCollider(tempCollider as MeshCollider);
                    else if (tempCollider is SphereCollider)
                        DrawSphereCollider(tempCollider as SphereCollider, ColliderList[i].Scale);
                    else if (tempCollider is BoxCollider)
                        DrawBoxCollider(tempCollider as BoxCollider, ColliderList[i].Scale);
                    else if (tempCollider is CapsuleCollider)
                        DrawCapsuleCollider(tempCollider as CapsuleCollider, ColliderList[i].Scale);
                }
        }

        private void DrawMeshfilters()
        {
                for (int i = 0; i <= MeshFilterList.Count - 1; i++)
                    DrawMeshfilter(MeshFilterList[i]);
        }

        private void DrawBoxCollider(BoxCollider boxCollider, float scale)
        {
            Vector3 boxScale = new Vector3(boxCollider.size.x * boxCollider.transform.lossyScale.x,
                boxCollider.size.y * boxCollider.transform.lossyScale.y,
                boxCollider.size.z * boxCollider.transform.lossyScale.z);

            Matrix4x4 positionMatrix = Matrix4x4.TRS(boxCollider.bounds.center, boxCollider.transform.rotation,
                boxScale * scale);
            if (ShowDebugColliders)
                Graphics.DrawMesh(_boxColliderMesh, positionMatrix, _touchReactMaterial, 0, null);
            Graphics.DrawMesh(_boxColliderMesh, positionMatrix, _touchReactMaterial, InvisibleLayer, TouchReactCamera);
        }

        private void DrawCapsuleCollider(CapsuleCollider capsuleCollider, float scale)
        {
            Mesh capsuleMesh = GetCapsuleColliderMesh(capsuleCollider.radius, capsuleCollider.height);

            Vector3 axisRotation = Vector3.zero;
            if (capsuleCollider.direction == 0) axisRotation = new Vector3(0, 0, 90);
            if (capsuleCollider.direction == 2) axisRotation = new Vector3(90, 0, 0);

            Matrix4x4 positionMatrix = Matrix4x4.TRS(capsuleCollider.bounds.center,
                capsuleCollider.transform.rotation * Quaternion.Euler(axisRotation),
                capsuleCollider.transform.lossyScale * scale);

            if (ShowDebugColliders)
                Graphics.DrawMesh(capsuleMesh, positionMatrix, _touchReactMaterial, 0, null);

            Graphics.DrawMesh(capsuleMesh, positionMatrix, _touchReactMaterial, InvisibleLayer, TouchReactCamera);
        }

        private void DrawSphereCollider(SphereCollider sphereCollider, float scale)
        {
            float sphereScale = GetMaxValue(sphereCollider.transform.lossyScale);
            Vector3 sphereScaleVector = new Vector3(sphereScale, sphereScale, sphereScale);

            Matrix4x4 positionMatrix = Matrix4x4.TRS(sphereCollider.bounds.center, sphereCollider.transform.rotation,
                sphereScaleVector * sphereCollider.radius * scale);

            if (ShowDebugColliders)
                Graphics.DrawMesh(_sphereColliderMesh, positionMatrix, _touchReactMaterial, 0, null);
            Graphics.DrawMesh(_sphereColliderMesh, positionMatrix, _touchReactMaterial, InvisibleLayer, TouchReactCamera);
        }

        private void DrawMeshCollider(MeshCollider meshCollider)
        {
            Matrix4x4 positionMatrix = Matrix4x4.TRS(meshCollider.transform.position, meshCollider.transform.rotation,
                meshCollider.transform.lossyScale);

            Mesh mesh = meshCollider.sharedMesh;
            if (mesh)
            {
                if (ShowDebugColliders)
                    Graphics.DrawMesh(mesh, positionMatrix, _touchReactMaterial, 0, null);
                Graphics.DrawMesh(mesh, positionMatrix, _touchReactMaterial, InvisibleLayer, TouchReactCamera);
            }
        }

        private float GetMaxValue(Vector3 vector)
        {
            float returnValue = float.MinValue;
            if (returnValue < vector.x) returnValue = vector.x;
            if (returnValue < vector.y) returnValue = vector.y;
            if (returnValue < vector.z) returnValue = vector.z;
            return returnValue;
        }

        private void DrawMeshfilter(MeshFilter meshfilter)
        {
            Matrix4x4 positionMatrix = Matrix4x4.TRS(meshfilter.transform.position, meshfilter.transform.rotation,
                meshfilter.transform.lossyScale);

            Mesh mesh = meshfilter.sharedMesh;
            if (mesh)
            {
                if (ShowDebugColliders)
                    Graphics.DrawMesh(mesh, positionMatrix, _touchReactMaterial, 0, null);

                Graphics.DrawMesh(mesh, positionMatrix, _touchReactMaterial, InvisibleLayer, TouchReactCamera);
            }
        }

        public void Init()
        {
            if (TouchReactCamera) TouchReactCamera.enabled = true;

            if (!TouchReactCamera) CreateTouchReactCamera();

            UpdateTouchReactCamera();

            if (AutoselectCamera) SelectedCamera = Camera.main;
            SetupMaterial();
            UpdateCamera();
        }

        public void UpdateTouchReactCamera()
        {
            if (TouchReactCamera && HideTouchReactCamera)
            {
                TouchReactCamera.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
            else
            {
                TouchReactCamera.gameObject.hideFlags = HideFlags.None;
            }
        }

        private void SetupMaterial()
        {
            _touchReactMaterial =
                new Material(Shader.Find("AwesomeTechnologies/TouchReact/RenderTouchBuffer"))
                {
#if UNITY_5_6_OR_NEWER
                    enableInstancing = true
#endif
                };

            _touchreactMaterialInstanced =
                new Material(Shader.Find("AwesomeTechnologies/TouchReact/RenderTouchBufferInstanced"))
                {
#if UNITY_5_6_OR_NEWER
                    enableInstancing = true
#endif
                };


            _touchreactMaterialPropertyBlock = new MaterialPropertyBlock();
        }

        private void CreateTouchReactCamera()
        {
            Transform tourchBendCameraTransform = transform.Find("TouchReactCamera");

            if (!tourchBendCameraTransform)
            {
                GameObject touchReactCameraObject = new GameObject("TouchReactCamera");
                touchReactCameraObject.transform.SetParent(transform, false);
                touchReactCameraObject.transform.position = Vector3.zero;
                touchReactCameraObject.transform.rotation = Quaternion.LookRotation(Vector3.up);

                Camera tempEditorCamera = touchReactCameraObject.AddComponent<Camera>();
                tempEditorCamera.farClipPlane = 10000;
                tempEditorCamera.nearClipPlane = -10000;
                tempEditorCamera.depth = -100;
                tempEditorCamera.clearFlags = CameraClearFlags.Color;
                tempEditorCamera.backgroundColor = Color.black;
                tempEditorCamera.renderingPath = RenderingPath.Forward;
                tempEditorCamera.useOcclusionCulling = true;
                tempEditorCamera.orthographic = true;
                tempEditorCamera.orthographicSize = 50f;
                tempEditorCamera.cullingMask = 1 << InvisibleLayer;
#if UNITY_5_6_OR_NEWER
                tempEditorCamera.allowMSAA = false;
                tempEditorCamera.allowHDR = false;
#endif
                tempEditorCamera.stereoTargetEye = StereoTargetEyeMask.None;
                TouchReactCamera = tempEditorCamera;                
            }
            else
            {
                TouchReactCamera = tourchBendCameraTransform.gameObject.GetComponent<Camera>();
            }
        }

        public Vector3 GetCameraPosition()
        {
            if (Application.isPlaying)
                if (SelectedCamera)
                    return SelectedCamera.transform.position;
                else
                    return Vector3.zero;
#if UNITY_EDITOR
            if (TrSceneViewDetector.GetCurrentSceneViewCamera())
            {
                return TrSceneViewDetector.GetCurrentSceneViewCamera().transform.position;
            }
            //Camera[] sceneviewCameras = SceneView.GetAllSceneCameras();
            //if (sceneviewCameras.Length > 0)
            //    return sceneviewCameras[0].transform.position;
            return Vector3.zero;

#else
  return Vector3.zero;
#endif
        }

        public void InstanceAddCollider(TouchColliderInfo touchColliderInfo)
        {
            if (!ColliderList.Contains(touchColliderInfo)) ColliderList.Add(touchColliderInfo);
        }

        public void InstanceRemoveCollider(TouchColliderInfo touchColliderInfo)
        {
            ColliderList.Remove(touchColliderInfo);
        }

        public void InstanceAddMeshFilter(MeshFilter meshFilter)
        {
            if (!MeshFilterList.Contains(meshFilter)) MeshFilterList.Add(meshFilter);
        }

        public void InstanceDrawMeshInstanced(Mesh mesh, List<Matrix4x4> instanceList, int subMeshIndex)
        {
            if (_touchreactMaterialInstanced)
            {
                if (ShowDebugColliders)
                {
                    Graphics.DrawMeshInstanced(mesh, subMeshIndex, _touchreactMaterialInstanced, instanceList,
                        _touchreactMaterialPropertyBlock, ShadowCastingMode.Off, false, 0, null);
                }               

                Graphics.DrawMeshInstanced(mesh, subMeshIndex, _touchreactMaterialInstanced, instanceList,
                    _touchreactMaterialPropertyBlock, ShadowCastingMode.Off, false,InvisibleLayer,TouchReactCamera);
            }
        }

        public void InstanceRemoveMeshFilter(MeshFilter meshFilter)
        {
            MeshFilterList.Remove(meshFilter);
        }

        public static void FindInstance()
        {
            Instance = FindObjectOfType<TouchReactSystem>();
        }

        public static void AddCollider(TouchColliderInfo touchColliderInfo)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.InstanceAddCollider(touchColliderInfo);
        }

        public static void RemoveCollider(TouchColliderInfo touchColliderInfo)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.InstanceRemoveCollider(touchColliderInfo);
        }

        public static void AddMeshFilter(MeshFilter mesh)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.InstanceAddMeshFilter(mesh);
        }

        public static void RemoveMeshFilter(MeshFilter mesh)
        {
            if (!Instance) FindInstance();
            if (Instance) Instance.InstanceRemoveMeshFilter(mesh);
        }


        public static bool TouchReactEnabled()
        {
            if (Instance)
            {
                if (Instance.isActiveAndEnabled) return true;                                  
            }

            return false;
        }

        public static void DrawMeshInstanced(Mesh mesh, List<Matrix4x4> instanceList, int subMeshIndex)
        {
            if (Instance)
            {
                if (Instance.isActiveAndEnabled)
                {
                    Instance.InstanceDrawMeshInstanced(mesh, instanceList, subMeshIndex);
                }
            }
        }
    }
}
