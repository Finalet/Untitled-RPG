using AwesomeTechnologies.VegetationSystem;

namespace AwesomeTechnologies
{
    [System.Serializable]
    public class VegetationTypeSettings
    {
        public VegetationTypeIndex Index = VegetationTypeIndex.VegetationType1;
        public float Density = 1f;
        public float Size = 1f;

        public VegetationTypeSettings()
        {

        }

        public VegetationTypeSettings(VegetationTypeSettings orgVegetationTypeSettings)
        {
            Index = orgVegetationTypeSettings.Index;
            Density = orgVegetationTypeSettings.Density;
            Size = orgVegetationTypeSettings.Size;
        }
    }
}