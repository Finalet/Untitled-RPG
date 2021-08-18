using UnityEngine;
using System.Collections;
using MalbersAnimations.Scriptables;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif


namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Events/Messages")] 
    public class Messages : MonoBehaviour
    {
        public MesssageItem[] messages;                                     //Store messages to send it when Enter the animation State
        public bool UseSendMessage = true;
        public bool SendToChildren = true;
        public bool debug = true;

        public bool nextFrame = false;
        public Component Pinned;
         

        public virtual void SendMessage(GameObject component) => SendMessage(component.transform);

        public virtual void Pin_Receiver(GameObject component) => Pinned = component.transform;
        public virtual void Pin_Receiver(Component component) => Pinned = component;
        public virtual void SendMessage(int index)
        {
            var m = messages[index];  

            if (nextFrame)
            {
                StartCoroutine(CNextFrame(messages[index], Pinned));
            }
            else
            {
                Deliver(m, Pinned);
            }
        }

        public virtual void SendMessage(Component go)
        {
            foreach (var m in messages)
            { 
                if (nextFrame)
                {
                    StartCoroutine(CNextFrame(m, go));
                }
                else
                {
                    Deliver(m, go);
                }
            }
        }


        IEnumerator CNextFrame(MesssageItem m, Component component)
        {
            yield return null;
            Deliver(m, component);
        }

        private void Deliver(MesssageItem m, Component go)
        {
            if (UseSendMessage)
               m.DeliverMessage(go, SendToChildren, debug);
            else
            {
                IAnimatorListener[] listeners;

                if (SendToChildren)
                    listeners = go.GetComponentsInChildren<IAnimatorListener>();
                else
                    listeners = go.GetComponents<IAnimatorListener>();

                if (listeners != null && listeners.Length > 0)
                {
                    foreach (var animListeners in listeners)
                        m.DeliverAnimListener(animListeners,debug);
                }
            }
        } 
    }

    [System.Serializable]
    public class MesssageItem
    {
        public string message;
        public TypeMessage typeM;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;
        public IntVar intVarValue;
        public Transform transformValue;
        public GameObject GoValue;
        public Component ComponentValue;

        public float time;
        public bool sent;
        public bool Active = true;

        public MesssageItem()
        {
            message = string.Empty;
            Active = true;
        }

        public bool IsActive => Active && !string.IsNullOrEmpty(message);


        public void DeliverAnimListener(IAnimatorListener listener, bool debug = false)
        {
            if (!IsActive) return; //Mean the Message cannot be sent

            string val = "";
            bool succesful = false;
            switch (typeM)
            {
                case TypeMessage.Bool:
                    succesful = listener.OnAnimatorBehaviourMessage(message, boolValue);
                    val = boolValue.ToString();
                    break;
                case TypeMessage.Int:
                    succesful = listener.OnAnimatorBehaviourMessage(message, intValue);
                    val = intValue.ToString();
                    break;
                case TypeMessage.Float:
                    succesful = listener.OnAnimatorBehaviourMessage(message, floatValue);
                    val = floatValue.ToString();
                    break;
                case TypeMessage.String:
                    succesful = listener.OnAnimatorBehaviourMessage(message, stringValue);
                    val = stringValue.ToString();
                    break;
                case TypeMessage.Void:
                    succesful = listener.OnAnimatorBehaviourMessage(message, null);
                    val = "Void";
                    break;
                case TypeMessage.IntVar:
                    succesful = listener.OnAnimatorBehaviourMessage(message, (int)intVarValue);
                    val = intVarValue.name.ToString();
                    break;
                case TypeMessage.Transform:
                    succesful = listener.OnAnimatorBehaviourMessage(message, transformValue);
                    val = transformValue.name.ToString();
                    break;
                case TypeMessage.GameObject:
                    succesful = listener.OnAnimatorBehaviourMessage(message, GoValue);
                    val = GoValue.name.ToString();
                    break;
                case TypeMessage.Component:
                    succesful = listener.OnAnimatorBehaviourMessage(message, ComponentValue);
                    val = GoValue.name.ToString();
                    break;
                default:
                    break;
            }

            if (debug && succesful) Debug.Log($"<b>[Msg: {message}->{val}] [{typeM}]</b> T:{Time.time:F3}");  //Debug
        }


        /// <summary>  Using Message to the Monovehaviours asociated to this animator delivery with Send Message  </summary>
        public void DeliverMessage(Component anim, bool SendToChildren, bool debug = false)
        {
            if (!IsActive) return; //Mean the Message cannot be sent

            switch (typeM)
            {
                case TypeMessage.Bool:
                    SendMessage(anim, message, boolValue, SendToChildren);

                    break;
                case TypeMessage.Int:
                    SendMessage(anim, message, intValue, SendToChildren);
                    break;
                case TypeMessage.Float:
                    SendMessage(anim, message, floatValue, SendToChildren);
                    break;
                case TypeMessage.String:
                    SendMessage(anim, message, stringValue, SendToChildren);
                    break;
                case TypeMessage.Void:
                    SendMessageVoid(anim, message, SendToChildren);
                    break;
                case TypeMessage.IntVar:
                    SendMessage(anim, message, (int)intVarValue, SendToChildren);
                    break;
                case TypeMessage.Transform:
                    SendMessage(anim, message, transformValue, SendToChildren);
                    break;
                case TypeMessage.GameObject:
                    SendMessage(anim, message, GoValue, SendToChildren);
                    break;
                case TypeMessage.Component:
                    SendMessage(anim, message, ComponentValue, SendToChildren);
                    break;
                default:
                    break;
            }

            if (debug) Debug.Log($"<b>[Send Msg: {message}->] [{typeM}]</b> T:{Time.time:F3}", anim);  //Debug
        }

        private void SendMessage(Component anim, string message, object value, bool SendToChildren)
        {
            if (SendToChildren)
                anim.BroadcastMessage(message, value, SendMessageOptions.DontRequireReceiver);
            else
                anim.SendMessage(message, value, SendMessageOptions.DontRequireReceiver);
        }


        private void SendMessageVoid(Component anim, string message, bool SendToChildren)
        {
            if (SendToChildren)
                anim.BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
            else
                anim.SendMessage(message, SendMessageOptions.DontRequireReceiver);
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MesssageItem))]
    public class MessageDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // position.y += 2;

            EditorGUI.BeginProperty(position, label, property);
            //GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            //var height = EditorGUIUtility.singleLineHeight;

            //PROPERTIES

            var Active = property.FindPropertyRelative("Active");
            var message = property.FindPropertyRelative("message");
            var typeM = property.FindPropertyRelative("typeM");

            var rect = new Rect(position);

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(R_0, Active, GUIContent.none);

            Rect R_1 = new Rect(rect.x + 15, rect.y, (rect.width / 3) + 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(R_1, message, GUIContent.none);


            Rect R_3 = new Rect(rect.x + ((rect.width) / 3) + 5 + 30, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(R_3, typeM, GUIContent.none);


            Rect R_5 = new Rect(rect.x + ((rect.width) / 3) * 2 + 5 + 15, rect.y, ((rect.width) / 3) - 5 - 15, EditorGUIUtility.singleLineHeight);
            var TypeM = (TypeMessage)typeM.intValue;

            SerializedProperty messageValue = property.FindPropertyRelative("boolValue");

            switch (TypeM)
            {
                case TypeMessage.Bool:
                    messageValue.boolValue = EditorGUI.ToggleLeft(R_5, messageValue.boolValue ? " True" : " False", messageValue.boolValue);
                    break;
                case TypeMessage.Int:
                    messageValue = property.FindPropertyRelative("intValue");
                    break;
                case TypeMessage.Float:
                    messageValue = property.FindPropertyRelative("floatValue");
                    break;
                case TypeMessage.String:
                    messageValue = property.FindPropertyRelative("stringValue");
                    break;
                case TypeMessage.IntVar:
                    messageValue = property.FindPropertyRelative("intVarValue");
                    break;
                case TypeMessage.Transform:
                    messageValue = property.FindPropertyRelative("transformValue");
                    break;
                case TypeMessage.Void:
                    break;
                case TypeMessage.GameObject:
                    messageValue = property.FindPropertyRelative("GoValue");
                    break;
                case TypeMessage.Component:
                    messageValue = property.FindPropertyRelative("ComponentValue");
                    break;
                default:
                    break;
            }

            if (TypeM != TypeMessage.Void && TypeM != TypeMessage.Bool)
            {
                EditorGUI.PropertyField(R_5, messageValue, GUIContent.none);
            }


            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }


    //INSPECTOR
    [CustomEditor(typeof(Messages))]
    public class MessagesEd : Editor
    {
        private ReorderableList list;

       // private Messages MMessage;
        private SerializedProperty sp_messages, debug, SendToChildren, UseSendMessage;

        private void OnEnable()
        {
            sp_messages = serializedObject.FindProperty("messages");
            debug = serializedObject.FindProperty("debug");
            SendToChildren = serializedObject.FindProperty("SendToChildren");
            UseSendMessage = serializedObject.FindProperty("UseSendMessage");

            list = new ReorderableList(serializedObject, sp_messages, true, true, true, true)
            {
                drawHeaderCallback = HeaderCallbackDelegate1,
               
                drawElementCallback = ( rect,  index,  isActive,  isFocused) => 
                {
                    EditorGUI.PropertyField(rect, sp_messages.GetArrayElementAtIndex(index), GUIContent.none);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Send Messages to all the MonoBehaviours that uses the Interface |IAnimatorListener|." +
                "\nEnable [SendMessage] to use Component.SendMessage() instead");

            EditorGUILayout.BeginVertical(MTools.StyleGray);
            {
                list.DoLayoutList();

                EditorGUILayout.BeginHorizontal();
                var currentGUIColor = GUI.color;

                GUI.color = SendToChildren.boolValue ? (Color.green) : currentGUIColor;

                SendToChildren.boolValue = GUILayout.Toggle(SendToChildren.boolValue,
                    new GUIContent("Children", "The Messages will be sent also to the gameobject children"), EditorStyles.miniButton);
                 

                GUI.color = UseSendMessage.boolValue ? (Color.green) : currentGUIColor;
                UseSendMessage.boolValue = GUILayout.Toggle(UseSendMessage.boolValue,
                    new GUIContent("SendMessage()", "Uses the SendMessage() method, instead of checking for IAnimator Listener Interfaces"), EditorStyles.miniButton);
                GUI.color = currentGUIColor;

                MalbersEditor.DrawDebugIcon(debug);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        static void HeaderCallbackDelegate1(Rect rect)
        {
            var width = (rect.width / 3);
            var height = EditorGUIUtility.singleLineHeight;

            Rect R_1 = new Rect(rect.x + 10, rect.y, width + 30, height);
            EditorGUI.LabelField(R_1, "Message");

            Rect R_3 = new Rect(rect.x + 10 + width + 5 + 30, rect.y, width - 20, height);
            EditorGUI.LabelField(R_3, "Type");

            Rect R_5 = new Rect(rect.x + 10 + width * 2 + 20, rect.y, width - 20, height);
            EditorGUI.LabelField(R_5, "Value");
        } 
    }
#endif
}