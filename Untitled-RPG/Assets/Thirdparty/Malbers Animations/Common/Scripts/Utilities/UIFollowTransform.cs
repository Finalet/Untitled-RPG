using UnityEngine;
using UnityEngine.UI;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;

namespace MalbersAnimations
{
    /// <summary>makes an UI Object to follow a World Object</summary>
    public class UIFollowTransform : MonoBehaviour
    {
      [SerializeField]  private Camera MainCamera;
        public Transform WorldTransform;
        public bool Lerp;
        public bool StartOff;
        public float Smoothness = 20f;
        public Color FadeOut;
        public Color FadeIn = Color.white;
        public float time = 0.3f;
        Graphic graphic;

        public Vector3 ScreenCenter { get; set; }

        Graphic Graph
        {
            get
            {
                if (graphic == null)
                    graphic = GetComponent<Graphic>();
                return graphic;
            }
        }

        private void OnEnable()
        {
            MainCamera = MalbersTools.FindMainCamera();
            Align();
        }

        public void SetTransform(Transform newTarget)
        {
            WorldTransform = newTarget;
            Align();
        }
        public void SetScreenCenter(Vector3 newScreenCenter)
        {
            ScreenCenter = newScreenCenter;
            Align();
        }

        private void Start()
        {
            if (StartOff && graphic)
            {
                graphic.CrossFadeColor(FadeOut, 0, false, true);
            }

            Align();
        }

        void Awake()
        {
            MainCamera = MalbersTools.FindMainCamera();
          
            graphic = GetComponent<Graphic>();

            ScreenCenter = transform.position;
        }

        void FixedUpdate()
        {
            if (Lerp)
                AlingLerp();
            else
                Align();
        }

        public void Align()
        {
            if (MainCamera == null) return;
            transform.position = WorldTransform != null ? MainCamera.WorldToScreenPoint(WorldTransform.position) : ScreenCenter;
        }


        public void AlingLerp()
        {
            if (Lerp)
            {
                Vector3 UIPos = WorldTransform != null ? MainCamera.WorldToScreenPoint(WorldTransform.position) : ScreenCenter;
                transform.position = Vector3.Slerp(transform.position, UIPos, Time.deltaTime * Smoothness);
            }
        }


        public virtual void Fade_In_Out(bool value)
        {
            Graph.CrossFadeColor(value ? FadeIn : FadeOut, time, false, true);
        }


        public virtual void Fade_In(float time)
        {
            graphic.CrossFadeColor(FadeIn, time, false, true);
        }

        public virtual void Fade_Out(float time)
        {
            graphic.CrossFadeColor(FadeOut, time, false, true);
        }

#if UNITY_EDITOR

        void Reset()
        {
            MEventListener MeventL = GetComponent<MEventListener>();

            if (MeventL == null)
            {
                MeventL = gameObject.AddComponent<MEventListener>();
            }

            MeventL.Events = new System.Collections.Generic.List<MEventItemListener>(1) { new MEventItemListener() };

            var listener = MeventL.Events[0];

            listener.useTransform = true;
            listener.useVector3 = true;
            listener.useVoid = false;

            listener.Event = MalbersTools.GetInstance<MEvent>("Follow UI Transform");

            if (listener.Event != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(listener.ResponseTransform, SetTransform);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(listener.ResponseVector3, SetScreenCenter);
            }

        }
#endif
    }
}