using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Message", fileName = "New Message", order = 3001)]

    public class MessagesSO : ScriptableObject
    {
        public List<MesssageItem> messages;                                     //Store messages to send it when Enter the animation State
        
        public bool UseSendMessage = true;
        public bool SendToChildren = false;
        public bool debug;
        private IAnimatorListener[] listeners;

        public virtual void SendMessage(GameObject component) => SendMessage(component.transform);

        public virtual void SendMessage(Component go)
        {
            foreach (var m in messages)
            {
                if (m.message == string.Empty || !m.Active) break;          //If the messaje is empty or disabled break.... ignore it
                Deliver(m, go);
            }
        }
         
        private void Deliver(MesssageItem m, Component go)
        {
            if (UseSendMessage)
                 m.DeliverMessage(go.transform.root,SendToChildren,debug);
            else
            {
                if (SendToChildren)
                    listeners = go.GetComponentsInChildren<IAnimatorListener>();
                else
                    listeners = go.GetComponents<IAnimatorListener>();


                if (listeners != null && listeners.Length > 0)
                {
                    foreach (var list in listeners)
                        m.DeliverAnimListener(list, debug);
                }
            }
        } 
    }

    //INSPECTOR

#if UNITY_EDITOR
    [CustomEditor(typeof(MessagesSO))]  public class MessagesSOEd : MessagesEd{ }
#endif
}