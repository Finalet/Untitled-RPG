using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace FIMSpace.FTail
{
    /// <summary>
    /// FCr: Part of Tail Animator Skinning Static Meshes API
    /// Methods used by skinner to create skinned mesh renderers from static meshes
    /// </summary>
    public static class FTail_Skinning
    {

        /// <summary>
        /// Calculating base vertices datas for provided bones setup
        /// </summary>
        /// <param name="baseMesh"> Mesh to be weighted </param>
        /// <param name="bonesCoords"> Required local positions and rotations for bones </param>
        /// <param name="spreadOffset"> Origin weighting offset which can be helpful in some cases, it can be Vector3.zero in most cases </param>
        /// <param name="weightBoneLimit"> To how many bones vertex can be weighted to create smooth weight effect </param>
        /// <param name="spreadValue"> Smoothing weights on the edges of bones if lower then more smooth but don't oversmooth it </param>
        /// <param name="spreadPower"> Making smoothing more sharp on edges </param>
        public static FTail_SkinningVertexData[] CalculateVertexWeightingData(Mesh baseMesh, Transform[] bonesCoords, Vector3 spreadOffset, int weightBoneLimit = 2, float spreadValue = 0.8f, float spreadPower = 0.185f)
        {
            Vector3[] pos = new Vector3[bonesCoords.Length];
            Quaternion[] rot = new Quaternion[bonesCoords.Length];

            //for (int i = 0; i < bonesCoords.Length; i++)
            //{
            //    pos[i] = bonesCoords[i].position;
            //    rot[i] = bonesCoords[i].rotation;
            //}

            // We must reset bones structure to identity space
            for (int i = 0; i < bonesCoords.Length; i++)
            {
                // Transforming from world to local space coords
                pos[i] = bonesCoords[0].parent.InverseTransformPoint(bonesCoords[i].position);
                rot[i] = FEngineering.QToLocal(bonesCoords[0].parent.rotation, bonesCoords[i].rotation);
            }

            return CalculateVertexWeightingData(baseMesh, pos, rot, spreadOffset, weightBoneLimit, spreadValue, spreadPower);
        }


        /// <summary>
        /// Calculating base vertices datas for provided bones setup
        /// </summary>
        /// <param name="baseMesh"> Mesh to be weighted </param>
        /// <param name="bonesPos"> Mesh local space positions for bones </param>
        /// <param name="bonesRot"> Mesh local space rotations for bones </param>
        /// <param name="bonesCoords"> Required local positions and rotations for bones </param>
        /// <param name="spreadOffset"> Origin weighting offset which can be helpful in some cases, it can be Vector3.zero in most cases </param>
        /// <param name="weightBoneLimit"> To how many bones vertex can be weighted to create smooth weight effect </param>
        /// <param name="spreadValue"> Smoothing weights on the edges of bones if lower then more smooth but don't oversmooth it </param>
        /// <param name="spreadPower"> Making smoothing more sharp on edges </param>
        public static FTail_SkinningVertexData[] CalculateVertexWeightingData(Mesh baseMesh, Vector3[] bonesPos, Quaternion[] bonesRot, Vector3 spreadOffset, int weightBoneLimit = 2, float spreadValue = 0.8f, float spreadPower = 0.185f)
        {
            if (weightBoneLimit < 1) weightBoneLimit = 1;
            if (weightBoneLimit > 2) weightBoneLimit = 2; // Limiting for now

            #region Editor progress dialogs
#if UNITY_EDITOR
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
#endif
            #endregion

            int vertCount = baseMesh.vertexCount;
            FTail_SkinningVertexData[] vertexDatas = new FTail_SkinningVertexData[vertCount];

            // Computing helper segments for weighting bones
            Vector3[] boneAreas = new Vector3[bonesPos.Length];
            for (int i = 0; i < bonesPos.Length - 1; i++)
            {
                // Direction vector towards further bone
                boneAreas[i] = bonesPos[i + 1] - bonesPos[i]; //bonesCoords[i + 1].localPosition - bonesCoords[i].localPosition;
            }

            if (boneAreas.Length > 1) boneAreas[boneAreas.Length - 1] = boneAreas[boneAreas.Length - 2];


#if UNITY_EDITOR
            try
            {
                for (int i = 0; i < vertCount; i++)
                {
                    vertexDatas[i] = new FTail_SkinningVertexData(baseMesh.vertices[i]);
                    vertexDatas[i].CalculateVertexParameters(bonesPos, bonesRot, boneAreas, weightBoneLimit, spreadValue, spreadOffset, spreadPower);

                    #region Editor progress dialogs
                    if (!Application.isPlaying)
                        // Displaying progress bar when iteration takes too much time
                        if (watch.ElapsedMilliseconds > 1500)
                            if (i % 10 == 0)
                                EditorUtility.DisplayProgressBar("Analizing mesh vertices...", "Analizing Vertices (" + i + "/" + vertCount + ")", ((float)i / (float)vertCount));
                    #endregion
                }

                #region Editor progress dialogs
                if (!Application.isPlaying)
                    EditorUtility.ClearProgressBar();
                #endregion
            }
            catch (System.Exception exc)
            {
                Debug.LogError(exc);
                #region Editor progress dialogs
                if (!Application.isPlaying)
                    EditorUtility.ClearProgressBar();
                #endregion
            }
#else
            for (int i = 0; i < vertCount; i++)
            {
                vertexDatas[i] = new FTail_SkinningVertexData(baseMesh.vertices[i]);
                vertexDatas[i].CalculateVertexParameters(bonesPos, bonesRot, boneAreas, weightBoneLimit, spreadValue, spreadOffset, spreadPower);
            }
#endif

            return vertexDatas;
        }


        /// <summary>
        /// Skinning target mesh with helper vertex datas which you can get with CalculateVertexWeightingData() method
        /// Using transforms as guidement for bones positions and rotations
        /// </summary>
        /// <returns> Skinned mesh can't be returned as 'Mesh' type because skinned mesh is mesh + bones transforms etc. </returns>
        public static SkinnedMeshRenderer SkinMesh(Mesh baseMesh, Transform skinParent, Transform[] bonesStructure, FTail_SkinningVertexData[] vertData)
        {
            Vector3[] pos = new Vector3[bonesStructure.Length];
            Quaternion[] rot = new Quaternion[bonesStructure.Length];

            // We must reset bones structure to identity space
            for (int i = 0; i < bonesStructure.Length; i++)
            {
                // Transforming from world to local space coords
                pos[i] = skinParent.InverseTransformPoint(bonesStructure[i].position);
                rot[i] = FEngineering.QToLocal(skinParent.rotation, bonesStructure[i].rotation);
            }

            SkinnedMeshRenderer skin = SkinMesh(baseMesh, pos, rot, vertData);

            return skin;
        }


        /// <summary>
        /// Skinning target mesh with helper vertex datas which you can 
        /// </summary>
        /// <param name="baseMesh"> Base static mesh to be skinned </param>
        /// <param name="bonesPositions"> Bones positions in mesh local space </param>
        /// <param name="bonesRotations"> Bones rotations in mesh local space </param>
        /// <param name="vertData"> Get it with CalculateVertexWeightingData() method </param>
        /// <returns> Skinned mesh can't be returned as 'Mesh' type because skinned mesh is mesh + bones transforms etc. </returns>
        public static SkinnedMeshRenderer SkinMesh(Mesh baseMesh, Vector3[] bonesPositions, Quaternion[] bonesRotations, FTail_SkinningVertexData[] vertData)
        {
            if (bonesPositions == null) return null;
            if (bonesRotations == null) return null;
            if (baseMesh == null) return null;
            if (vertData == null) return null;

            // Creating copy of target mesh and refreshing it
            Mesh newMesh = GameObject.Instantiate(baseMesh);
            newMesh.name = baseMesh.name + " [FSKINNED]";

            // Preparing new object which will have skinned mesh renderer with new mesh and bones in it
            GameObject newSkinObject = new GameObject(baseMesh.name + " [FSKINNED]");
            Transform newParent = newSkinObject.transform;

            // Preparing skin
            SkinnedMeshRenderer skin = newParent.gameObject.AddComponent<SkinnedMeshRenderer>();

            // Preparing bones for weighting
            Transform[] bones = new Transform[bonesPositions.Length];
            Matrix4x4[] bindPoses = new Matrix4x4[bonesPositions.Length];

            string nameString;
            if (baseMesh.name.Length < 6) nameString = baseMesh.name; else nameString = baseMesh.name.Substring(0, 5);

            for (int i = 0; i < bonesPositions.Length; i++)
            {
                bones[i] = new GameObject("BoneF-" + nameString + "[" + i + "]").transform;
                if (i == 0) bones[i].SetParent(newParent, true); else bones[i].SetParent(bones[i - 1], true);

                bones[i].transform.position = bonesPositions[i];
                bones[i].transform.rotation = bonesRotations[i];

                bindPoses[i] = bones[i].worldToLocalMatrix * newParent.localToWorldMatrix;
            }

            BoneWeight[] weights = new BoneWeight[newMesh.vertexCount];
            for (int v = 0; v < weights.Length; v++) weights[v] = new BoneWeight();

            // Calculating and applying weights for verices
            for (int i = 0; i < vertData.Length; i++)
            {
                for (int w = 0; w < vertData[i].weights.Length; w++)
                {
                    weights[i] = SetWeightIndex(weights[i], w, vertData[i].bonesIndexes[w]);
                    weights[i] = SetWeightToBone(weights[i], w, vertData[i].weights[w]);
                }
            }

            newMesh.bindposes = bindPoses;
            newMesh.boneWeights = weights;

            List<Vector3> normals = new List<Vector3>();
            List < Vector4> tangents = new List<Vector4>();
            baseMesh.GetNormals(normals);
            baseMesh.GetTangents(tangents);

            newMesh.SetNormals(normals);
            newMesh.SetTangents(tangents);
            newMesh.bounds = baseMesh.bounds;

            // Applying generated mesh to skin controller
            skin.sharedMesh = newMesh;
            skin.rootBone = bones[0];
            skin.bones = bones;

            return skin;
        }




        /// <summary>
        /// Method which is setting certain weight variable from BoneWeight struct
        /// </summary>
        public static BoneWeight SetWeightIndex(BoneWeight weight, int bone = 0, int index = 0)
        {
            switch (bone)
            {
                case 1: weight.boneIndex1 = index; break;
                case 2: weight.boneIndex2 = index; break;
                case 3: weight.boneIndex3 = index; break;
                default: weight.boneIndex0 = index; break;
            }

            return weight;
        }


        /// <summary>
        /// Method which is setting certain weight variable from BoneWeight struct
        /// </summary>
        public static BoneWeight SetWeightToBone(BoneWeight weight, int bone = 0, float value = 1f)
        {
            switch (bone)
            {
                case 1: weight.weight1 = value; break;
                case 2: weight.weight2 = value; break;
                case 3: weight.weight3 = value; break;
                default: weight.weight0 = value; break;
            }

            return weight;
        }
    }

}