using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [System.Serializable]
    public abstract class ModeModifier : ScriptableObject
    {
        public virtual void OnModeEnter(Mode mode) { }

        public virtual void OnModeMove(Mode mode, AnimatorStateInfo stateinfo, Animator anim, int Layer) { }

        public virtual void OnModeExit(Mode mode) { }

        public virtual void DebugMode(Mode mode) { }
    }
}

