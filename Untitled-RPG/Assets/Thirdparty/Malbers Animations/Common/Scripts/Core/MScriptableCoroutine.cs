using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MalbersAnimations
{
    public class MScriptableCoroutine : MonoBehaviour
    {
        internal     List<ScriptableCoroutine> ScriptableCoroutines;
        public static MScriptableCoroutine Main;
        

        internal void Reset()
        {
            if (Main == null) //if there's a main animal already seted
            {
                Main = this;
                ScriptableCoroutines = new List<ScriptableCoroutine>();
            }
        }


        public static void PlayCoroutine(ScriptableCoroutine SC, IEnumerator Coroutine)
        {
            Initialize(); //In case is not initialize

            if (!Main.ScriptableCoroutines.Contains(SC))
            {
                Main.ScriptableCoroutines.Add(SC); //Add the Fist Time
            }

            Main.StartCoroutine(Coroutine);
        }

        public static void Stop_Coroutine(IEnumerator Coroutine)
        {
            Main.StopCoroutine(Coroutine);
        }

        public static void Initialize()
        {
            if (Main == null)
            {
                var ScriptCoro = new GameObject();
                ScriptCoro.name = "Scriptable Coroutines";
                var MonoCoro = ScriptCoro.AddComponent<MScriptableCoroutine>();
                MonoCoro.Reset();
            }
        }

        protected virtual void OnDisable()
        {
            if (ScriptableCoroutines != null)
            foreach (var c in ScriptableCoroutines)
                c.CleanCoroutine();

            StopAllCoroutines();
        }
    }
}