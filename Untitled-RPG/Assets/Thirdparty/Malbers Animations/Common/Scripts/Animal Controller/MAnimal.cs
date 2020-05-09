using UnityEngine;
using MalbersAnimations;
using MalbersAnimations.Utilities;
using System.Collections.Generic;
using MalbersAnimations.Events;
using UnityEngine.Events;
using MalbersAnimations.Scriptables; 

namespace MalbersAnimations.Controller
{
    /// <summary>
    /// This will controll all Animals Motion it is more Modular
    /// Version 1.1
    /// </summary>
   // [RequireComponent(typeof(Animator))]
   // [RequireComponent(typeof(Rigidbody))]
   [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller")]
    public partial class MAnimal : MonoBehaviour, IAnimatorListener, ICharacterMove, IGravity, IMDamage, IMHitLayer , IAnimatorParameters
    {
        //Animal Variables: All variables
        //Animal Movement:  All Locomotion Logic
        //Animal CallBacks: All public methods and behaviors that it can be called outside the script

        #region Editor Show 
        [HideInInspector] public bool showPivots = true;
        [HideInInspector] public int PivotPosDir;
        //[HideInInspector] public Color ShowpivotColor = Color.blue;
        [HideInInspector] public bool showStates = true;
        [HideInInspector] public bool ModeShowAbilities;
        [HideInInspector] public int Editor_Tabs1;
        [HideInInspector] public int Editor_Tabs2;
        [HideInInspector] public int SelectedMode;
        #endregion

#if UNITY_EDITOR
        void Reset()
        {
            MalbersTools.SetLayer(transform, 20);     //Set all the Childrens to Animal Layer   .
            gameObject.tag = "Animal";                //Set the Animal to Tag Animal
            AnimatorSpeed = 1;

            Anim = GetComponentInParent<Animator>();            //Cache the Animator
            RB = GetComponentInParent<Rigidbody>();             //Catche the Rigid Body  

            if (RB == null)
            {
                RB = gameObject.AddComponent<Rigidbody>();
                RB.useGravity = false;
                RB.constraints = RigidbodyConstraints.FreezeRotation;
                RB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            speedSets = new List<MSpeedSet>(1)
            {
                new MSpeedSet()
            {
                name = "Ground",
                    StartVerticalIndex = new Scriptables.IntReference(1),
                    TopIndex = new Scriptables.IntReference(2),
                    states =  new  List<StateID>(2) { MalbersTools.GetInstance<StateID>("Idle") , MalbersTools.GetInstance<StateID>("Locomotion")},
                    Speeds =  new  List<MSpeed>(3) { new MSpeed("Walk",1,4,4) , new MSpeed("Trot", 2, 4, 4), new MSpeed("Run", 3, 4, 4) }
            }
            };

            BoolVar useCameraInp = MalbersTools.GetInstance<BoolVar>("Global Camera Input");
            BoolVar globalSmooth = MalbersTools.GetInstance<BoolVar>("Global Smooth Vertical");
            FloatVar globalTurn = MalbersTools.GetInstance<FloatVar>("Global Turn Multiplier");

            if (useCameraInp != null) useCameraInput.Variable = useCameraInp;
            if (globalSmooth != null) SmoothVertical.Variable = globalSmooth;
            if (globalTurn != null) TurnMultiplier.Variable = globalTurn;

            CalculateHeight();

        }

        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {

            MEventListener listener = GetComponent<MEventListener>();

            if (listener == null) listener = gameObject.AddComponent<MEventListener>();
            if (listener.Events == null) listener.Events = new List<MEventItemListener>();

            MEvent MovementMobile = MalbersTools.GetInstance<MEvent>("Set Movement Mobile");
            if (listener.Events.Find(item => item.Event == MovementMobile) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = MovementMobile,
                    useVoid = true,
                    useVector2 = true,
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseVector2, SetInputAxis);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.Response, UseCameraBasedInput);

                listener.Events.Add(item);

                Debug.Log("<B>Set Movement Mobile</B> Added to the Event Listeners");
            }

            //********************************//

            SetModesListeners(listener, "Set Attack1", "Attack1");
            SetModesListeners(listener, "Set Attack2", "Attack2");
            SetModesListeners(listener, "Set Action", "Action");

            /************************/

            MEvent actionstatus = MalbersTools.GetInstance<MEvent>("Set Action Status");
            if (listener.Events.Find(item => item.Event == actionstatus) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = actionstatus,
                    useVoid = false,
                    useInt = true, useFloat = true
                };

                ModeID ac = MalbersTools.GetInstance<ModeID>("Action");
                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener(item.ResponseInt, Mode_Pin, ac);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Mode_Pin_Status);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseFloat, Mode_Pin_Time);

