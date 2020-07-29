using System.Collections.Generic;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using UnityEngine;

namespace AwesomeTechnologies.BillboardSystem
{
    public class BillboardInstance
    {
        public bool Loaded;
        public int InstanceCount;
        public Mesh Mesh;
        public Vector3 Position;
        public NativeList<MatrixInstance> InstanceList;
        public NativeList<Vector3> VerticeList;
        public NativeList<int> IndexList;
        public NativeList<Vector2> UvList;
        public NativeList<Vector2> Uv2List;
        public NativeList<Vector2> Uv3List;
        public NativeList<Vector3> NormalList;

        public BillboardInstance()
        {
            InstanceList = new NativeList<MatrixInstance>(0,Allocator.Persistent);

            VerticeList = new NativeList<Vector3>(0, Allocator.Persistent);
            IndexList = new NativeList<int>(0, Allocator.Persistent);
            UvList = new NativeList<Vector2>(0, Allocator.Persistent);
            Uv2List = new NativeList<Vector2>(0, Allocator.Persistent);
            Uv3List = new NativeList<Vector2>(0, Allocator.Persistent);
            NormalList = new NativeList<Vector3>(0, Allocator.Persistent);
        }

        public void ClearCache()
        {
            if (!Loaded) return;

            InstanceList.Clear();
            IndexList.Clear();
            VerticeList.Clear();
            NormalList.Clear();
            UvList.Clear();
            Uv2List.Clear();
            Uv3List.Clear();
            Object.DestroyImmediate(Mesh);
            Loaded = false;
        }

        public void Dispose()
        {
            if (InstanceList.IsCreated) InstanceList.Dispose();
            if (VerticeList.IsCreated) VerticeList.Dispose();
            if (IndexList.IsCreated) IndexList.Dispose();
            if (UvList.IsCreated) UvList.Dispose();
            if (Uv2List.IsCreated) Uv2List.Dispose();
            if (Uv3List.IsCreated) Uv3List.Dispose();
            if (NormalList.IsCreated) NormalList.Dispose();
        }
    }

    public class VegetationPackageBillboardInstances
    {
        public List<BillboardInstance> BillboardInstanceList = new List<BillboardInstance>();

        public VegetationPackageBillboardInstances(int vegetationItemCount)
        {
            for (int i = 0; i <= vegetationItemCount - 1; i++)
            {
                BillboardInstance billboardInstance = new BillboardInstance();
                BillboardInstanceList.Add(billboardInstance);
            }
        }

        public void ClearCache()
        {
            for (int i = 0; i <= BillboardInstanceList.Count - 1; i++)
            {
                BillboardInstanceList[i].ClearCache();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i <= BillboardInstanceList.Count - 1; i++)
            {
                BillboardInstanceList[i].Dispose();
            }
            BillboardInstanceList.Clear();
        }
    }
}

