using AwesomeTechnologies.VegetationSystem;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.Masks
{
    [CustomEditor(typeof(VegetationItemMask))]
    public class VegetationItemMaskEditor : VegetationStudioProBaseEditor
    {
        public override void OnInspectorGUI()
        {
            //HelpTopic = "home/vegetation-studio/components/vegetation-masks/vegetation-mask-area";
            base.OnInspectorGUI();

            VegetationItemMask vegetationItemMask = (VegetationItemMask) target;

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Vegetation Item Instance", LabelStyle);
            EditorGUI.BeginChangeCheck();
            vegetationItemMask.Position = EditorGUILayout.Vector3Field("Position", vegetationItemMask.Position);
            vegetationItemMask.VegetationType = (VegetationType)EditorGUILayout.EnumPopup("",vegetationItemMask.VegetationType);
            if (EditorGUI.EndChangeCheck())
            {
                vegetationItemMask.SetDirty();
            }
            EditorGUILayout.HelpBox("Set the position and VegetationType of the object to mask.", MessageType.Info);
            GUILayout.EndVertical();
        }
    }
}
