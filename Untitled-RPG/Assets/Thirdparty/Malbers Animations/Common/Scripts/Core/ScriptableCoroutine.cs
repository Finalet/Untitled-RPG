using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MalbersAnimations
{
    public abstract class ScriptableCoroutine : ScriptableObject
    {
        internal Dictionary<Component, IEnumerator> Coroutine;

        //[Tooltip("Interrupt any Coroutine Playing")]
        //public bool Interrupt = false;



        internal void StartCoroutine(Component component, IEnumerator ICoroutine)
        {
            if (Coroutine == null) Coroutine = new Dictionary<Component, IEnumerator>();

            //if (Interrupt && Coroutine.ContainsKey(component))
            //{
            //    ExitValue(component);
            //    Stop(component);
            //}

            if (!Coroutine.ContainsKey(component)) //Play the coroutine in case the component is tryin to do the coroutine is not playing any other coroutine
            {
                Coroutine.Add(component, ICoroutine);
                MScriptableCoroutine.PlayCoroutine(this, ICoroutine);
               // Debug.Log($"Coroutine Started {component.name}");
            }
        }


        /// <summary> Stop the coroutine in case it exist</summary>
        internal virtual void Stop(Component component)
        {
            if (Coroutine == null) return;

            if (Coroutine.ContainsKey(component))
            {
                var CurrentCoro = Coroutine[component];

                if (CurrentCoro != null)
                    MScriptableCoroutine.Stop_Coroutine(CurrentCoro);

                Coroutine.Remove(component); //Remove the Coroutine since its already Finished
               // Debug.Log($"Coroutine Stopped {component.name}");
            }
        }

        internal virtual void CleanCoroutine()
        {
            if (Coroutine != null)
                foreach (var c in Coroutine)
                    ExitValue(c.Key);

            Coroutine = null;
        }

        internal virtual void ExitValue(Component compoennt) { }
    }
}