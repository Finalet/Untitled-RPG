using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class ModeBehaviour : StateMachineBehaviour
    {
        public ModeID ModeID;
        private MAnimal animal;
        private Mode modeOwner;

        [Tooltip("Calls 'Animation Tag Enter' on the Modes")]  public bool EnterMode = true;
        [Tooltip("Calls 'Animation Tag Exit' on the Modes")] public bool ExitMode = true;
       

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animal = animator.GetComponent<MAnimal>();

            if (animal.IntID == Int_ID.Loop) return; //Means is Looping

            if (ModeID == null) Debug.LogError("Mode behaviour needs an ID");

            modeOwner = animal.Mode_Get(ModeID);

            if (EnterMode)
                modeOwner?.AnimationTagEnter(stateInfo.tagHash);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animal.IntID == Int_ID.Loop) return;                //Means is Looping So Skip the Exit Mode

            if (ExitMode)
                modeOwner?.AnimationTagExit();
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (modeOwner != null)
                modeOwner.OnModeStateMove(stateInfo,animator,layerIndex);
        }
    }
}