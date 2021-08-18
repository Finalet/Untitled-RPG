using UnityEngine;
namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Animator/Blink Eyes")]

    public class BlinkEyes : MonoBehaviour, IAnimatorListener
    {
        [RequiredField]  public Animator animator;
        public string parameter = "Eyes";

        /// <summary>This method is called by animation clips events, this will open an close the animal eyes</summary>
        public virtual void Eyes(int ID) => animator.SetInteger(parameter, ID);

        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);
    }
}
