using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies
{
//    [BurstCompile(CompileSynchronously = true)]
//    public struct IncludeVegetatiomMaskBeaconJob : IJob
//    {
//        public NativeList<VegetationInstance> VegetationInstanceList;
//        [ReadOnly] public NativeArray<float> FalloutCurveArray;
//        public float Denisty;
//        public float Scale;
//        public float Radius;
//        public float3 MaskPosition;
//
//        public void Execute()
//        {
//            for (int i = VegetationInstanceList.Length - 1; i >= 0; i--)
//            {
//                VegetationInstance vegetationInstance = VegetationInstanceList[i];
//                Vector2 position = new Vector2(vegetationInstance.Position.x, vegetationInstance.Position.z);
//
//                float distance = math.distance(position, new float2(MaskPosition.x, MaskPosition.z));
//                if (distance < Radius)
//                {
//                    float floatIndex = (distance / Radius);
//                    float curveDensity = SampleFalloutCurveArray(floatIndex);
//
//                    vegetationInstance.VegetationMaskScale = math.max(vegetationInstance.VegetationMaskScale, Scale);
//                    vegetationInstance.VegetationMaskDensity = math.max(vegetationInstance.VegetationMaskDensity, Denisty * curveDensity);
//                    VegetationInstanceList[i] = vegetationInstance;
//                }
//            }
//        }
//
//        private float SampleFalloutCurveArray(float value)
//        {
//            if (FalloutCurveArray.Length == 0) return 0f;
//            int index = Mathf.RoundToInt((value) * FalloutCurveArray.Length);
//            index = Mathf.Clamp(index, 0, FalloutCurveArray.Length - 1);
//            return FalloutCurveArray[index];
//        }
//    }

    [BurstCompile(CompileSynchronously = true)]
#if UNITY_2019_1_OR_NEWER
    public struct IncludeVegetatiomMaskBeaconJob : IJobParallelForDefer
#else
    public struct IncludeVegetatiomMaskBeaconJob : IJobParallelFor 
#endif

    {
        public NativeArray<byte> Excluded;
        public NativeArray<float3> Position;
        public NativeArray<float> VegetationMaskScale;
        public NativeArray<float> VegetationMaskDensity;
        [ReadOnly] public NativeArray<float> FalloutCurveArray;
        public float Denisty;
        public float Scale;
        public float Radius;
        public float3 MaskPosition;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;

            Vector2 position = new Vector2(Position[index].x, Position[index].z);

            float distance = math.distance(position, new float2(MaskPosition.x, MaskPosition.z));
            if (distance < Radius)
            {
                float floatIndex = (distance / Radius);
                float curveDensity = SampleFalloutCurveArray(floatIndex);

                VegetationMaskScale[index] = math.max(VegetationMaskScale[index], Scale);
                VegetationMaskDensity[index] = math.max(VegetationMaskDensity[index], Denisty * curveDensity);
            }
        }

        private float SampleFalloutCurveArray(float value)
        {
            if (FalloutCurveArray.Length == 0) return 0f;
            int index = Mathf.RoundToInt((value) * FalloutCurveArray.Length);
            index = Mathf.Clamp(index, 0, FalloutCurveArray.Length - 1);
            return FalloutCurveArray[index];
        }
    }

    public class BeaconMaskArea : BaseMaskArea
    {
        public float Radius;
        public Vector3 Position;
        public NativeArray<float> FalloutCurveArray;

        public void Init()
        {
            MaskBounds = GetMaskBounds();
        }

        public void SetFalloutCurve(float[] curveArray)
        {
            FalloutCurveArray = new NativeArray<float>(curveArray.Length, Allocator.Persistent);
            FalloutCurveArray.CopyFrom(curveArray);
        }

        public override JobHandle SampleMask(VegetationInstanceData instanceData,
            VegetationType vegetationType,
            JobHandle dependsOn)
        {
            return dependsOn;
        }

        public override JobHandle SampleIncludeVegetationMask(VegetationInstanceData instanceData,
            VegetationTypeIndex vegetationTypeIndex,
            JobHandle dependsOn)
        {
            VegetationTypeSettings vegetationTypeSettings = GetVegetationTypeSettings(vegetationTypeIndex);

            if (vegetationTypeSettings != null)
            {
                IncludeVegetatiomMaskBeaconJob includeVegetatiomMaskBeaconJob =
                    new IncludeVegetatiomMaskBeaconJob
                    {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                        Excluded = instanceData.Excluded.AsDeferredJobArray(),
                        Position = instanceData.Position.AsDeferredJobArray(),
                        VegetationMaskDensity = instanceData.VegetationMaskDensity.AsDeferredJobArray(),
                        VegetationMaskScale = instanceData.VegetationMaskScale.AsDeferredJobArray(),
#else
                        Excluded = instanceData.Excluded.ToDeferredJobArray(),
                        Position = instanceData.Position.ToDeferredJobArray(),
                        VegetationMaskDensity = instanceData.VegetationMaskDensity.ToDeferredJobArray(),
                        VegetationMaskScale = instanceData.VegetationMaskScale.ToDeferredJobArray(),               
#endif

                        

                        Denisty = vegetationTypeSettings.Density,
                        Scale = vegetationTypeSettings.Size,
                        FalloutCurveArray = FalloutCurveArray,
                        MaskPosition = Position,
                        Radius = Radius
                    };
                dependsOn = includeVegetatiomMaskBeaconJob.Schedule(instanceData.Excluded, 32, dependsOn);
            }

            return dependsOn;
        }

        public override bool HasVegetationTypeIndex(VegetationTypeIndex vegetationTypeIndex)
        {
            for (int i = 0; i <= VegetationTypeList.Count - 1; i++)
            {
                if (VegetationTypeList[i].Index == vegetationTypeIndex) return true;
            }

            return false;
        }

        private Bounds GetMaskBounds()
        {
            return new Bounds(Position, new Vector3(Radius * 2, Radius * 2, Radius * 2));
        }

        public override void Dispose()
        {
            base.Dispose();
            if (FalloutCurveArray.IsCreated) FalloutCurveArray.Dispose();
        }
    }
}