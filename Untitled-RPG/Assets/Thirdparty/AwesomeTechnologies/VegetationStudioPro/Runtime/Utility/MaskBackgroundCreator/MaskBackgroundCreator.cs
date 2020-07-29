using AwesomeTechnologies.Utility.Quadtree;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public enum BackgroundMaskQuality
    {
        Low1024 = 0,
        Normal2048 = 1,
        High4096 = 2
    }

    [HelpURL("http://www.awesometech.no/index.php/background-mask-creator")]
    public class MaskBackgroundCreator : MonoBehaviour
    {
        public BackgroundMaskQuality BackgroundMaskQuality = BackgroundMaskQuality.Normal2048;
        public Rect AreaRect;


        public int GetBackgroundMaskQualityPixelResolution(BackgroundMaskQuality backgroundMaskQuality)
        {
            switch (backgroundMaskQuality)
            {
                case BackgroundMaskQuality.Low1024:
                    return 1024;
                case BackgroundMaskQuality.Normal2048:
                    return 2048;
                case BackgroundMaskQuality.High4096:
                    return 4096;
                default:
                    return 1024;
            }
        }

        void Reset()
        {
            VegetationSystemPro vegetationSystemPro = this.GetComponent<VegetationSystemPro>();
            if (vegetationSystemPro)
            {
                AreaRect = RectExtension.CreateRectFromBounds(vegetationSystemPro.VegetationSystemBounds);
            }
        }        
    }
}
