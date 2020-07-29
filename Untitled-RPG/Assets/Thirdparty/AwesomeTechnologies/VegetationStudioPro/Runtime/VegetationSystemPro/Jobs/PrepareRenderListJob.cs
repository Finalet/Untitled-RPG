using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [BurstCompile(CompileSynchronously = true)]
    public struct MergeCellInstancesJob : IJob
    {
        public NativeList<MatrixInstance> OutputNativeList;
        [ReadOnly]
        public NativeList<MatrixInstance> InputNativeList;

        public void Execute()
        {
            for (int l = 0; l <= InputNativeList.Length - 1; l++)
            {
                OutputNativeList.Add(InputNativeList[l]);
            }

            //TODO replace when NativeList is fixed.

            //if (InputNativeList.Length > 0)
            //{
            //    OutputNativeList.AddRange(InputNativeList);
            //}
        }
    }    
}
