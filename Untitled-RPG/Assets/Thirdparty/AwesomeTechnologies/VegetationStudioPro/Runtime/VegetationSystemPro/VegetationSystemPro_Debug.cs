using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class VegetationSystemPro
    {

        public void RefreshAllPrefabs()
        {
            for (int i = 0; i <= VegetationPackageProList.Count - 1; i++)
            {
                for (int j = 0; j <= VegetationPackageProList[i].VegetationInfoList.Count - 1; j++)
                {
                    VegetationPackageProList[i]
                        .RefreshVegetationItemPrefab(VegetationPackageProList[i].VegetationInfoList[j]);
                }

#if UNITY_EDITOR
                EditorUtility.SetDirty(VegetationPackageProList[i]);
#endif
            }
        }

        // ReSharper disable once UnusedMember.Local
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(VegetationSystemBounds.center, VegetationSystemBounds.size);

            if (CurrentTabIndex == 0)
            {
                DrawSeaLevel();
            }

            if (CurrentTabIndex == 8)
            {
                DrawTextureMaskAreas();
            }
        }

        void DrawSeaLevel()
        {
            Gizmos.color = new Color(0,0,0.8f,0.4f);
            float minZ = VegetationSystemBounds.center.y - VegetationSystemBounds.extents.y;

            Gizmos.DrawCube(new Vector3(VegetationSystemBounds.center.x, minZ + SeaLevel, VegetationSystemBounds.center.z), new Vector3(VegetationSystemBounds.size.x,0,VegetationSystemBounds.size.z));
            Gizmos.DrawWireCube(VegetationSystemBounds.center, VegetationSystemBounds.size);
        }



        void DrawTextureMaskAreas()
        {
            if (DebugTextureMask == null) return;

            Vector3 center = new Vector3(DebugTextureMask.TextureRect.center.x, VegetationSystemBounds.center.y,DebugTextureMask.TextureRect.center.y);
            Vector3 size = new Vector3(DebugTextureMask.TextureRect.width,VegetationSystemBounds.size.y,DebugTextureMask.TextureRect.height);
            Gizmos.color = new Color(0, 0.8f, 0.8f, 0.4f);
            Gizmos.DrawCube(center, size); 
        }

 

        // ReSharper disable once UnusedMember.Local
        void OnDrawGizmos()
        {
            if (!enabled) return;

            //DrawBillboardGizmos();
            //for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
            //{
            //    Gizmos.color = Color.yellow;
            //    Vector3 position = VegetationStudioCameraList[i].SelectedCamera.transform.position;
            //    float distance = VegetationSettings.GetVegetationDistance();
            //    Gizmos.DrawWireSphere(position, distance);
            //}

        

            if (ShowVegetationCells)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i <= VegetationCellList.Count - 1; i++)
                {                  
                    if (VegetationCellList[i].Enabled)
                    {
                        Gizmos.DrawWireCube(VegetationCellList[i].VegetationCellBounds.center,
                            VegetationCellList[i].VegetationCellBounds.size);

                        if (float.IsNegativeInfinity(VegetationCellList[i].VegetationCellBounds.size.y))
                        {
                            
                        }
                    }
                }
            }

            if (ShowBiomeCells)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i <= VegetationCellList.Count - 1; i++)
                {
                    if (VegetationCellList[i].Enabled && VegetationCellList[i].BiomeMaskList != null && VegetationCellList[i].BiomeMaskList.Count > 0)
                    {
                            Gizmos.DrawWireCube(VegetationCellList[i].VegetationCellBounds.center,
                                VegetationCellList[i].VegetationCellBounds.size);
                    }
                }
            }

            if (ShowVegetationMaskCells)
            {
                Gizmos.color = Color.magenta;
                for (int i = 0; i <= VegetationCellList.Count - 1; i++)
                {
                    if (VegetationCellList[i].Enabled && VegetationCellList[i].VegetationMaskList != null && VegetationCellList[i].VegetationMaskList.Count > 0)
                    {
                        Gizmos.DrawWireCube(VegetationCellList[i].VegetationCellBounds.center,
                            VegetationCellList[i].VegetationCellBounds.size);
                    }
                }
            }

            if (ShowPotentialVisibleCells)
            {
                for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
                {
                    VegetationStudioCameraList[i].DrawPotentialCellGizmos();
                }
            }

            if (ShowVisibleBillboardCells)
            {
                for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
                {
                    VegetationStudioCameraList[i].DrawVisibleBillboardCellGizmos();
                }
            }

            if (ShowVisibleCells)
            {
                for (int i = 0; i <= VegetationStudioCameraList.Count - 1; i++)
                {
                    VegetationStudioCameraList[i].DrawVisibleCellGizmos();
                }
            }

            if (ShowBillboardCells)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i <= BillboardCellList.Count - 1; i++)
                {

                        Gizmos.DrawWireCube(BillboardCellList[i].BilllboardCellBounds.center,
                            BillboardCellList[i].BilllboardCellBounds.size);
                }
            }
        }
    }
}
