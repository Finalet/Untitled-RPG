using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Effect Manager")]

    public class EffectManager : MonoBehaviour, IAnimatorListener
    {
        [RequiredField, Tooltip("Root Gameobject of the Hierarchy")]
        public Transform Owner;

        public List<Effect> Effects;

        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void PlayEffect(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID && effect.active == true);

            if (effects != null)
                foreach (var effect in effects) Play(effect);
        }

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void StopEffect(int ID) => Effect_Stop(ID);

        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void Effect_Play(int ID) => PlayEffect(ID);

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void Effect_Stop(int ID)
        {
            var effects = Effects.FindAll(effect => effect.ID == ID && effect.active == true);

            if (effects != null)
            {
                foreach (var e in effects)
                {
                    e.Modifier?.StopEffect(e);              //Play Modifier when the effect play
                    e.OnStop.Invoke();
                }
            }
        }
        private IEnumerator IPlayEffect(Effect e)
        {

            if (e.delay > 0)
                yield return new WaitForSeconds(e.delay);

            yield return new WaitForEndOfFrame();           //Wait until the Animation Cyle has pass (LateUpdate)


            if (e.effect.IsPrefab())                        //If instantiate is active (meaning is a prefab)
            {
                e.Instance = Instantiate(e.effect);         //Instantiate!
                e.effect.gameObject.SetActive(false);
            }
            else
            {
                e.Instance = e.effect;                              //Use the effect as the gameobject
            }

            if (Owner == null) Owner = transform.root;
            if (e.Owner == null) e.Owner = Owner;  //Save in all effects that the owner of the effects is this transform


            if (e.Instance && e.root)
            {
                e.Instance.transform.position = e.root.position;


                e.Instance.gameObject.SetActive(true);
            }

            var trail = e.Instance.GetComponentInChildren<TrailRenderer>(); //UNITY BUG!!! WITH TRAIL RENDERERS
            if (trail) trail.Clear();

            e.Instance.transform.localScale = Vector3.Scale(e.Instance.transform.localScale, e.ScaleMultiplier); //Scale the Effect

            e.OnPlay.Invoke();                                      //Invoke the Play Event

            if (e.root)
            {
                if (e.isChild)
                {
                    e.Instance.transform.parent = e.root;

                    e.Instance.transform.localPosition += e.PositionOffset;
                    e.Instance.transform.localRotation *= Quaternion.Euler(e.RotationOffset);
                }
                else
                {
                    e.Instance.transform.position = e.root.TransformPoint(e.PositionOffset);
                    // e.Instance.transform.rotation  = Quaternion.Euler(e.RotationOffset) * e.root.rotation;
                }

                if (e.useRootRotation) e.Instance.transform.rotation = e.root.rotation;     //Orient to the root rotation

            }

            //Apply Offsets


            if (e.Modifier) e.Modifier.StartEffect(e);              //Apply  Modifier when the effect play

            StartCoroutine(Life(e));

            yield return null;
        }

        private IEnumerator Life(Effect e)
        {
            if (e.life > 0)
            {
                yield return new WaitForSeconds(e.life);

                e.Modifier?.StopEffect(e);              //Play Modifier when the effect play
                e.OnStop.Invoke();


                if (e.effect.IsPrefab()) Destroy(e.Instance);       //Means the effect is a Prefab destroy the Instance

            }

            yield return null;
        }

        protected virtual void Play(Effect effect)
        {
            if (effect.effect == null) return;  //There's no effect available

            if (effect.Modifier) effect.Modifier.AwakeEffect(effect);        //Execute the Method PreStart Effect if it has a modifier
            StartCoroutine(IPlayEffect(effect));
        }


        /// <summary>IAnimatorListener function </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value)
        { return this.InvokeWithParams(message, value); }

        //─────────────────────────────────CALLBACKS METHODS───────────────────────────────────────────────────────────────────

        /// <summary>Disables all effects using their name </summary>
        public virtual void Effect_Disable(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name.ToUpper() == name.ToUpper());

            if (effects != null)
            {
                foreach (var e in effects) e.active = false;
            }
            else
            {
                Debug.LogWarning("No effect with the name: " + name + " was found");
            }
        }

        /// <summary> Disables all effects using their ID</summary>
        public virtual void Effect_Disable(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID);

            if (effects != null)
            {
                foreach (var e in effects) e.active = false;
            }
            else
            {
                Debug.LogWarning("No effect with the ID: " + ID + " was found");
            }
        }

        /// <summary>Enable all effects using their name</summary>
        public virtual void Effect_Enable(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name.ToUpper() == name.ToUpper());

            if (effects != null)
            {
                foreach (var e in effects) e.active = true;
            }
            else
            {
                Debug.LogWarning("No effect with the name: " + name + " was found");
            }
        }


        /// <summary> Enable all effects using their ID</summary>
        public virtual void Effect_Enable(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID);

            if (effects != null)
            {
                foreach (var e in effects) e.active = true;
            }
            else
            {
                Debug.LogWarning("No effect with the ID: " + ID + " was found");
            }
        }

        private void Reset()
        {
            Owner = transform.root;
        }

#if UNITY_EDITOR
        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {
            MEventListener listener = gameObject.FindComponent<MEventListener>();

            if (listener == null) listener = gameObject.AddComponent<MEventListener>();
            if (listener.Events == null) listener.Events = new List<MEventItemListener>();

            MEvent effectEnable = MTools.GetInstance<MEvent>("Effect Enable");
            MEvent effectDisable = MTools.GetInstance<MEvent>("Effect Disable");

            if (listener.Events.Find(item => item.Event == effectEnable) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = effectEnable,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Effect_Enable);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, Effect_Enable);
                listener.Events.Add(item);

                Debug.Log("<B>Effect Enable</B> Added to the Event Listeners");
            }

            if (listener.Events.Find(item => item.Event == effectDisable) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = effectDisable,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Effect_Disable);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, Effect_Disable);
                listener.Events.Add(item);

                Debug.Log("<B>Effect Disable</B> Added to the Event Listeners");
            }

            UnityEditor.EditorUtility.SetDirty(listener);
        }
#endif


    }

    [System.Serializable]
    public class Effect
    {
        public string Name = "EffectName";
        public int ID;
        public bool active = true;
        public Transform root;

        public bool isChild;
        public bool useRootRotation = true;
        public GameObject effect;
        public Vector3 RotationOffset;
        public Vector3 PositionOffset;
        public Vector3 ScaleMultiplier = Vector3.one;

        /// <summary>Life of the Effect</summary>
        public float life = 10f;

        /// <summary>Delay Time to execute the effect after is called.</summary>
        public float delay;

        /// <summary>Scriptable Object to Modify anything you want before, during or after the effect is invoked</summary>
        public EffectModifier Modifier;


        public UnityEvent OnPlay;
        public UnityEvent OnStop;

        /// <summary>Returns the Owner of the Effect </summary>
#pragma warning disable CA2235 // Mark all non-serializable fields
        public Transform Owner { get; set; }
#pragma warning restore CA2235 // Mark all non-serializable fields


        /// <summary>Returns the Instance of the Effect Prefab </summary>
        public GameObject Instance { get => instance; set => instance = value; }

        [System.NonSerialized]
        private GameObject instance;
    }
}