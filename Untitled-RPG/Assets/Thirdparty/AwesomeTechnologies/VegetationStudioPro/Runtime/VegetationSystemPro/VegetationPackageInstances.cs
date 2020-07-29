using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public class ComputeBufferInfo
    {
        public ComputeBuffer ComputeBuffer;
        public bool Created;
    }

    public struct MatrixInstance
    {
        public Matrix4x4 Matrix;
        public float DistanceFalloff;
    }

    public class IndirectInstanceInfo
    {
        public NativeArray<InstancedIndirectInstance> InstancedIndirectInstanceList;
        public bool Created;
    }

    public struct InstancedIndirectInstance
    {
        // ReSharper disable once NotAccessedField.Global
        public Matrix4x4 Matrix;
        // ReSharper disable once NotAccessedField.Global
        public Vector4 ControlData;
    }

    [BurstCompile]
    public struct CreateInstancedIndirectInstancesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<MatrixInstance> InstanceList;
        public NativeArray<InstancedIndirectInstance> IndirectInstanceList;

        public void Execute(int index)
        {
            InstancedIndirectInstance instancedIndirectInstance =
                new InstancedIndirectInstance
                {
                    ControlData = new Vector4(InstanceList[index].DistanceFalloff, 0, 0),
                    Matrix = InstanceList[index].Matrix
                };
            IndirectInstanceList[index] = instancedIndirectInstance;
        }
    }

    public class VegetationPackageInstances
    {
        public readonly List<NativeList<MatrixInstance>> VegetationItemMatrixList = new List<NativeList<MatrixInstance>>();       
        public NativeList<int> LoadStateList;

        public readonly List<ComputeBufferInfo> VegetationItemComputeBufferList = new List<ComputeBufferInfo>();
        public readonly List<IndirectInstanceInfo> VegetationItemInstancedIndirectInstanceList = new List<IndirectInstanceInfo>();

        public JobHandle LoadVegetationJobHandle;
        public VegetationPackageInstances(int vegetationItemCount)
        {
            VegetationItemMatrixList.Capacity = vegetationItemCount;
            LoadStateList = new NativeList<int>(vegetationItemCount,Allocator.Persistent);
            for (int i = 0; i <= vegetationItemCount - 1; i++)
            {
                VegetationItemMatrixList.Add(new NativeList<MatrixInstance>(Allocator.Persistent));
                
                VegetationItemInstancedIndirectInstanceList.Add(new IndirectInstanceInfo());
                VegetationItemComputeBufferList.Add(new ComputeBufferInfo());
                LoadStateList.Add(0);
            }          
        }

        public void ClearInstanceMemory()
        {
            for (int i = 0; i <= VegetationItemMatrixList.Count - 1; i++)
            {
                NativeList<MatrixInstance> vegetationItemMatrixList = VegetationItemMatrixList[i];
                if (vegetationItemMatrixList.IsCreated)
                {
                    vegetationItemMatrixList.Clear();
                    vegetationItemMatrixList.Capacity = 0;
                }
            }
        }
               
        public void Dispose()
        {           
            for (int i = 0; i <= VegetationItemMatrixList.Count - 1; i++)
            {
                VegetationItemMatrixList[i].Dispose();                
            }

            VegetationItemMatrixList.Clear();
                       
            for (int i = 0; i <= VegetationItemInstancedIndirectInstanceList.Count - 1; i++)
            {
                if (VegetationItemInstancedIndirectInstanceList[i].Created)
                {
                    VegetationItemInstancedIndirectInstanceList[i].InstancedIndirectInstanceList.Dispose();
                }               
            }

            for (int i = 0; i <= VegetationItemComputeBufferList.Count - 1; i++)
            {
                ComputeBufferInfo computeBufferInfo = VegetationItemComputeBufferList[i];
                if (computeBufferInfo.Created)
                {
                    computeBufferInfo.ComputeBuffer.Dispose();
                    computeBufferInfo.Created = false;
                }
            }
            if (LoadStateList.IsCreated) LoadStateList.Dispose();
        }
    }
}

