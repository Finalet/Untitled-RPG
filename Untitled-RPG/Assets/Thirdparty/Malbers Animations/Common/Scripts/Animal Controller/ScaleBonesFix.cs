using UnityEngine;
using System.Collections;
using MalbersAnimations.Controller;

namespace MalbersAnimations
{
    /// <summary>
    /// This class is used when the foot bones are scaled and when seat or lie or death do not match with the original animations
    /// it uses messages from the Animators Behaviors States.
    /// it uses messages from the Animators Behaviors States.
    /// </summary>
    [AddComponentMenu("Malbers/Animal Controller/Scale Bones Fix")]
    public class ScaleBonesFix : MonoBehaviour, IAnimatorListener
    {
        private MAnimal animal;
        public float Offset = -0.2f;
        public float duration = 0.2f;

        private void Awake() => animal = this.FindComponent<MAnimal>();

        public void FixHeight(bool active) => StartCoroutine(SmoothFix(active));

        public IEnumerator SmoothFix(bool active)
        {
            float t = 0f;
            float startpos = animal.height;
            float endpos = startpos + (active ? Offset : -Offset);

            while (t < duration)
            {
                animal.height = Mathf.Lerp(startpos, endpos, t / duration);
                t += Time.deltaTime;
                yield return null;
            }
            animal.height = endpos;
            yield return null;
        }

        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);
    }
}