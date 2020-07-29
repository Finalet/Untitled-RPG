using UnityEditor;

namespace AwesomeTechnologies.TouchReact
{
    [CustomEditor(typeof(TouchReactMesh))]
    public class TouchReactMeshEditor : TouchReactBaseEditor
    {
        public override void OnInspectorGUI()
        {
            HelpTopic = "home/vegetation-studio/components/touch-bend-system";
            base.OnInspectorGUI();
        }
    }
}
