using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.Events;


namespace MalbersAnimations
{
    [DefaultExecutionOrder(750)]
    [AddComponentMenu("Malbers/Variables/Vector3 Listener")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/variable-listeners-and-comparers")]
    public class Vector3VarListener : VarListener
    {
        public Vector3Reference value = new Vector3Reference();
        public Vector3Event OnValue = new Vector3Event();
      
        public Vector3 Value
        {
            get => value;
            set
            {
                this.value.Value = value;
                Invoke(value); 
            }
        }

        void OnEnable()
        {
            if (value.Variable != null) value.Variable.OnValueChanged += Invoke;
            Invoke(value);
        }

        void OnDisable()
        {
            if (value.Variable != null) value.Variable.OnValueChanged -= Invoke;
        }

        public virtual void Invoke(Vector3 value)
        {
            if (Enable)
            { 
                 OnValue.Invoke(value);

#if UNITY_EDITOR
                if (debug) Debug.Log($"Vector3Var: ID [{ID.Value}] -> [{name}] -> [{value}]");
#endif
            }
        }

        private void OnDrawGizmosSelected()
        {
            Debug.DrawRay(transform.position, Value, Color.white);
        }
    }




    //INSPECTOR
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Vector3VarListener)), UnityEditor.CanEditMultipleObjects]
    public class V3ListenerEditor : VarListenerEditor
    {
        private UnityEditor.SerializedProperty OnTrue;

        private void OnEnable()
        {
            base.SetEnable();
            OnTrue = serializedObject.FindProperty("OnValue");
        }

        protected override void DrawEvents()
        {
            UnityEditor.EditorGUILayout.PropertyField(OnTrue);
        }
    }
#endif
}
