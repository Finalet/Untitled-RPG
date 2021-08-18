using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    public class IsKinematicB : StateMachineBehaviour
    {
        public enum OnEnterOnExit { OnEnter, OnExit, OnEnterOnExit}
        public OnEnterOnExit SetKinematic = OnEnterOnExit.OnEnterOnExit;

        [Tooltip("Changes the Kinematic property of the RigidBody On Enter/OnExit")]
        [Hide("onenterexit",true,true)]
        public bool isKinematic = true;
        CollisionDetectionMode current;

        Rigidbody rb;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            rb = animator.GetComponent<Rigidbody>();

            if (SetKinematic == OnEnterOnExit.OnEnter)
            {
                if (isKinematic == true)
                {
                    current = rb.collisionDetectionMode;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }

                rb.isKinematic = isKinematic;
            }
            else if (SetKinematic == OnEnterOnExit.OnEnterOnExit)
            {
                current = rb.collisionDetectionMode;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rb.isKinematic = true;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (SetKinematic == OnEnterOnExit.OnExit)
            {
                if (isKinematic == true)
                {
                    current = rb.collisionDetectionMode;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
                else
                {
                    rb.collisionDetectionMode = current;
                }

                rb.isKinematic = isKinematic;
            }
            else if (SetKinematic == OnEnterOnExit.OnEnterOnExit)
            {
                rb.isKinematic = false;
                rb.collisionDetectionMode = current;
            }
        }

        [HideInInspector] public bool onenterexit;
        private void OnValidate()
        {
            onenterexit = SetKinematic == OnEnterOnExit.OnEnterOnExit;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(IsKinematicB))] 
    public class IsKinematicBED : Editor
    {
        SerializedProperty SetKinematic, isKinematic;
        void OnEnable()
        {

            SetKinematic = serializedObject.FindProperty("SetKinematic");
            isKinematic = serializedObject.FindProperty("isKinematic");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginHorizontal(); 
            EditorGUIUtility.labelWidth = 50;
            EditorGUILayout.PropertyField(SetKinematic, new GUIContent("Set: "));

            if (SetKinematic.intValue != 2)
            {
                EditorGUIUtility.labelWidth = 70; 
                EditorGUILayout.PropertyField(isKinematic, new GUIContent("Kinematic:"), GUILayout.Width(100));
            } 
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}