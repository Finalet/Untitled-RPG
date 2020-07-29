using UnityEditor;


namespace AwesomeTechnologies
{
    [CustomEditor(typeof(VegetationMask))]
    public class VegetationMaskErrorEditor : VegetationStudioProBaseEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("VegetationMask component is a base class used by the other mask components. Add VegetationMaskArea or VegetationMaskLine components to use the masks.", MessageType.Error);
        }
    }
}
