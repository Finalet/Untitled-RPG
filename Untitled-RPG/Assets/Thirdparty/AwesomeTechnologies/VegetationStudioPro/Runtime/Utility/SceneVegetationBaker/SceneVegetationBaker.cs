using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Utility.Baking
{
    public class SceneVegetationBaker : MonoBehaviour
    {
        public VegetationSystemPro VegetationSystemPro;
        public bool ExportStatic = true;
      
        public int VegetationPackageIndex;
        
        private void Reset()
        {
            VegetationSystemPro = gameObject.GetComponent<VegetationSystemPro>();
        }
    }
} 
