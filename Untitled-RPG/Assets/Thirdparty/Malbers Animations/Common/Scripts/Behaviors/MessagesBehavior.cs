using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using System.Linq;
using UnityEngine.Animations;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations
{
    public class MessagesBehavior : StateMachineBehaviour
    {
        public bool UseSendMessage;
        public bool SendToChildren = false;
        public bool debug;
        public bool NormalizeTime = true;

        public MesssageItem[] onEnterMessage;   //Store messages to send it when Enter the animation State
        public MesssageItem[] onExitMessage;    //Store messages to send it when Exit  the animation State
        public MesssageItem[] onTimeMessage;    //Store messages to send on a specific time  in the animation State

        IAnimatorListener[] listeners;         //To all the MonoBehavious that Have this 

        private bool firstime = false;

        public bool OnEnter = true;
        public bool OnExit= true;
        public bool OnTime = true;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!firstime)
            {
                if (SendToChildren)
                    listeners = animator.GetComponentsInChildren<IAnimatorListener>();
                else
                    listeners = animator.GetComponents<IAnimatorListener>();
                firstime = true;
               
            }

           if (OnTime)
                foreach (MesssageItem ontimeM in onTimeMessage) ontimeM.sent = false;  //Set all the messages Ontime Sent = false when start

            if (OnEnter)
            {
                foreach (MesssageItem onEnterM in onEnterMessage)
                {
                    if (onEnterM.Active && !string.IsNullOrEmpty(onEnterM.message))
                    {
                        if (UseSendMessage)
                            onEnterM.DeliverMessage(animator, SendToChildren, debug);
                        else
                            foreach (var animListener in listeners)
                                onEnterM.DeliverAnimListener(animListener,debug);
                    }
                }
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
        {
            if (OnExit)
            {
                foreach (MesssageItem onExitM in onExitMessage)
                {
                    if (onExitM.Active && !string.IsNullOrEmpty(onExitM.message))
                    {
                        if (onEnterMessage != null && onEnterMessage.Length > 0 && onEnterMessage.ToList().Exists(x => x.message == onExitM.message))
                        {
                            if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash)
                            {
                                return;   //Means is Looping to itself So Skip the Exit Mode because an Enter Mode is Playing
                            }
                        }

                        if (UseSendMessage)
                            onExitM.DeliverMessage(animator, SendToChildren, debug);
                        else
                            foreach (var animListener in listeners)
                                onExitM.DeliverAnimListener(animListener, debug);
                    }
                }
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (OnTime)
            {
                if (stateInfo.fullPathHash == animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash) return; //means is transitioning to itself

                foreach (MesssageItem onTimeM in onTimeMessage)
                {
                    if (onTimeM.Active && !string.IsNullOrEmpty(onTimeM.message))
                    {
                        // float stateTime = stateInfo.loop ? stateInfo.normalizedTime % 1 : stateInfo.normalizedTime;
                        float stateTime = NormalizeTime ? stateInfo.normalizedTime % 1 : stateInfo.normalizedTime;

                        if (!onTimeM.sent && (stateTime >= onTimeM.time))
                        {
                            onTimeM.sent = true;

                            if (UseSendMessage)
                                onTimeM.DeliverMessage(animator, SendToChildren, debug);

                            else
                                foreach (var item in listeners)
                                    onTimeM.DeliverAnimListener(item, debug);
                        }
                    }
                }
            }
        } 
    }
   
    //INSPECTOR

#if UNITY_EDITOR
    [CustomEditor(typeof(MessagesBehavior))]
    public class MessageBehaviorsEd : Editor
    {
        private ReorderableList listOnEnter, listOnExit, listOnTime;
        private MessagesBehavior MMessage;
        private SerializedProperty onExitMessage, onEnterMessage, onTimeMessage, UseSendMessage, SendToChildren, NormalizeTime, debug, OnExit, OnTime, OnEnter;


        Color selected = new Color(0, 0.6f, 1f, 1f);

        private void OnEnable()
        {

            MMessage = ((MessagesBehavior)target);
            onExitMessage = serializedObject.FindProperty("onExitMessage");
            onEnterMessage = serializedObject.FindProperty("onEnterMessage");
            onTimeMessage = serializedObject.FindProperty("onTimeMessage");
            UseSendMessage = serializedObject.FindProperty("UseSendMessage");
            SendToChildren = serializedObject.FindProperty("SendToChildren");
            NormalizeTime = serializedObject.FindProperty("NormalizeTime");
            debug = serializedObject.FindProperty("debug");
            OnEnter = serializedObject.FindProperty("OnEnter");
            OnExit = serializedObject.FindProperty("OnExit");
            OnTime = serializedObject.FindProperty("OnTime");

            //script = MonoScript.FromScriptableObject(MMessage);

            listOnEnter = new ReorderableList(serializedObject, onEnterMessage, true, true, true, true);
            listOnExit = new ReorderableList(serializedObject, onExitMessage, true, true, true, true);
            listOnTime = new ReorderableList(serializedObject, onTimeMessage, true, true, true, true);

            listOnEnter.drawHeaderCallback = HeaderCallbackDelegate1;
            listOnExit.drawHeaderCallback = HeaderCallbackDelegate2;
            listOnTime.drawHeaderCallback = HeaderCallbackDelegate3;


            listOnEnter.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                EditorGUI.PropertyField(rect, onEnterMessage.GetArrayElementAtIndex(index), GUIContent.none);
            };

            listOnExit.drawElementCallback =  (rect, index, isActive, isFocused) =>
            {
                EditorGUI.PropertyField(rect, onExitMessage.GetArrayElementAtIndex(index), GUIContent.none);
            };

            listOnTime.drawElementCallback = drawElementCallback3;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Send Messages to all the MonoBehaviours that uses the Interface |IAnimatorListener|");


            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(MTools.StyleGray);
            {
                EditorGUILayout.BeginHorizontal();

                var currentGUIColor = GUI.color;

                GUI.color = OnEnter.boolValue ? selected : currentGUIColor;
                OnEnter.boolValue = GUILayout.Toggle(OnEnter.boolValue,
                                    new GUIContent("Enter", "Send the message On Enter State "), EditorStyles.miniButton);
                
                GUI.color = OnExit.boolValue ? selected : currentGUIColor;
                OnExit.boolValue = GUILayout.Toggle(OnExit.boolValue,
                                   new GUIContent("Exit", "Send the message On Exit State"), EditorStyles.miniButton);


                GUI.color = OnTime.boolValue ? selected : currentGUIColor;

                OnTime.boolValue = GUILayout.Toggle(OnTime.boolValue,
                                  new GUIContent("Time", "Send the message On Update State using a time"), EditorStyles.miniButton);


                GUI.color = SendToChildren.boolValue ? ( Color.green) : currentGUIColor;

                SendToChildren.boolValue = GUILayout.Toggle(SendToChildren.boolValue,
                    new GUIContent("Children", "The Messages will be sent also to the Animator gameobject children"), EditorStyles.miniButton);
             
                
                
                GUI.color = UseSendMessage.boolValue ? (Color.green) : currentGUIColor;
                UseSendMessage.boolValue = GUILayout.Toggle(UseSendMessage.boolValue,
                    new GUIContent("SendMessage()", "Uses the SendMessage() method, instead of checking for IAnimator Listener Interfaces"), EditorStyles.miniButton);

                //EditorGUILayout.PropertyField(SendToChildren, new GUIContent("Message Children", "All the children gameObjects in the hierarchy will receive the message"));

                GUI.color = currentGUIColor;
                
                MalbersEditor.DrawDebugIcon(debug);

                EditorGUILayout.EndHorizontal();

                if (OnEnter.boolValue) 
                    listOnEnter.DoLayoutList();

                if (OnExit.boolValue) 
                    listOnExit.DoLayoutList(); 
              
                if (OnTime.boolValue) 
                    listOnTime.DoLayoutList();  
              
                EditorGUIUtility.labelWidth = 0;
            }

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Message Behaviour Inspector");
                //EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }


        /// <summary>  Reordable List Header  </summary>
        void HeaderCallbackDelegate1(Rect rect)
        {
            Rect R_1 = new Rect(rect.x, rect.y, (rect.width / 3) + 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Msg (On Enter)");

            Rect R_3 = new Rect(rect.x + 10 + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Type");

            Rect R_5 = new Rect(rect.x + 10 + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_5, "Value");
        }

        void HeaderCallbackDelegate2(Rect rect)
        {
            Rect R_1 = new Rect(rect.x, rect.y, (rect.width / 3) + 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Msg (On Exit)");

            Rect R_3 = new Rect(rect.x + 10 + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Type");

            Rect R_5 = new Rect(rect.x + 10 + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_5, "Value");
        }

        void HeaderCallbackDelegate3(Rect rect)
        {
            Rect R_1 = new Rect(rect.x, rect.y, (rect.width / 4) + 30, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Msg (OnTime)");

            Rect R_3 = new Rect(rect.x + 10 + ((rect.width) / 4) + 5 + 30, rect.y,  32, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Type");
            Rect R_4 = new Rect(rect.x + 10 + ((rect.width) / 4) * 2 + 5 + 20, rect.y, ((rect.width) / 4) - 5, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_4, "Time");

            Rect R_4_1 = new Rect(R_4);
            R_4_1.x += 32;
            R_4_1.width = 23;

            NormalizeTime.boolValue = GUI.Toggle(R_4_1,NormalizeTime.boolValue, new GUIContent("N", "Normalize the State Animation Time"),EditorStyles.miniButton);

            Rect R_5 = new Rect(rect.x + ((rect.width) / 4) * 3 + 5 + 10, rect.y, ((rect.width) / 4) - 5, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_5, "Value");

        }

        //ON ENTER
        void drawElementCallback1(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MMessage.onEnterMessage[index];
            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            element.Active = EditorGUI.Toggle(R_0, element.Active);

            Rect R_1 = new Rect(rect.x + 15, rect.y, (rect.width / 3) + 15, EditorGUIUtility.singleLineHeight);
            element.message = EditorGUI.TextField(R_1, element.message);


            Rect R_3 = new Rect(rect.x + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            element.typeM = (TypeMessage)EditorGUI.EnumPopup(R_3, element.typeM);

            Rect R_5 = new Rect(rect.x + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            switch (element.typeM)
            {
                case TypeMessage.Bool:
                    element.boolValue = EditorGUI.ToggleLeft(R_5, element.boolValue ? " True" : " False", element.boolValue);
                    break;
                case TypeMessage.Int:
                    element.intValue = EditorGUI.IntField(R_5, element.intValue);
                    break;
                case TypeMessage.Float:
                    element.floatValue = EditorGUI.FloatField(R_5, element.floatValue);
                    break;
                case TypeMessage.String:
                    element.stringValue = EditorGUI.TextField(R_5, element.stringValue);
                    break;
                case TypeMessage.IntVar:
                    element.intVarValue = (IntVar)EditorGUI.ObjectField(R_5, element.intVarValue, typeof(IntVar), false);
                    break;
                default:
                    break;
            }

        }

        //ON EXIT
        void drawElementCallback2(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MMessage.onExitMessage[index];
            var sp_element = onExitMessage.GetArrayElementAtIndex(index);

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            element.Active = EditorGUI.Toggle(R_0, element.Active);

            Rect R_1 = new Rect(rect.x + 15, rect.y, (rect.width / 3) + 15, EditorGUIUtility.singleLineHeight);
            element.message = EditorGUI.TextField(R_1, element.message);

            Rect R_3 = new Rect(rect.x + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            element.typeM = (TypeMessage)EditorGUI.EnumPopup(R_3, element.typeM);

            Rect R_5 = new Rect(rect.x + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            switch (element.typeM)
            {
                case TypeMessage.Bool:
                    element.boolValue = EditorGUI.ToggleLeft(R_5, element.boolValue ? " True" : " False", element.boolValue);
                    break;
                case TypeMessage.Int:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("intValue"), GUIContent.none);
                    break;
                case TypeMessage.Float:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("floatValue"), GUIContent.none);
                    break;
                case TypeMessage.String:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("stringValue"), GUIContent.none);
                    break;
                case TypeMessage.IntVar:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("intVarValue"), GUIContent.none);
                    break;
                case TypeMessage.Transform:
                    EditorGUI.PropertyField(R_5, sp_element.FindPropertyRelative("transformValue"), GUIContent.none);
                    break;
                default:
                    break;
            }

        }

        //ON Time
        void drawElementCallback3(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MMessage.onTimeMessage[index];
            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            element.Active = EditorGUI.Toggle(R_0, element.Active);

            Rect R_1 = new Rect(rect.x + 15, rect.y, (rect.width / 4) + 15, EditorGUIUtility.singleLineHeight);
            element.message = EditorGUI.TextField(R_1, element.message);

            Rect R_3 = new Rect(rect.x + ((rect.width) / 4) + 5 + 30, rect.y, ((rect.width) / 4) - 5 - 5, EditorGUIUtility.singleLineHeight);
            element.typeM = (TypeMessage)EditorGUI.EnumPopup(R_3, element.typeM);

            Rect R_4 = new Rect(rect.x + ((rect.width) / 4) * 2 + 5 + 25, rect.y, ((rect.width) / 4) - 5 - 15, EditorGUIUtility.singleLineHeight);

            element.time = EditorGUI.FloatField(R_4, element.time);

            //if (element.time > 1) element.time = 1;
             if (element.time < 0) element.time = 0;

            Rect R_5 = new Rect(rect.x + ((rect.width) / 4) * 3 + 15, rect.y, ((rect.width) / 4) - 15, EditorGUIUtility.singleLineHeight);
            switch (element.typeM)
            {
                case TypeMessage.Bool:
                    element.boolValue = EditorGUI.ToggleLeft(R_5, element.boolValue ? " True" : " False", element.boolValue);
                    break;
                case TypeMessage.Int:
                    element.intValue = EditorGUI.IntField(R_5, element.intValue);
                    break;
                case TypeMessage.Float:
                    element.floatValue = EditorGUI.FloatField(R_5, element.floatValue);
                    break;
                case TypeMessage.String:
                    element.stringValue = EditorGUI.TextField(R_5, element.stringValue);
                    break;
                case TypeMessage.IntVar:
                    element.intVarValue = (IntVar)EditorGUI.ObjectField(R_5, element.intVarValue, typeof(IntVar), false);
                    break;
                default:
                    break;
            }

        }

    }
#endif
}