using System.Linq;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    using Scriptables;
    using System.Collections.Generic;

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Var Listener", order = 5)]
    public class CheckVarListener : MAIDecision
    {

        public override string DisplayName => "Variables/Check Variable Listener";


        public enum Affect
        {
            Self,
            Target,
            Tag, 
            Transform,
            GameObject, 
            RuntimeGameObjectSet
        }

        public enum ComponentPlace
        {
            Self,
            Parent,
            Child
        }

        [Space]
        /// <summary>Range for Looking forward and Finding something</summary>
        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target), or on an object with a tag")]
        public Affect checkOn = Affect.Self;
        
        [Tooltip("Check if the Var Listener component is in the same GameObject, is on its parent or is on any of the childs")]
        public ComponentPlace Placed = ComponentPlace.Self;
        [Hide("showTag", true, false)] public Tag tag;
        [Hide("showTrans", true, false)] public TransformVar Transform;
        [Hide("showGO", true, false)] public GameObjectVar GameObject;
        [Hide("showGOSet", true, false)] public RuntimeGameObjects GameObjectSet;


        [Space,
            Tooltip("Check on the Target or Self if it has a Listener Variable Component <Int><Bool><Float> and compares it with the local variable)")]
        public VarType varType = VarType.Bool;


        [Hide("showBoolValue", true, true)] public ComparerInt comparer;

        [Hide("showBoolValue", true)] public bool boolValue = true;
        [Hide("showIntValue", true)] public int intValue = 0;
        [Hide("showFloatValue", true)] public float floatValue = 0f;


        [Tooltip("Check the Variable Listener ID Value, when this value is Zero, the ID is ignored")]
        public IntReference ListenerID = 0;
        public bool debug = false;

        [HideInInspector] public bool showFloatValue;
        [HideInInspector] public bool showBoolValue = true;
        [HideInInspector] public bool showIntValue;
        [HideInInspector] public bool showTag;
        [HideInInspector] public bool showTrans;
        [HideInInspector] public bool showGO;
        [HideInInspector] public bool showGOSet;


        private void OnValidate()
        {
            showTag = checkOn == Affect.Tag;
            showTrans = checkOn == Affect.Transform;
            showGO = checkOn == Affect.GameObject;
            showGOSet = checkOn == Affect.RuntimeGameObjectSet;

            switch (varType)
            {
                case VarType.Bool:
                    showFloatValue = false;
                    showBoolValue = true;
                    showIntValue = false;
                    break;
                case VarType.Int:
                    showFloatValue = false;
                    showBoolValue = false;
                    showIntValue = true;
                    break;
                case VarType.Float:
                    showFloatValue = true;
                    showBoolValue = false;
                    showIntValue = false;
                    break;
                default:
                    break;
            }
        }


        public override void PrepareDecision(MAnimalBrain brain, int Index)
        {
            brain.DecisionsVars[Index].Components = null;

            MonoBehaviour[] monoValue = null;

            var objectives = GetObjective(brain);

            if (objectives != null && objectives.Length >0)
            {
                foreach (var targets in objectives)
                {
                  //  Debug.Log("targets = " + targets);

                    switch (varType)
                        {
                            case VarType.Bool:
                                monoValue = GetComponents<BoolVarListener>(targets.gameObject);
                                break;
                            case VarType.Int:
                                monoValue = GetComponents<IntVarListener>(targets.gameObject);
                                break;
                            case VarType.Float:
                                monoValue = GetComponents<FloatVarListener>(targets.gameObject);
                                break;
                        }
                }
            }

            brain.DecisionsVars[Index].Components = monoValue;
        }

        

        private Transform[] GetObjective(MAnimalBrain brain)
        {
            switch (checkOn)
            {
                case Affect.Self:
                    return new Transform[1] { brain.transform };
                case Affect.Target:
                    return new Transform[1] { brain.Target };
                case Affect.Tag:
                    var tagH = Tags.TagsHolders.FindAll(X => X.HasTag(tag));
                    if (tagH != null)
                    {
                        var TArray = new List<Transform>();
                        foreach (var t in tagH)TArray.Add(t.transform);
                        return TArray.ToArray(); 
                    }
                        return null;
                case Affect.Transform:
                    return new Transform[1] { Transform.Value };
                case Affect.GameObject:
                    if (!GameObject.Value.IsPrefab())
                        return new Transform[1] { GameObject.Value.transform };
                    return null;
                case Affect.RuntimeGameObjectSet:
                    var TGOS = new List<Transform>();
                    foreach (var t in GameObjectSet.Items) TGOS.Add(t.transform);
                    return TGOS.ToArray();
                default:
                    return null;
            }
        }

        

        private TVarListener[] GetComponents<TVarListener>(GameObject gameObject)
            where TVarListener : VarListener
        {
            TVarListener[] list;

            switch (Placed)
            {
                case ComponentPlace.Child:
                    list = gameObject.GetComponentsInChildren<TVarListener>();
                    break;
                case ComponentPlace.Parent:
                    list = gameObject.GetComponentsInParent<TVarListener>();
                    break;
                case ComponentPlace.Self:
                    list = gameObject.GetComponents<TVarListener>();
                    break;
                default:
                    list = gameObject.GetComponents<TVarListener>();
                    break;
            }

            list = list.ToList().FindAll(x => ListenerID.Value == 0 || x.ID == ListenerID.Value).ToArray();

            return list;
        }

        public override bool Decide(MAnimalBrain brain, int Index)
        {
            var listeners = brain.DecisionsVars[Index].Components;

            if (listeners == null || listeners.Length == 0) return false;

            bool result = false;

            foreach (var varListener in listeners)
            {
                switch (varType)
                {
                    case VarType.Bool:
                        var LB = (varListener as BoolVarListener);
                         result = LB.Value == boolValue;
                        if (debug)
                            Debug.Log($"{brain.Animal.name}: <B>[{name}]</B> ListenerBool<{LB.transform.name}> ID<{LB.ID.Value}> Value<{LB.Value}>  <B>Result[{result}]</B>");
                        break;
                    case VarType.Int:
                        var LI = (varListener as IntVarListener);
                        result = CompareInteger(LI.Value);
                        if (debug)
                            Debug.Log($"{brain.Animal.name}: <B>[{name}]</B> ListenerInt<{LI.transform.name}> ID<{LI.ID.Value}> Value<{LI.Value}>  <B>Result[{result}]</B>");
                        break;
                    case VarType.Float:
                        var LF = (varListener as FloatVarListener);
                        result =  CompareFloat(LF.Value);
                        if (debug)
                            Debug.Log($"{brain.Animal.name}: <B>[{name}]</B> ListenerInt<{LF.transform.name}> ID<{LF.ID.Value}> Value<{LF.Value}>  <B>Result[{result}]</B>");
                        break;
                    default:
                        return false;
                }
            }
            return result;
        }

        //public override void FinishDecision(MAnimalBrain brain, int Index)
        //{
        //    brain.DecisionsVars[Index].MonoBehaviours = null;
        //}

        public enum VarType
        {
            Bool,
            Int,
            Float
        }

        public enum BoolType
        {
            True,
            False
        }


        public bool CompareInteger(int IntValue)
        {
            switch (comparer)
            {
                case ComparerInt.Equal:
                    return (IntValue == intValue);
                case ComparerInt.Greater:
                    return (IntValue > intValue);
                case ComparerInt.Less:
                    return (IntValue < intValue);
                case ComparerInt.NotEqual:
                    return (IntValue != intValue);
                default:
                    return false;
            }
        }

        public bool CompareFloat(float IntValue)
        {
            switch (comparer)
            {
                case ComparerInt.Equal:
                    return (IntValue == floatValue);
                case ComparerInt.Greater:
                    return (IntValue > floatValue);
                case ComparerInt.Less:
                    return (IntValue < floatValue);
                case ComparerInt.NotEqual:
                    return (IntValue != floatValue);
                default:
                    return false;
            }
        }
    }
}