                listener.Events.Add(item);

                Debug.Log("<B>Set Action Status</B> Added to the Event Listeners");
            }


            /************************/
            SetStateListeners(listener, "Set Jump", "Jump");
            SetStateListeners(listener, "Set Fly", "Fly");
            /************************/
        }

        void SetModesListeners(MEventListener listener ,string EventName, string ModeName)
        {
            MEvent e = MalbersTools.GetInstance<MEvent>(EventName);
            if (listener.Events.Find(item => item.Event == e) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = e,
                    useVoid = true, useInt = true, useBool = true,
                };

                ModeID att2 = MalbersTools.GetInstance<ModeID>(ModeName);

                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<ModeID>(item.ResponseBool, Mode_Pin, att2);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseBool, Mode_Pin_Input);
                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<ModeID>(item.ResponseInt, Mode_Pin, att2);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Mode_Pin_Ability);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.Response, Mode_Interrupt);

                listener.Events.Add(item);

                Debug.Log("<B>"+EventName+"</B> Added to the Event Listeners");
            }
        }

        void SetStateListeners(MEventListener listener, string EventName, string statename)
        {
            MEvent e = MalbersTools.GetInstance<MEvent>(EventName);
            if (listener.Events.Find(item => item.Event == e) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = e,
                    useVoid = false,
                    useInt = true,
                    useBool = true,
                };

                StateID ss = MalbersTools.GetInstance<StateID>(statename);

                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<StateID>(item.ResponseBool, State_Pin, ss);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseBool, State_Pin_ByInput);

                listener.Events.Add(item);

                Debug.Log("<B>" + EventName + "</B> Added to the Event Listeners");
            }
        }

        private void OnDrawGizmosSelected()
        {
            float sc = transform.localScale.y;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Center, 0.02f * sc);
        }

        void OnDrawGizmos()
        {
            if (!debugGizmos) return;

            float sc = transform.localScale.y;

            if (Application.isPlaying)
            {
                if (ActiveState != null) ActiveState.DebugState();

                if (MainRay)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(hit_Hip.point+AdditivePosition, RayCastRadius * sc);
                    Debug.DrawRay(hit_Hip.point + AdditivePosition, hit_Hip.normal * 0.2f, Color.blue * sc);
                }
                if (FrontRay)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(hit_Chest.point + AdditivePosition, RayCastRadius * sc);
                    Debug.DrawRay(hit_Chest.point + AdditivePosition, hit_Chest.normal * 0.2f, Color.red * sc);
                }

                Gizmos.color = Color.black;
                Gizmos.DrawSphere(_transform.position + DeltaPos, 0.02f * sc);
            }

             

            if (showPivots)
            {
                foreach (var pivot in pivots)
                {
                    if (pivot != null)
                    {
                        if (pivot.PivotColor.a == 0)
                        {
                            pivot.PivotColor = Color.blue;
                        }

                        Gizmos.color = pivot.PivotColor;
                        Gizmos.DrawWireSphere(pivot.World(transform), sc * RayCastRadius);
                        Gizmos.DrawRay(pivot.World(transform), pivot.WorldDir(transform) * pivot.multiplier * sc);
                    }
                }
            }
        }
#endif
    }

    [System.Serializable] public class AnimalEvent : UnityEvent<MAnimal> { }
}
