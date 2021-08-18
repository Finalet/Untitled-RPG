using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    public class RigidConstraintsB : StateMachineBehaviour
    {
        public bool PosX, PosY, PosZ, RotX = true, RotY = true, RotZ = true;
        public bool OnEnter = true, OnExit;
        //  public bool OnEnterKinematic, OnExitKinematic;
        protected int Amount = 0;
        Rigidbody rb;

        bool ExitTime;

        public float OnEnterDrag = 0;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Amount = 0;
            rb = animator.GetComponent<Rigidbody>();

            if (PosX) Amount += 2;
            if (PosY) Amount += 4;
            if (PosZ) Amount += 8;
            if (RotX) Amount += 16;
            if (RotY) Amount += 32;
            if (RotZ) Amount += 64;

            if (OnEnter && rb) { rb.constraints = (RigidbodyConstraints)Amount; }

            ExitTime = false;

            rb.drag = OnEnterDrag;
            //  rb.isKinematic = OnEnterKinematic;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!ExitTime && OnExit && stateInfo.normalizedTime > 1)
            {
                rb.constraints = (RigidbodyConstraints)Amount;
                ExitTime = true;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (OnExit) rb.constraints = (RigidbodyConstraints)Amount;

            //  rb.isKinematic = OnExitKinematic;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RigidConstraintsB))]
    public class RigidConstraintsBEd : Editor
    {
        SerializedProperty OnEnter, OnExit, PosX, PosY, PosZ, RotX, RotY, RotZ;

        private void OnEnable()
        {
            OnEnter = serializedObject.FindProperty("OnEnter");
            OnExit = serializedObject.FindProperty("OnExit");

            PosX = serializedObject.FindProperty("PosX");
            PosY = serializedObject.FindProperty("PosY");
            PosZ = serializedObject.FindProperty("PosZ");

            RotX = serializedObject.FindProperty("RotX");
            RotY = serializedObject.FindProperty("RotY");
            RotZ = serializedObject.FindProperty("RotZ");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Modify the Rigidbody Constraints attached to this Animator");

            EditorGUILayout.BeginVertical(MTools.StyleGray);
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnterDrag"));
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    OnEnter.boolValue = EditorGUILayout.Toggle("On Enter", OnEnter.boolValue, EditorStyles.radioButton);
                    OnExit.boolValue = !OnEnter.boolValue;

                    OnExit.boolValue = EditorGUILayout.Toggle("On Exit", OnExit.boolValue, EditorStyles.radioButton);
                    OnEnter.boolValue = !OnExit.boolValue;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Constraints  ", EditorStyles.boldLabel, GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("X", EditorStyles.boldLabel, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("Y", EditorStyles.boldLabel, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("     Z", EditorStyles.boldLabel, GUILayout.MaxWidth(35));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Position ", GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        PosX.boolValue = EditorGUILayout.Toggle(PosX.boolValue, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        PosY.boolValue = EditorGUILayout.Toggle(PosY.boolValue, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        PosZ.boolValue = EditorGUILayout.Toggle(PosZ.boolValue, GUILayout.MaxWidth(15));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Rotation ", GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        RotX.boolValue = EditorGUILayout.Toggle(RotX.boolValue, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        RotY.boolValue = EditorGUILayout.Toggle(RotY.boolValue, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        RotZ.boolValue = EditorGUILayout.Toggle(RotZ.boolValue, GUILayout.MaxWidth(15));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}