namespace AwesomeTechnologies.Vegetation.PersistentStorage
{
    public class PersistentVegetationStorageTools
    {
        /// <summary>
        /// This function will get the name of the vegetationSourceID. Contact me at lennart@awesometech.no if you want me to register your system as a source. Use ID 200-250 for your games own SourceIDs 
        /// </summary>
        /// <param name="vegetationSourceID"></param>
        /// <returns></returns>
        public static string GetSourceName(byte vegetationSourceID)
        {
            switch (vegetationSourceID)
            {
                case 0:
                    return "Vegetation Studio - Baked vegetation";
                case 1:
                    return "Vegetation Studio - Manual edited";
                case 2:
                    return "Terrain tree importer";
                case 3:
                    return "Scene object importer";
                case 4:
                    return "Terrain detail importer";
                case 5:
                    return "Vegetation Studio - Painted";
                case 10:
                    return "Gaia";
                case 11:
                    return "GeNa";
                case 12:
                    return "Sentieri";
                case 13:
                    return "TC2 Node Painter";
                case 14:
                    return "TC2";
                case 15:
                    return "MapMagic";
                case 16:
                    return "Origami";
                case 17:
                    return "Landscape Builder";
                case 18:
                    return "Voxeland";
                case 19:
                    return "YAPP";
            }

            return "Source_" + vegetationSourceID;
        }
    }
}
