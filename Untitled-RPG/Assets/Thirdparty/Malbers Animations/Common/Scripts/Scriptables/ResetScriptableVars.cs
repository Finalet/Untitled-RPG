using UnityEngine;
using System.Collections.Generic;

namespace MalbersAnimations.Scriptables
{
    public class ResetScriptableVars : MonoBehaviour
    {
        public bool ResetOnEnable = true;
        public bool ResetOnDisable = false;
        public List<ScriptableVarReseter> vars;


        // Use this for initialization
        void OnEnable()
        {
            if (ResetOnEnable)
                ResetVars();
        }

        void OnDisable()
        {
            if (ResetOnDisable)
                ResetVars(); 
        }

        public virtual void ResetVars()
        {
            foreach (var v in vars) v.Reset();
        }
    }
    
    [System.Serializable]
    public struct ScriptableVarReseter
    {
        public ScriptableVar Var;
        public BoolReference DefaultBool;
        public IntReference DefaultInt;
        public FloatReference DefaultFloat;
        public StringReference DefaultString;
        public Vector2Reference DefaultVector2;
        public Vector3Reference DefaultVector3;
        public ColorReference DefaultColor;

        public void Reset()
        {
            if (Var is IntVar) (Var as IntVar).Value = DefaultInt;
            else if (Var is BoolVar) (Var as BoolVar).Value = DefaultBool;
            else if (Var is FloatVar) (Var as FloatVar).Value = DefaultFloat;
            else if (Var is StringVar) (Var as StringVar).Value = DefaultString;
            else if (Var is Vector3Var) (Var as Vector3Var).Value = DefaultVector3;
            else if (Var is Vector2Var)  (Var as Vector2Var).Value = DefaultVector2;
            else if (Var is ColorVar)     (Var as ColorVar).Value = DefaultColor;
        }
    }
}