using UnityEditor;
using UnityEngine;

public class AnimUnhideTool
{
    [MenuItem("Tools/Malbers Animations/Animator States Unhide Fix")]
    private static void Fix()
    {
        if (Selection.objects.Length < 1)
            throw new UnityException("Select animator controller(s) before try fix it!");

        int scnt = 0;

        foreach (Object o in Selection.objects)
        {
            UnityEditor.Animations.AnimatorController ac = o as UnityEditor.Animations.AnimatorController;
            if (ac == null)
                continue;

            foreach (UnityEditor.Animations.AnimatorControllerLayer layer in ac.layers)
            {

                foreach (UnityEditor.Animations.ChildAnimatorState curState in layer.stateMachine.states)
                {
                    scnt = FixState(scnt, curState);
                }

                scnt = FixStateMachines(scnt, layer.stateMachine.stateMachines);

            }
            EditorUtility.SetDirty(ac);
        }
        Debug.Log("Fixing " + scnt + " states done!");
    }

    private static int FixStateMachines(int scnt, UnityEditor.Animations.ChildAnimatorStateMachine[] stateMachines)
    {
        foreach (UnityEditor.Animations.ChildAnimatorStateMachine curStateMachine in stateMachines)
        {
            if (curStateMachine.stateMachine.hideFlags != (HideFlags)1)
            {
                curStateMachine.stateMachine.hideFlags = (HideFlags)1;
            }

            if (curStateMachine.stateMachine.stateMachines != null)
            {
                scnt = FixStateMachines(scnt, curStateMachine.stateMachine.stateMachines);
            }

            if (curStateMachine.stateMachine.entryTransitions != null)
            {
                foreach (UnityEditor.Animations.AnimatorTransition curTrans in curStateMachine.stateMachine.entryTransitions)
                {
                    if (curTrans.hideFlags != (HideFlags)1)
                    {
                        curTrans.hideFlags = (HideFlags)1;
                    }
                }
            }

            foreach (UnityEditor.Animations.ChildAnimatorState curState in curStateMachine.stateMachine.states)
            {
                scnt = FixState(scnt, curState);
            }
        }

        return scnt;
    }

    private static int FixState(int scnt, UnityEditor.Animations.ChildAnimatorState curState)
    {
        if (curState.state.hideFlags != (HideFlags)1)
        {
            curState.state.hideFlags = (HideFlags)1;
            scnt++;
        }
        if (curState.state.motion != null)
        {
            if (curState.state.motion.hideFlags != (HideFlags)1)
                curState.state.motion.hideFlags = (HideFlags)1;
        }
        if (curState.state.transitions != null)
        {
            foreach (UnityEditor.Animations.AnimatorStateTransition curTrans in curState.state.transitions)
            {
                if (curTrans.hideFlags != (HideFlags)1)
                {
                    curTrans.hideFlags = (HideFlags)1;
                }
            }
        }
        if (curState.state.behaviours != null)
        {
            foreach (StateMachineBehaviour behaviour in curState.state.behaviours)
            {
                if (behaviour.hideFlags != (HideFlags)1)
                    behaviour.hideFlags = (HideFlags)1;
            }
        }
        return scnt;
    }
}