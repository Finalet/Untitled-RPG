using AwesomeTechnologies.VegetationSystem;

namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    public interface IVegetationImporter
    {
        string ImporterName { get;}
        PersistentVegetationStoragePackage PersistentVegetationStoragePackage { get; set; }
        VegetationPackagePro VegetationPackagePro { get; set; }
        PersistentVegetationStorage PersistentVegetationStorage { get; set; }
        void OnGUI();
    }
}
