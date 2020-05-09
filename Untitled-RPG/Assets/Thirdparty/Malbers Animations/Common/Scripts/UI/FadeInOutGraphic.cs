using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MalbersAnimations
{
    public class FadeInOutGraphic : MonoBehaviour
    {
        public Graphic graphic;
        public Color FadeIn;
        public Color FadeOut;
        public float time = 0.25f;

        public virtual void Fade_In_Out(GameObject fade)
        {
            Fade_In_Out(fade != null);
        }

        public virtual void Fade_In_Out(bool fade)
        {
            graphic.CrossFadeColor(fade ? FadeIn : FadeOut, time, false, true);
        }

        public virtual void Fade_In(float time)
        {
            graphic.CrossFadeColor(FadeIn, time, false, true);
        }

        public virtual void Fade_Out(float time)
        {
            graphic.CrossFadeColor(FadeOut, time, false, true);
        }
    }
}
