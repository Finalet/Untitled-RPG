#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
 
 public class FinaleAnimUnhideTool : ScriptableObject
 {
     [MenuItem("Assets/Unhide Fix")]
     private static void unhide()
     {
         UnityEditor.Animations.AnimatorController ac = Selection.activeObject as UnityEditor.Animations.AnimatorController;
 
         foreach (UnityEditor.Animations.AnimatorControllerLayer layer in ac.layers)
         {
 
             foreach (UnityEditor.Animations.ChildAnimatorState curState in layer.stateMachine.states)
             {
                 if (curState.state.hideFlags != 0) curState.state.hideFlags = (HideFlags)1;
                 if (curState.state.motion != null)
                 {
                     if (curState.state.motion.hideFlags != 0) curState.state.motion.hideFlags = (HideFlags)1;
                 }
             }
 
             foreach (UnityEditor.Animations.ChildAnimatorStateMachine curStateMachine in layer.stateMachine.stateMachines)
             {
                 foreach (UnityEditor.Animations.ChildAnimatorState curState in curStateMachine.stateMachine.states)
                 {
                     if (curState.state.hideFlags != 0) curState.state.hideFlags = (HideFlags)1;
                     if (curState.state.motion != null)
                     {
                         if (curState.state.motion.hideFlags != 0) curState.state.motion.hideFlags = (HideFlags)1;
                     }
                 }
             }
         }
         EditorUtility.SetDirty(ac);
     }
}
#endif