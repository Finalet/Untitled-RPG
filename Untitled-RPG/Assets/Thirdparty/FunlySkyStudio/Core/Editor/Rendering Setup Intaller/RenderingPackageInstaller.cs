using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.VersionControl;

namespace Funly.SkyStudio {

    /// <summary>
    /// This editor script detects your Unity rendering pipeline and installs the necessary demo and support packages
    /// that Sky Studio needs.
    /// </summary>
    [InitializeOnLoad]
    public class RenderingPackageInstaller
    {
        public const string kRenderingSupportInstalledRoot = "Assets/FunlySkyStudio/Rendering Support";
        public const string kDisableContentInstallEditorKey = "SkyStudioDisableContentInstall";
        
        private const string kInstalledContentVersionFile = "InstalledContentVersion.asset";
        private const string kRenderingSupportPackagesDir = "SkyStudioRenderingSupportPackages";
        private const string kSkyStudioDisableImportFile = "Assets/SkyStudioDisableImport.txt";
        
        static RenderingPackageInstaller() {
            Install();
        }

        public static void Install() {
            RenderingConfig.DetectedRenderingConfig renderingConfig = RenderingConfig.Detect();

            string package = FindPackagePathForRenderingConfig(renderingConfig);

            if (package == null)
            {
                Debug.LogError("Failed to locate a package that supports your rendering configuration.");
                return;
            }
            
            if (!ShouldInstallContent(package))
            {
                return;
            }
            
            // Check for the package install lock.
            if (IsImportLocked())
            {
                Debug.Log("Ignoring package install, the lock is enabled.");
                return;
            }

            ClearPreviouslyInstalledContent();
            InstallPackage(package);
        }
        
        public static void ClearInstalledVersionFile()
        {
            AssetDatabase.DeleteAsset(InstalledVersionFileAssetPath());
        }
        
        // Call this after the content has been detected by the post processing script to finish the install.
        public static void FinishContentInstall()
        {
            if (IsImportLocked())
            {
                return;
                
                
                
            }
            
            string[] moveFolders = new [] {"Demos", "Tutorials"};
            bool didMigrate = false;
            
            // Migrate folders if they exist in the installed content.
            foreach (string folder in moveFolders)
            {
                if (AssetDatabase.IsValidFolder($"{kRenderingSupportInstalledRoot}/{folder}"))
                {
                    MoveRenderingSupportFolderIntoRoot(folder);
                    didMigrate = true;
                }
            }

            if (!didMigrate)
            {
                return;
            }
            
            Debug.Log($"Sky Studio, successfully imported rendering support package.");
        }
        
        /// <summary>
        ///  Delete any folders we created as part of a previous content install.
        /// </summary>
        public static void ClearPreviouslyInstalledContent()
        {
            SkyEditorUtility.DeleteAssetInSkyStudio("Rendering Support");
            SkyEditorUtility.DeleteAssetInSkyStudio("Demos");
            SkyEditorUtility.DeleteAssetInSkyStudio("Tutorials");
        }

        public static void SetImportLock(bool isLocked)
        {
            if (isLocked)
            {
                AssetDatabase.CreateAsset(new TextAsset(), kSkyStudioDisableImportFile);
            }
            else
            {
                AssetDatabase.DeleteAsset(kSkyStudioDisableImportFile);
            }
            
            AssetDatabase.SaveAssets();
        }

        public static bool IsImportLocked()
        {
            TextAsset lockFile = AssetDatabase.LoadAssetAtPath<TextAsset>(kSkyStudioDisableImportFile);
            return lockFile != null ? true : false;
        }
        
        // Find the best package to install based on the rendering config's name. Returns only package name (not path).
        static string FindPackagePathForRenderingConfig(RenderingConfig.DetectedRenderingConfig config)
        {
            string packagesDir = FindAndGetAssetPath($"l:{kRenderingSupportPackagesDir}");
            string[] packages = Directory.GetFiles(packagesDir);
            string configName = config.ToString();
 
            // Search for a package with this rendering platform config name.
            foreach (string package in packages)
            {
                if (package.Contains(".meta"))
                {
                    continue;
                }
                
                if (package.Contains(configName))
                {
                    return Path.GetFileName(package);
                }
            }

            return null;
        }

        static string FindAndGetAssetPath(string search)
        {
            string[] assets = AssetDatabase.FindAssets(search);

            if (assets == null || assets.Length == 0)
            {
                return null;
            }

            return AssetDatabase.GUIDToAssetPath(assets[0]);
        }

        static void InstallPackage(string packageName) {
            string rootDir = SkyEditorUtility.SkyStudioRootDirectory();
            
            string packagePath = $"{rootDir}/Core/Internal/Rendering Support Packages/{packageName}";
            
            UpdateInstalledVersionFile(packageName);
            AssetDatabase.ImportPackage(packagePath, false);
        }

        static Boolean ShouldInstallContent(string packageName)
        {
            RenderingContentVersion current = GetInstalledVersionFile();

            if (current == null || current.version == null)
            {
                return true;
            }
            
            // If version is the same, skip the install.
            if (current.version.ToUpper().Equals(packageName.ToUpper()))
            {
                return false;
            }

            return true;
        }
        
        // The asset path to the installed version file.
        static string InstalledVersionFileAssetPath()
        {
            string rootDir = SkyEditorUtility.SkyStudioRootDirectory();
            return $"{rootDir}/{kInstalledContentVersionFile}";
        }
        
        // Return the installed version object.
        static RenderingContentVersion GetInstalledVersionFile()
        {
            return AssetDatabase.LoadAssetAtPath<RenderingContentVersion>(InstalledVersionFileAssetPath());
        }
        
        // The installed package name has a version in it, so we write then into the root directory.
        static void UpdateInstalledVersionFile(string installedPackageName)
        {
            RenderingContentVersion version = ScriptableObject.CreateInstance<RenderingContentVersion>();
            version.version = installedPackageName;

            AssetDatabase.CreateAsset(version, InstalledVersionFileAssetPath());
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Move a folder out of a the rendering support directory and into the root of Sky Studio.
        /// </summary>
        /// <param name="folderName"></param>
        static void MoveRenderingSupportFolderIntoRoot(string folderName)
        {
            AssetDatabase.MoveAsset(
                $"{kRenderingSupportInstalledRoot}/{folderName}", 
                $"{SkyEditorUtility.SkyStudioRootDirectory()}/{folderName}");
        }
    }

}