using UnityEngine;
using UnityEditor;

namespace  Funly.SkyStudio
{
    /// <summary>
    ///  Watch for the Sky Studio rendering package content to finish importing, then finish setup.
    /// </summary>
    class RenderingContentPostProcessor: AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            bool didFindAssetContent = false;
        
            foreach (string str in importedAssets)
            {
                if (str.Contains(RenderingPackageInstaller.kRenderingSupportInstalledRoot))
                {
                    didFindAssetContent = true;
                    break;
                }
            }

            if (!didFindAssetContent)
            {
                return;
            }

            RenderingPackageInstaller.FinishContentInstall();
        }
    }
}
