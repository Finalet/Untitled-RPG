using System;
using AwesomeTechnologies.MeshTerrains;
using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AwesomeTechnologies.Utility
{
    public enum DropZoneType
    {
        GrassPrefab,
        PlantPrefab,
        TreePrefab,
        ObjectPrefab,
        LargeObjectPrefab,
        GrassTexture,
        PlantTexture,
        MeshRenderer,
        Terrain
    }

    public class DropZoneTools
    {
        public static Type GetDropZoneSystemType(DropZoneType dropZoneType)
        {
            switch (dropZoneType)
            {
                case DropZoneType.GrassPrefab:
                    return typeof(GameObject);
                case DropZoneType.PlantPrefab:
                    return typeof(GameObject);
                case DropZoneType.TreePrefab:
                    return typeof(GameObject);
                case DropZoneType.ObjectPrefab:
                    return typeof(GameObject);
                case DropZoneType.LargeObjectPrefab:
                    return typeof(GameObject);
                case DropZoneType.GrassTexture:
                    return typeof(Texture2D);
                case DropZoneType.PlantTexture:
                    return typeof(Texture2D);
                case DropZoneType.MeshRenderer:
                    return typeof(MeshRenderer);
                case DropZoneType.Terrain:
                    return typeof(Terrain);
                default:
                    return typeof(GameObject);
            }
        }

        public static void DrawVegetationItemDropZone(DropZoneType dropZoneType, VegetationPackagePro vegetationPackage, ref Boolean addedItem)
        {
            Event evt = Event.current;

            Type selectedType = GetDropZoneSystemType(dropZoneType);
            Texture2D iconTexture = GetDropZoneIconTexture(dropZoneType);

            Rect dropArea = GUILayoutUtility.GetRect(iconTexture.width, iconTexture.height, GUILayout.ExpandWidth(false));
            GUILayoutUtility.GetRect(5, iconTexture.height, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(dropArea, iconTexture);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (!dropArea.Contains(evt.mousePosition))
                    {
                        return;
                    }

                    bool hasType = HasDropType(DragAndDrop.objectReferences, selectedType);
                    if (!hasType) return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject.GetType() == selectedType)
                            {
                                switch (dropZoneType)
                                {
                                    case DropZoneType.GrassTexture:
                                        vegetationPackage.AddVegetationItem(draggedObject as Texture2D, VegetationType.Grass);
                                        break;
                                    case DropZoneType.PlantTexture:
                                        vegetationPackage.AddVegetationItem(draggedObject as Texture2D, VegetationType.Plant);
                                        break;
                                    case DropZoneType.GrassPrefab:
                                        vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.Grass);
                                        break;
                                    case DropZoneType.PlantPrefab:
                                        vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.Plant);
                                        break;
                                    case DropZoneType.TreePrefab:
                                        vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.Tree);
                                        break;
                                    case DropZoneType.ObjectPrefab:
                                        vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.Objects);
                                        break;
                                    case DropZoneType.LargeObjectPrefab:
                                        vegetationPackage.AddVegetationItem(draggedObject as GameObject, VegetationType.LargeObjects);
                                        break;
                                }
                                addedItem = true;
                            }
                        }
                    }
                    break;
            }

        }
        public static void DrawMeshTerrainDropZone(DropZoneType dropZoneType, MeshTerrain meshTerrain, ref Boolean addedItem)
        {
            Event evt = Event.current;
            Texture2D iconTexture = GetDropZoneIconTexture(dropZoneType);

            Rect dropArea = GUILayoutUtility.GetRect(iconTexture.width, iconTexture.height, GUILayout.ExpandWidth(false));
            GUILayoutUtility.GetRect(5, iconTexture.height, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(dropArea, iconTexture);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (!dropArea.Contains(evt.mousePosition))
                    {
                        return;
                    }

                    bool hasType = HasDropComponentType(DragAndDrop.objectReferences, dropZoneType);
                    if (!hasType) return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            GameObject droppedgo;
                            switch (dropZoneType)
                            {
                                case DropZoneType.Terrain:
                                    droppedgo = draggedObject as GameObject;
                                    if (!droppedgo) break;
                                    addedItem = true;
                                    meshTerrain.AddTerrain(droppedgo, TerrainSourceID.TerrainSourceID1);
                                    break;
                                case DropZoneType.MeshRenderer:
                                    droppedgo = draggedObject as GameObject;
                                    if (!droppedgo) break;
                                    addedItem = true;
                                    meshTerrain.AddMeshRenderer(droppedgo, TerrainSourceID.TerrainSourceID1);
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        static Texture2D GetDropZoneIconTexture(DropZoneType dropZoneType)
        {
            if (EditorGUIUtility.isProSkin)
            {
                switch (dropZoneType)
                {                        
                    case DropZoneType.Terrain:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkTerrainDropZoneNoBorder", typeof(Texture2D));
                    case DropZoneType.MeshRenderer:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkMeshDropZoneNoBorder", typeof(Texture2D));
                    case DropZoneType.GrassPrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkGrassPrefabNoBorder", typeof(Texture2D));
                    case DropZoneType.PlantPrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkPlantPrefabNoBorder", typeof(Texture2D));
                    case DropZoneType.TreePrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkTreePrefabNoBorder", typeof(Texture2D));
                    case DropZoneType.ObjectPrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkObjectPrefabNoBorder", typeof(Texture2D));
                    case DropZoneType.LargeObjectPrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkLargeObjectPrefabNoBorder",
                            typeof(Texture2D));
                    case DropZoneType.GrassTexture:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkGrassTextureNoBorder", typeof(Texture2D));
                    case DropZoneType.PlantTexture:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkPlantTextureNoBorder", typeof(Texture2D));
                    default:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkPlantPrefabNoBorder", typeof(Texture2D));
                }
            }
            else
            {
                switch (dropZoneType)
                {
                    case DropZoneType.Terrain:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkTerrainDropZoneNoBorder", typeof(Texture2D));
                    case DropZoneType.MeshRenderer:
                        return (Texture2D)Resources.Load("DropZoneIcons/DarkMeshDropZoneNoBorder", typeof(Texture2D));
                    case DropZoneType.GrassPrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/LightGrassPrefabNoBorder", typeof(Texture2D));
                    case DropZoneType.PlantPrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/LightPlantPrefabNoBorder", typeof(Texture2D));
                    case DropZoneType.TreePrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/LightTreePrefabNoBorder", typeof(Texture2D));
                    case DropZoneType.ObjectPrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/LightObjectPrefabNoBorder", typeof(Texture2D));
                    case DropZoneType.LargeObjectPrefab:
                        return (Texture2D)Resources.Load("DropZoneIcons/LightLargeObjectPrefabNoBorder",
                            typeof(Texture2D));
                    case DropZoneType.GrassTexture:
                        return (Texture2D)Resources.Load("DropZoneIcons/LightGrassTextureNoBorder", typeof(Texture2D));
                    case DropZoneType.PlantTexture:
                        return (Texture2D)Resources.Load("DropZoneIcons/LightPlantTextureNoBorder", typeof(Texture2D));
                    default:
                        return (Texture2D)Resources.Load("DropZoneIcons/LightPlantPrefabNoBorder", typeof(Texture2D));
                }
            }
        }

        private static bool HasDropType(Object[] dragObjects, System.Type type)
        {
            foreach (Object draggedObject in dragObjects)
            {
                if (draggedObject.GetType() != type) continue;
                return true;
            }

            return false;
        }

        private static bool HasDropComponentType(Object[] dragObjects, DropZoneType dropZoneType)
        {
            foreach (Object draggedObject in dragObjects)
            {
                GameObject draggedGo = draggedObject as GameObject;
                if (!draggedGo) continue;

                if (dropZoneType == DropZoneType.MeshRenderer)
                {
                    if (draggedGo.GetComponentInChildren<MeshRenderer>() != null) return true;
                }

                if (dropZoneType == DropZoneType.Terrain)
                {
                    if (draggedGo.GetComponentInChildren<Terrain>() != null) return true;
                }
            }
            return false;
        }
    }
}

