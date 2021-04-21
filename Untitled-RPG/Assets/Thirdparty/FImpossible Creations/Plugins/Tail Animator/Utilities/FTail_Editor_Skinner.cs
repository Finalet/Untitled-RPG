#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    /// <summary>
    /// FCr: Implementation of Tail Animator Skinning Static Meshes API
    /// Class to use only in editor, it creates bones with preview static mesh then skin it to skinned mesh renderer
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("FImpossible Creations/Tail Animator Utilities/Editor Tail Skinner")]
    public class FTail_Editor_Skinner : MonoBehaviour
    {

        #region Inspector Variables

        [FPD_Header("SKIN STATIC MESHES INSIDE UNITY", 3, 8, 8)]
        [BackgroundColor(0.75f, 0.75f, 1.0f, 0.7f)]
        public int AutoMarkersCount = 8;
        public float DistanceValue = 0.3f;
        public Vector3 positionOffset = new Vector3(0, 0f);
        public Vector2 startDirection = new Vector2(-90, 0f);
        public Vector2 rotationOffset = new Vector2(0f, 0f);

        [Range(0f, 5f)]
        public float HelpScaleValue = 1f;

        [BackgroundColor(0.85f, 0.85f, 1.0f, 0.85f)]
        public AnimationCurve DistancesFaloff = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        public AnimationCurve RotationsFaloff = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [BackgroundColor(0.5f, 1f, 0.5f, 0.8f)]
        [Space(10f, order = 0)]
        [Header("Left empty if you don't use custom markers", order = 1)]
        [Space(-7f, order = 2)]
        [Header("Moving custom markers will not trigger realtime update", order = 3)]
        public Transform[] CustomBoneMarkers;

        [Space(7f, order = 0)]
        [FPD_Header("Weights Spread Settings", 7, 4, 4)]
        [Space(3f, order = 2)]
        [Range(0f, 1f)]
        public float SpreadValue = 0.8f;
        [Range(0f, 1f)]
        public float SpreadPower = .185f;
        [Tooltip("Offsetting spreading area, For example 0,0,1 and recommended values from 0 to 2 not bigger")]
        public Vector3 SpreadOffset = Vector3.zero;
        [Range(1, 2)]
        public int LimitBoneWeightCount = 2;

        [BackgroundColor(0.4f, 0.8f, 0.8f, 0.8f)]
        [FPD_Header("Additional Variables", 7, 4, 4)]
        [Range(0f, 5f)]
        public float GizmoSize = 0.1f;
        [Range(0f, 1f)]
        public float GizmoAlpha = .65f;

        [BackgroundColor()]
        [Tooltip("If your model have many vertices, turn it only when neccesary")]
        public bool RealtimeUpdate = true;
        public bool ShowPreview = true;
        public bool DebugMode = false;

        #endregion

        #region Private variables

        /// <summary> Base Mesh </summary>
        private Mesh baseMesh;

        [HideInInspector]
        public List<Color32> baseVertexColor;

        private MeshRenderer meshRenderer;


        /// <summary> Fake bones list before creating true skeleton for mesh </summary>
        private Transform[] ghostBones;

        /// <summary> Vertex datas used for setting weights precisely</summary>
        private FTail_SkinningVertexData[] vertexDatas;

        /// <summary> Generated marker points for automatic bone points </summary>
        internal Transform[] autoMarkers;

        // Hide in inspector because when variables are private, they're resetted to null every time code compiles
        /// <summary> Because we can't destroy gameObjects in OnValidate, we do something similar to object pools </summary>
        [HideInInspector]
        public List<Transform> allMarkersTransforms = new List<Transform>();

        /// <summary> Transform with components helping drawing how weights are spread on model </summary>
        [HideInInspector]
        public Transform weightPreviewTransform;

        [HideInInspector]
        public bool popupShown = false;

        internal bool initValues = false;

        #endregion


        /// <summary>
        /// When something changes in inspector, let's recalculate parameters
        /// </summary>
        private void OnValidate()
        {
            if (!initValues)
            {
                MeshRenderer m = GetComponent<MeshRenderer>();
                if (m) DistanceValue = m.bounds.extents.magnitude / 7f;
                initValues = true;
            }

            if (AutoMarkersCount < 2) AutoMarkersCount = 2;

            if (!GetBaseMesh()) return;

            if (CustomBoneMarkers == null) CustomBoneMarkers = new Transform[0]; // Prevent error log when adding component

            // Use only custom markers if they're assigned
            if (CustomBoneMarkers.Length > 0)
                ghostBones = CustomBoneMarkers;
            else // Use auto markers
            {
                CalculateAutoMarkers();
                ghostBones = autoMarkers;
            }

            if (RealtimeUpdate)
            {
                vertexDatas = FTail_Skinning.CalculateVertexWeightingData(
                    GetBaseMesh(), ghostBones, SpreadOffset, LimitBoneWeightCount, SpreadValue, SpreadPower);

                UpdatePreviewMesh();
            }
        }


        /// <summary>
        /// Drawing helper stuff
        /// </summary>
        private void OnDrawGizmos()
        {
            if (CustomBoneMarkers == null)
                CustomBoneMarkers = new Transform[0];

            if (ghostBones[0] == null)
                CalculateAutoMarkers();

            if (CustomBoneMarkers.Length < 1)
                DrawMarkers(autoMarkers);
            else
                DrawMarkers(CustomBoneMarkers);
        }


        /// <summary>
        /// Calculating auto markers transforms
        /// </summary>
        private void CalculateAutoMarkers()
        {
            #region Creation of markers' transforms

            if (autoMarkers == null) autoMarkers = new Transform[0];

            if (allMarkersTransforms.Count < AutoMarkersCount)
            {
                for (int i = autoMarkers.Length; i < AutoMarkersCount; i++)
                {
                    GameObject newMarker = new GameObject(name + "-SkinMarker " + i);
                    newMarker.transform.SetParent(transform, true);
                    allMarkersTransforms.Add(newMarker.transform);
                }
            }

            if (autoMarkers.Length != AutoMarkersCount)
            {
                autoMarkers = new Transform[AutoMarkersCount];
                for (int i = 0; i < AutoMarkersCount; i++)
                {
                    autoMarkers[i] = allMarkersTransforms[i];
                }
            }

            #endregion

            autoMarkers[0].position = transform.position + positionOffset;
            autoMarkers[0].rotation = Quaternion.Euler(startDirection + rotationOffset);

            float step = 1f / (float)AutoMarkersCount;

            for (int i = 1; i < AutoMarkersCount; i++)
            {
                float forwardMultiplier = DistanceValue;
                forwardMultiplier *= DistancesFaloff.Evaluate(i * step);
                forwardMultiplier *= HelpScaleValue;
                Vector3 targetPosition = autoMarkers[i - 1].position + autoMarkers[i - 1].rotation * Vector3.forward * forwardMultiplier;

                Vector3 newRot = startDirection + rotationOffset * (i + 1) * RotationsFaloff.Evaluate(i * step);

                autoMarkers[i].position = targetPosition;
                autoMarkers[i].rotation = Quaternion.Euler(newRot);
            }
        }


        /// <summary>
        /// Getting base mesh variable, depends if it's skinned mesh or static mesh
        /// </summary>
        private Mesh GetBaseMesh()
        {
            if (baseMesh == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                if (meshFilter) baseMesh = meshFilter.sharedMesh;
            }
            else return baseMesh;

            if (!baseMesh)
            {
                if (!popupShown)
                {
                    EditorUtility.DisplayDialog("Tail Skinner Error", "[Tail Skinner] No base mesh! (mesh filter and mesh renderer)", "Ok");
                    popupShown = true;
                }

                Debug.LogError("No BaseMesh!");
            }

            if (baseMesh)
            {
                if (baseVertexColor == null) baseVertexColor = new List<Color32>();
                if (baseVertexColor.Count != baseMesh.vertexCount)
                {
                    baseVertexColor.Clear();
                    baseMesh.GetColors(baseVertexColor);
                }
            }

            return baseMesh;
        }


        /// <summary>
        /// Skinning mesh to new skinned mesh renderer with choosed weight markers settings
        /// </summary>
        public void SkinMesh(bool addTailAnimator, Vector3 newObjectOffset)
        {
            // Remembering data and preparing objects with components
            List<Color32> baseVertColors = new List<Color32>();
            GetBaseMesh().GetColors(baseVertColors);

            // Doing skinning
            vertexDatas = FTail_Skinning.CalculateVertexWeightingData(
                GetBaseMesh(), ghostBones, SpreadOffset, LimitBoneWeightCount, SpreadValue, SpreadPower);

            SkinnedMeshRenderer newSkinnedMesh = FTail_Skinning.SkinMesh(baseMesh, transform, ghostBones, vertexDatas);

            if (newSkinnedMesh == null)
            { Debug.LogError("[Tail Animator Skinning] Creating skinned mesh failed!"); return; }


            // Skin renderer quality
            switch (LimitBoneWeightCount)
            {
                case 1: newSkinnedMesh.quality = SkinQuality.Bone1; break;
                case 2: newSkinnedMesh.quality = SkinQuality.Bone2; break;
                case 4: newSkinnedMesh.quality = SkinQuality.Bone4; break;
                default: newSkinnedMesh.quality = SkinQuality.Auto; break;
            }

            // Filling new mesh with materials
            MeshRenderer meshRend = GetComponent<MeshRenderer>();
            if (meshRend)
            {
                newSkinnedMesh.materials = meshRend.sharedMaterials;
                newSkinnedMesh.sharedMaterials = meshRend.sharedMaterials;
            }

            // Adding tail animator
            if (addTailAnimator)
            {
                TailAnimator2 t = newSkinnedMesh.bones[0].gameObject.AddComponent<TailAnimator2>();
                t.StartBone = newSkinnedMesh.bones[0];
                t.EndBone = newSkinnedMesh.bones[newSkinnedMesh.bones.Length - 1];
            }

            // Setting new object position to be next to current model
            newSkinnedMesh.transform.position = transform.position + new Vector3(1f, 1f, 1f);

            // Create asset for new model so it not disappear when we create prefab from this gameObject
            string newMeshPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(baseMesh));
            AssetDatabase.CreateAsset(newSkinnedMesh.sharedMesh, newMeshPath + "/" + newSkinnedMesh.name + ".mesh");
            AssetDatabase.SaveAssets();

            Debug.Log("New skinned mesh '" + newSkinnedMesh.name + ".mesh" + "' saved under path: '" + newMeshPath + "'");
        }


        /// <summary>
        /// Make sure everything which was created by this script is destroyed
        /// </summary>
        private void OnDestroy()
        {
            for (int i = 0; i < allMarkersTransforms.Count; i++) if (allMarkersTransforms[i] != null) DestroyImmediate(allMarkersTransforms[i].gameObject);
            if (weightPreviewTransform != null) DestroyImmediate(weightPreviewTransform.gameObject);

            if (baseMesh == null) return;
            meshRenderer.enabled = true;
        }


        /// <summary>
        /// Drawing markers to be visible in editor window to help place bones correctly
        /// </summary>
        public void DrawMarkers(Transform[] markers)
        {
            if (markers == null) return;

            for (int i = 0; i < markers.Length; i++)
            {
                Gizmos.color = ChangeColorAlpha(GetBoneIndicatorColor(i, markers.Length), GizmoAlpha);

                Vector3 targetPosition = markers[i].position;

                Gizmos.DrawWireSphere(targetPosition, GizmoSize);

                Gizmos.color = ChangeColorAlpha(GetBoneIndicatorColor(i, markers.Length, 1f, 1f), GizmoAlpha * 0.8f);
                Gizmos.DrawSphere(targetPosition, GizmoSize * 0.7f);

                Gizmos.DrawRay(targetPosition, markers[i].up * GizmoSize * 1.1f);
                Gizmos.DrawRay(targetPosition, -markers[i].up * GizmoSize * 1.1f);
                Gizmos.DrawRay(targetPosition, markers[i].right * GizmoSize * 1.1f);
                Gizmos.DrawRay(targetPosition, -markers[i].right * GizmoSize * 1.1f);

                Vector3 targetPoint;
                if (i < markers.Length - 1) targetPoint = markers[i + 1].position;
                else
                    targetPoint = markers[i].position + (markers[i].position - markers[i - 1].position);

                Gizmos.DrawLine(targetPosition + markers[i].up * GizmoSize * 1.1f, targetPoint);
                Gizmos.DrawLine(targetPosition - markers[i].up * GizmoSize * 1.1f, targetPoint);
                Gizmos.DrawLine(targetPosition + markers[i].right * GizmoSize * 1.1f, targetPoint);
                Gizmos.DrawLine(targetPosition - markers[i].right * GizmoSize * 1.1f, targetPoint);
            }
        }


        /// <summary>
        /// Updating preview mesh to view weights correctly
        /// </summary>
        private void UpdatePreviewMesh()
        {
            #region Creation of new preview mesh when needed

            if (weightPreviewTransform == null)
            {
                weightPreviewTransform = new GameObject(name + "[preview mesh]").transform;
                weightPreviewTransform.SetParent(transform);
                weightPreviewTransform.localPosition = Vector3.zero;
                weightPreviewTransform.localRotation = Quaternion.identity;
                weightPreviewTransform.localScale = Vector3.one;

                weightPreviewTransform.gameObject.AddComponent<MeshFilter>().mesh = baseMesh;

                Material[] newMaterials = new Material[meshRenderer.sharedMaterials.Length];

                for (int i = 0; i < newMaterials.Length; i++) newMaterials[i] = new Material(Shader.Find("Particles/FVertexLit Blended"));
                weightPreviewTransform.gameObject.AddComponent<MeshRenderer>().materials = newMaterials;
            }

            #endregion

            if (ShowPreview)
            {
                meshRenderer.enabled = false;
                weightPreviewTransform.gameObject.SetActive(true);
                List<Color> vColors = new List<Color>();
                for (int i = 0; i < vertexDatas.Length; i++) vColors.Add(vertexDatas[i].GetWeightColor());
                baseMesh.SetColors(vColors);
                weightPreviewTransform.gameObject.GetComponent<MeshFilter>().mesh = baseMesh;
            }
            else
            {
                meshRenderer.enabled = true;
                if (baseVertexColor != null) if (baseMesh) if (baseMesh.vertexCount == baseVertexColor.Count) baseMesh.SetColors(baseVertexColor);
                weightPreviewTransform.gameObject.SetActive(false);
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (!DebugMode) return;
            if (vertexDatas == null) return;
            if (vertexDatas.Length == 0) return;
            if (vertexDatas[0].bonesIndexes == null) return;

            for (int i = 0; i < vertexDatas.Length; i++)
            {
                Handles.color = GetBoneIndicatorColor(vertexDatas[i].bonesIndexes[0], vertexDatas[i].bonesIndexes.Length) * new Color(1f, 1f, 1f, GizmoAlpha);
                Gizmos.color = Handles.color;

                Handles.Label(transform.TransformPoint(vertexDatas[i].position), "[" + i + "]");
                //Handles.Label(transform.TransformPoint(vertexDatas[i].position), "[" + i + "]\n" + Math.Round(vertexDatas[i].debugDists[0], 3) + "\n" + Math.Round(vertexDatas[i].debugDists[1], 3));
                Gizmos.DrawSphere(transform.TransformPoint(vertexDatas[i].position), 0.125f * GizmoSize);
            }
        }


        /// <summary>
        /// Returning helper color for bone
        /// </summary>
        public static Color GetBoneIndicatorColor(int boneIndex, int bonesCount, float s = 0.9f, float v = 0.9f)
        {
            float h = ((float)(boneIndex) * 1.125f) / bonesCount;
            h += 0.125f * boneIndex;
            h += 0.3f;
            h %= 1f;
            return Color.HSVToRGB(h, s, v);
        }


        public static Color ChangeColorAlpha(Color color, float alpha)
        { return new Color(color.r, color.g, color.b, alpha); }

    }

    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [UnityEditor.CustomEditor(typeof(FTail_Editor_Skinner))]
    public class FTail_Editor_SkinnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FTail_Editor_Skinner targetScript = (FTail_Editor_Skinner)target;
            DrawDefaultInspector();

            GUILayout.Space(10f);

            if (GUILayout.Button("Skin It")) targetScript.SkinMesh(false, Vector3.right);
            if (GUILayout.Button("Skin and add Tail Animator")) targetScript.SkinMesh(true, Vector3.right);
        }
    }

}

#endif
