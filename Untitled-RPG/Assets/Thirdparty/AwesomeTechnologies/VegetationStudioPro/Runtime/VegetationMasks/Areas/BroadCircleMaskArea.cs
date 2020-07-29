using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.VegetationSystem;
using Unity.Collections;
using Unity.Jobs;

namespace AwesomeTechnologies
{
    public class BroadCircleMaskArea : CircleMaskArea
    {
        public bool MaskGrass = false;
        public bool MaskPlants = false;
        public bool MaskTrees = false;
        public bool MaskObjects = false;
        public bool MaskLargeObjects = false;

        public new void Init()
        {
            base.Init();
            RemoveGrass = MaskGrass;
            RemovePlants = MaskPlants;
            RemoveTrees = MaskTrees;
            RemoveObjects = MaskObjects;
            RemoveLargeObjects = MaskLargeObjects;
        }

        public override JobHandle SampleMask(VegetationInstanceData instanceData, VegetationType vegetationType,
            JobHandle dependsOn)
        {
            if (!ExcludeVegetationType(vegetationType)) return dependsOn;

            SampleVegetatiomMaskCircleJob sampleVegetatiomMaskCircleJob =
                new SampleVegetatiomMaskCircleJob
                {
                    MaskPosition = Position,
                    Radius = Radius,
                    
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                    Position = instanceData.Position.AsDeferredJobArray(),
                    Excluded = instanceData.Excluded.AsDeferredJobArray()
#else
                    Position = instanceData.Position.ToDeferredJobArray(),
                    Excluded = instanceData.Excluded.ToDeferredJobArray()               
#endif
                };
            dependsOn = sampleVegetatiomMaskCircleJob.Schedule(instanceData.Excluded,32, dependsOn);

            return dependsOn;
        }
    }
}
