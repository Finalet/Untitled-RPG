using AwesomeTechnologies.Vegetation.Masks;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    public class VegetationItemInstanceInfo : MonoBehaviour
    {
        public string VegetationItemInstanceID;
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;
        public string VegetationItemID;
        public VegetationType VegetationType;

        public void MaskVegetationItem()
        {
            GameObject vegetationItemMaskObject = new GameObject { name = "VegetationItemMask - " + name };
            vegetationItemMaskObject.transform.position = Position;
            vegetationItemMaskObject.AddComponent<VegetationItemMask>().SetVegetationItemInstanceInfo(this);
        }
    }
}
