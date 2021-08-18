using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Interaction/Interactor")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/interactor")]
    public class MInteractor : UnityUtils, IInteractor
    {
        [Tooltip ("ID for the Interacter")]
        public IntReference m_ID = new IntReference(0);

        [Tooltip ("Collider set as Trigger to Find Interactables OnTrigger Enter")]
        public Collider InteractionArea;

        [Tooltip("When an Interaction is executed these events will be invoked." +
         "\n\nOnInteractWithGO(GameObject) -> will have the *INTERACTABLE* gameObject as parameter" +
         "\n\nOnInteractWith(Int) -> will have the *INTERACTABLE* ID as parameter")]
        public InteractionEvents events = new InteractionEvents();
        public GameObjectEvent OnFocused = new GameObjectEvent();
        public GameObjectEvent OnUnfocused = new GameObjectEvent();
        
        public int ID => m_ID.Value;

        public bool Active { get => !enabled; set => enabled = !value; }
        
        public GameObject Owner => gameObject;

        /// <summary>Current Interactable this interactor has on its Interaction Area </summary>
        public IInteractable FocusedInteractable { get; set; }
      

        /// <summary>Interaction Trigger Proxy to Subsribe to OnEnter OnExit Trigger</summary>
        public TriggerProxy TriggerArea { get; set; }

        private void OnEnable()
        {
            //Trigger Area
            if (InteractionArea)
            {
                CheckTriggerProxy();
            }

            if (TriggerArea)
            {
                TriggerArea.OnTrigger_Enter.AddListener(TriggerEnter);
                TriggerArea.OnTrigger_Exit.AddListener(TriggerExit);
            }
        }


        private void OnDisable()
        {
            if (TriggerArea)
            {
                TriggerArea.OnTrigger_Enter.RemoveListener(TriggerEnter);
                TriggerArea.OnTrigger_Exit.RemoveListener(TriggerExit);
            }
        }

        private void TriggerEnter(Collider collider)
        {
            if (FocusedInteractable != null) FocusedInteractable.Focused = false; //Just in case it has an interactable stored

            var NewInteractabless = collider.GetComponentsInParent<IInteractable>().ToList();
            IInteractable NewInter = NewInteractabless.Find(x => (x as Behaviour).enabled); //Find the one that is enable

            if (NewInter != null && NewInter.CanInteract) //Ignore One Interaction Interactables
            {
                FocusedInteractable = NewInter;

                if (FocusedInteractable.Auto)
                {
                    Interact(FocusedInteractable); //Interact if the interacter is on Auto
                }
                else
                {
                    FocusedInteractable.FocusingInteractor = this;
                    FocusedInteractable.Focused = true;
                    OnFocused.Invoke(FocusedInteractable.Owner);
                }
            }
        }


        private void TriggerExit(Collider collider)
        {
            if (collider == null) //Means that the Proxy has been hidden
            {
                ResetFocusedItem();
            }
            else
            {
                var NewInteractabless = collider.GetComponentsInParent<IInteractable>().ToList();
                IInteractable NewInter = NewInteractabless.Find(x => (x as Behaviour).enabled); //Find the one that is enable

                if (NewInter == FocusedInteractable)
                {
                    ResetFocusedItem();
                }
            }
        }

        private void ResetFocusedItem()
        {
            if (FocusedInteractable != null)
            {
                FocusedInteractable.FocusingInteractor = this;
                FocusedInteractable.Focused = false;
                FocusedInteractable.FocusingInteractor = null;

                OnUnfocused.Invoke(FocusedInteractable.Owner);
                FocusedInteractable = null;
            }
        }



        /// <summary> Receive an Interaction from the Interacter </summary>
        public void Interact(IInteractable inter)
        {
            if (inter != null && inter.CanInteract)
            {
                FocusedInteractable = inter;

                events.OnInteractWithGO.Invoke(FocusedInteractable.Owner);
                events.OnInteractWith.Invoke(FocusedInteractable.ID);
                FocusedInteractable.Interact(this);

                if (FocusedInteractable.SingleInteraction) FocusedInteractable = null; //Clear the Focus interactable since it was a single interaction

            }
        }

        public void Restart()
        {
            FocusedInteractable = null;
            OnUnfocused.Invoke(null);
        }

       

        public void Interact(GameObject interactable)
        {
             Interact(interactable?.FindInterface<IInteractable>());
        }

        public void Interact(Component interactable)
        {
            Interact(interactable?.FindInterface<IInteractable>());
        }

        public void Interact() => Interact(FocusedInteractable);

        

        internal void CheckTriggerProxy()
        {
            if (TriggerArea == null)
            {
                TriggerArea = InteractionArea.GetComponent<TriggerProxy>();
                if (TriggerArea == null) TriggerArea = InteractionArea.gameObject.AddComponent<TriggerProxy>();
            }
        }

        [SerializeField] private int Editor_Tabs1;
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(MInteractor))]
    public class MInteractorEditor : UnityEditor.Editor
    {
        SerializedProperty m_ID, InteractionArea, events,  Editor_Tabs1, OnFocusedInteractable, OnUnfocusedInteractable;
        protected string[] Tabs1 = new string[] { "General", "Events" };

        MInteractor M;

        private void OnEnable()
        {
            M = (MInteractor)target;
            m_ID = serializedObject.FindProperty("m_ID");
            InteractionArea = serializedObject.FindProperty("InteractionArea");
            events = serializedObject.FindProperty("events");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            OnFocusedInteractable = serializedObject.FindProperty("OnFocused");
            OnUnfocusedInteractable = serializedObject.FindProperty("OnUnfocused");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Interactor element that invoke events when interacts with an Interactable");
            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
            if (Editor_Tabs1.intValue == 0) DrawGeneral();
            else DrawEvents();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(m_ID);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(InteractionArea);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                M.CheckTriggerProxy();
                EditorUtility.SetDirty(M.InteractionArea.gameObject);
                EditorUtility.SetDirty(target);
            }

            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
                if (M.FocusedInteractable != null) 
                    EditorGUILayout.ObjectField("Focused Int (" + M.FocusedInteractable.ID + ")", M.FocusedInteractable.Owner, typeof(GameObject), false);
                else 
                    EditorGUILayout.ObjectField("Focused Int", null, typeof(GameObject), false);
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(events);
            
            if (events.isExpanded)
            {
                EditorGUILayout.PropertyField(OnFocusedInteractable);
                EditorGUILayout.PropertyField(OnUnfocusedInteractable);
            }
        }
    }
#endif
}