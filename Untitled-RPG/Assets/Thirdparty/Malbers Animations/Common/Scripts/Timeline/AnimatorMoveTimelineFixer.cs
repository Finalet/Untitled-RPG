using UnityEngine;
namespace MalbersAnimations.Controller
{
    /// <summary> This is used for all the components that use OnAnimator Move... it breaks the Timeline edition  </summary>
    [AddComponentMenu("Malbers/Timeline/Animator Move Timeline Fixer")]

    [ExecuteInEditMode]
    public class AnimatorMoveTimelineFixer : MonoBehaviour
    {
        public Animator anim;
        void Start()
        {
            if (Application.isEditor && Application.isPlaying) Destroy(this);
            anim = GetComponent<Animator>();
        }

        private void OnAnimatorMove() 
        {
            if (anim != null) 
                anim.ApplyBuiltinRootMotion(); 
        }

        private void Reset()
        { anim = GetComponent<Animator>(); }
    }
}