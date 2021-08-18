using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/modes#mode-behaviour")]
    public class ModeBehaviour : StateMachineBehaviour
    { 
        public ModeID ModeID;

        [Tooltip("Calls 'Animation Tag Enter' on the Modes")]  
        public bool EnterMode = true;
        [Tooltip("Calls 'Animation Tag Exit' on the Modes")]
        public bool ExitMode = true;

        [Tooltip("Used for Playing an Ability and Finish on another Ability Mode")]
        public bool ExitOnAbility = false;
        [Tooltip("Next Ability to do on the Mode. -1 is the Default and the Exit On Ability Logic will be ignored")] 
        public int ExitAbility = -1;
       
        private MAnimal animal;
        private Mode ModeOwner;
        private Ability ActiveAbility;


        public void InitializeBehaviour(MAnimal animal)
        {
            this.animal = animal;
            if (ModeID != null)
            {
                ModeOwner = animal.Mode_Get(ModeID);
            }
            else
            {
                Debug.LogWarning("There's a Mode behaviour without an ID. Please check all your Mode Animations states.");
                Destroy(this);
            }
        }

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (ModeID == null)  { Debug.LogError("Mode behaviour needs an ID"); return; }
            if (ModeOwner == null) { Debug.LogError($"There's no [{ModeID.name}] mode on your character"); return; }

            if (animal.ModeStatus == Int_ID.Loop) return;            //Means is Looping so Skip!!!

            ActiveAbility = ModeOwner.ActiveAbility;
            if (EnterMode)   ModeOwner.AnimationTagEnter();
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!animal) return; 
            
            //Means is Looping to itself So Skip the Exit Mode EXTREMELY IMPORTANT
            if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash) return;
            
            if (ExitMode) ModeOwner.AnimationTagExit(ActiveAbility, ExitOnAbility ? ExitAbility : -1);
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            ModeOwner.OnModeStateMove(stateInfo, animator, layerIndex);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ModeBehaviour))]
    public class ModeBehaviourED : Editor
    {
        SerializedProperty EnterMode, ExitMode, ModeID, ExitOnAbility, ExitAbility;
        Color RequiredColor = new Color(1,0.4f,0.4f,1);

        void OnEnable()
        {

            ModeID = serializedObject.FindProperty("ModeID");
            EnterMode = serializedObject.FindProperty("EnterMode");
            ExitMode = serializedObject.FindProperty("ExitMode");
            ExitOnAbility = serializedObject.FindProperty("ExitOnAbility");
            ExitAbility = serializedObject.FindProperty("ExitAbility");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
          
            var currentGUIColor = GUI.color;
            GUI.color = ModeID.objectReferenceValue == null ? RequiredColor : currentGUIColor;
            EditorGUIUtility.labelWidth = 70;
            EditorGUILayout.PropertyField(ModeID);

            var width = 42; 
            GUI.color = EnterMode.boolValue ? Color.green : currentGUIColor;

            EnterMode.boolValue = GUILayout.Toggle(EnterMode.boolValue,
                               new GUIContent("Enter"), EditorStyles.miniButton, GUILayout.Width(width));

            
            GUI.color = ExitMode.boolValue ? Color.green : currentGUIColor;
             
            ExitMode.boolValue = GUILayout.Toggle(ExitMode.boolValue,
                               new GUIContent("Exit"), EditorStyles.miniButton, GUILayout.Width(width));

            GUI.color = currentGUIColor;
           
            EditorGUILayout.EndHorizontal();

            if (ExitMode.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                ExitOnAbility.boolValue = EditorGUILayout.ToggleLeft( 
                    new GUIContent("Exit on Ability", "Used for Playing an Ability and Finish on another Ability Mode."), ExitOnAbility.boolValue, GUILayout.Width(105)); 
              
                if (ExitOnAbility.boolValue) 
                    EditorGUILayout.PropertyField(ExitAbility,
                    new GUIContent("   ", "Next Ability to do on the Mode. -1 is the Default and the Exit On Ability Logic will be ignored.\n" +
                    "Used for Playing an Ability and Finish on another Ability Mode. E.g.: The wolf can Howl he is Sit. The Howl Ability will go to the Sit Ability Right After"));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUIUtility.labelWidth =0;
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}