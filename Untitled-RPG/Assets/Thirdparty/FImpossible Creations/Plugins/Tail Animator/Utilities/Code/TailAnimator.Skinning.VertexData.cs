using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail
{
    /// <summary>
    /// FCr: Part of Tail Animator Skinning Static Meshes API
    /// Simple helper class to store vertices parameters in reference to bones
    /// </summary>
    [System.Serializable]
    public class FTail_SkinningVertexData
    {
        public Vector3 position;
        //public Transform[] bones;

        /// <summary> Indexes for helpers in visualization </summary>
        public int[] bonesIndexes;
        public int allMeshBonesCount;

        // Assigned during custom weight calculations
        public float[] weights;

        public FTail_SkinningVertexData(Vector3 pos) { position = pos; }

        /// <summary>
        /// Distance to bone area for weighting
        /// </summary>
        public float DistanceToLine(Vector3 pos, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 dirVector1 = pos - lineStart;
            Vector3 dirVector2 = (lineEnd - lineStart).normalized;

            float distance = Vector3.Distance(lineStart, lineEnd);
            float dot = Vector3.Dot(dirVector2, dirVector1);

            if (dot <= 0) return Vector3.Distance(pos, lineStart);
            if (dot >= distance) return Vector3.Distance(pos, lineEnd);

            Vector3 dotVector = dirVector2 * dot;
            Vector3 closestPoint = lineStart + dotVector;

            return Vector3.Distance(pos, closestPoint);
        }


        /// <summary>
        /// Calculating vertex's distances to 4 nearest bones (4 bone weights is maximum count in Unity)
        /// for further custom weight calculations
        /// </summary>
        public void CalculateVertexParameters(Vector3[] bonesPos, Quaternion[] bonesRot, Vector3[] boneAreas, int maxWeightedBones, float spread, Vector3 spreadOffset, float spreadPower = 1f)
        {
            allMeshBonesCount = bonesPos.Length;

            // Using Vector2 for simple two float values in one variable, x = bone index y = distance of vertex to this bone, later we will sort list using distances
            List<Vector2> calculatedDistances = new List<Vector2>();

            // Check later if we don't need to transpone points to model space scale
            for (int i = 0; i < bonesPos.Length; i++)
            {
                Vector3 boneEnd;
                if (i != bonesPos.Length - 1)
                    boneEnd = Vector3.Lerp(bonesPos[i], bonesPos[i + 1], 0.9f);
                else
                    boneEnd = Vector3.Lerp(bonesPos[i], bonesPos[i] + (bonesPos[i] - bonesPos[i - 1]), 0.9f);

                boneEnd += bonesRot[i] * spreadOffset;

                float distance = DistanceToLine(position, bonesPos[i], boneEnd);

                // Making bone offset to behave like bone area
                calculatedDistances.Add(new Vector2(i, distance));
            }

            // Sorting by nearest all bones
            calculatedDistances.Sort((a, b) => a.y.CompareTo(b.y));

            // Limiting vertex weight up to 4 bones
            int maxBones = (int)Mathf.Min(maxWeightedBones, bonesPos.Length);

            // Assigning max 4 nearest bones and their distances to this vertex
            bonesIndexes = new int[maxBones];
            float[] nearestDistances = new float[maxBones];

            for (int i = 0; i < maxBones; i++)
            {
                bonesIndexes[i] = (int)calculatedDistances[i].x;
                nearestDistances[i] = calculatedDistances[i].y;
            }

            // Basing on spread value we spreading weight to nearest bones
            // Calculating percentage distances to bones
            float[] boneWeightsForVertex = new float[maxBones];


            AutoSetBoneWeights(boneWeightsForVertex, nearestDistances, spread, spreadPower, boneAreas);


            float weightLeft = 1f; // Must amount of weight which needs to be assigned
            weights = new float[maxBones]; // New weight parameters

            // Applying weights to each bone assigned to vertex
            for (int i = 0; i < maxBones; i++)
            {
                if (spread == 0) if (i > 0) break;

                if (weightLeft <= 0f) // No more weight to apply
                {
                    weights[i] = 0f;
                    continue;
                }

                float targetWeight = boneWeightsForVertex[i];

                weightLeft -= targetWeight;
                if (weightLeft <= 0f) targetWeight += weightLeft; else { if (i == maxBones - 1) targetWeight += weightLeft; } // Using weight amount which is left to assign

                weights[i] = targetWeight;
            }
        }


        public float[] debugDists;
        public float[] debugDistWeights;
        public float[] debugWeights;

        /// <summary>
        /// Spreading weights over bones for current vertex
        /// </summary>
        public void AutoSetBoneWeights(float[] weightForBone, float[] distToBone, float spread, float spreadPower, Vector3[] boneAreas)
        {
            int bonesC = weightForBone.Length;
            float[] boneLengths = new float[bonesC]; for (int i = 0; i < boneLengths.Length; i++) boneLengths[i] = boneAreas[i].magnitude;
            float[] normalizedDistanceWeights = new float[bonesC];
            for (int i = 0; i < weightForBone.Length; i++) weightForBone[i] = 0f;

            float normalizeDistance = 0f;
            for (int i = 0; i < bonesC; i++) normalizeDistance += distToBone[i];
            for (int i = 0; i < bonesC; i++) normalizedDistanceWeights[i] = 1f - distToBone[i] / normalizeDistance; // Reversing weight power - nearest (smallest distance) must have biggest weight value

            debugDists = distToBone;

            if (bonesC == 1 || spread == 0f) // Simpliest ONE BONE -------------------------------------------------------------
            {
                // [0] - nearest bone
                weightForBone[0] = 1f; // Just one weight - spread does not change anything
            }
            else if (bonesC == 2) // Simple TWO BONES -------------------------------------------------------------
            {
                float normalizer = 1f;
                weightForBone[0] = 1f;

                // distToBone[0] is zero, max spread distance is length of bone / 3 
                float distRange = Mathf.InverseLerp(distToBone[0] + (boneLengths[0] / 1.25f) * spread, distToBone[0], distToBone[1]);
                debugDists[0] = distRange;

                // 0 -> full nearest bone weight
                // 1 -> half nearest half second bone weight
                float value = DistributionIn(Mathf.Lerp(0f, 1f, distRange), Mathf.Lerp(1.5f, 16f, spreadPower));

                weightForBone[1] = value;
                normalizer += value;

                debugDistWeights = new float[weightForBone.Length];

                weightForBone.CopyTo(debugDistWeights, 0);

                for (int i = 0; i < bonesC; i++) weightForBone[i] /= normalizer;

                debugWeights = weightForBone;
            }
            else // Complex > TWO BONES -------------------------------------------------------------
            {
                float reffVal = boneLengths[0] / 10f;
                float refLength = boneLengths[0] / 2f;
                float normalizer = 0f;

                for (int i = 0; i < bonesC; i++)
                {
                    float weight = Mathf.InverseLerp(0f, reffVal + refLength * (spread), distToBone[i]);
                    float value = Mathf.Lerp(1f, 0f, weight);
                    if (i == 0) if (value == 0f) value = 1f;

                    weightForBone[i] = value;

                    normalizer += value;
                }

                debugDistWeights = new float[weightForBone.Length];
                weightForBone.CopyTo(debugDistWeights, 0);

                for (int i = 0; i < bonesC; i++) weightForBone[i] /= normalizer;

                debugWeights = weightForBone;
            }
        }

        /// <summary>
        /// Easing weight distribution
        /// </summary>
        public static float DistributionIn(float k, float power)
        { return Mathf.Pow(k, power + 1f); }


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

        /// <summary>
        /// Returns average color value for weight idicator for this vertex
        /// </summary>
        public Color GetWeightColor()
        {
            Color lerped = GetBoneIndicatorColor(bonesIndexes[0], allMeshBonesCount, 1f, 1f);

            for (int i = 1; i < bonesIndexes.Length; i++)
                lerped = Color.Lerp(lerped, GetBoneIndicatorColor(bonesIndexes[i], allMeshBonesCount, 1f, 1f), weights[i]);

            return lerped;
        }
    }

}