using UnityEditor;
using UnityEngine;
namespace AwesomeTechnologies.TouchReact
{
    [CustomEditor(typeof(TouchReactCollider))]
    public class TouchReactColliderEditor : TouchReactBaseEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            TouchReactCollider touchReactCollider = (TouchReactCollider)target;
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Colliders", LabelStyle);
            touchReactCollider.AddChildColliders = EditorGUILayout.Toggle("Add child colliders", touchReactCollider.AddChildColliders);
            EditorGUILayout.HelpBox("Add all colliders from child GameObjects. ", MessageType.Info);
            touchReactCollider.ColliderScale = EditorGUILayout.Slider("Collider scale", touchReactCollider.ColliderScale,0.1f,5f);
            EditorGUILayout.HelpBox("Collider scale affects the touch react area of the collider. Can be usefull to increase on character colliders and large grass patches. Mesh colliders will not scale.", MessageType.Info);

            if (GUILayout.Button("Refresh colliders"))
            {
                touchReactCollider.RefreshColliders();
            }
            EditorGUILayout.HelpBox("Refresh colliders. Update colliders from Gameobject.", MessageType.Info);

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                touchReactCollider.RefreshColliders();
                EditorUtility.SetDirty(touchReactCollider);
            }
        }
    }
}
