using System.Collections;
using UnityEngine;

namespace MalbersAnimations
{
  //  public enum AnimCycle { None, Loop, Repeat, PingPong }

    public class PlayTransformAnimation : MonoBehaviour
    {
        [CreateScriptableAsset]
        public TransformAnimation anim;
        public Transform m_transform;

        public bool PlayOnStart = true;
        public bool PlayForever;

        internal IEnumerator ICoroutine;

        [SerializeField]
        [ContextMenuItem("Store starting value", "StoreDefault")]
        TransformOffset DefaultValue;

        private void Reset()
        {
            m_transform = transform;
            DefaultValue = new TransformOffset(m_transform);
        }

        [ContextMenu("Store starting value")]
        void StoreDefault()
        {
            DefaultValue = new TransformOffset(m_transform);
        }

        private void Start()
        {
            if (PlayOnStart) Play();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            anim.CleanCoroutine();
        }

        public void Play()
        {
            if (!isActiveAndEnabled) return;

            if (ICoroutine != null) //Means that the Animal is already playing an animation 
            {
                DefaultValue.RestoreTransform(m_transform);
                StopCoroutine(ICoroutine);
            }

            if (PlayForever)
            {
                ICoroutine = anim.PlayTransformAnimationForever(m_transform);
                
            }
            else
            {
                ICoroutine = anim.PlayTransformAnimation(m_transform);
            }
            StartCoroutine(ICoroutine);
        }
    }
}