using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Utilities
{
    /// <summary> Enable Disable the LookAt Logic by Priority </summary>
    public interface ILookAtActivation
    {
        void EnableByPriority(int layer);
        void DisableByPriority(int layer);
        void ResetByPriority(int layer);
    }

    public class LookAtBehaviour : StateMachineBehaviour
    {
        public enum LookAtState { DoNothing, Enable, Disable , Reset}

        private ILookAtActivation lookat;
        public LookAtState OnEnter = LookAtState.Enable;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (lookat == null) lookat = animator.FindInterface<ILookAtActivation>();

            if (lookat != null)
            {
                switch (OnEnter)
                {
                    case LookAtState.Reset: lookat.ResetByPriority(layerIndex + 1); break;
                    case LookAtState.Enable: lookat.EnableByPriority(layerIndex + 1); break;
                    case LookAtState.Disable: lookat.DisableByPriority(layerIndex + 1); break;
                    default: break;
                }
            }
        } 
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(LookAtBehaviour))]
    public class LookAtBehaviourED : Editor
    {
        SerializedProperty OnEnter;
        void OnEnable()
        {
            OnEnter = serializedObject.FindProperty("OnEnter");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Enable/Disable the Look At logic by layer priority");
            EditorGUILayout.PropertyField(OnEnter, new GUIContent("Status"));
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}