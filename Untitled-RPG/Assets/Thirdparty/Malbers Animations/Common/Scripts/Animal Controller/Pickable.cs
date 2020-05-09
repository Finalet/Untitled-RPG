using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine.Events;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    public class Pickable : MonoBehaviour
    {
        /// <summary>Use Pick Animations</summary>
        public bool PickAnimations = true;
        public ModeID PickUpMode;
        public int PickUpAbility = 1;

        public bool Align = true;
        public float AlignTime = 0.15f;
        public float AlignDistance = 1f;

        public bool DropAnimations = true;
        public ModeID DropMode;
        public int DropAbility = 2;


        public FloatReference m_Value = new FloatReference(1f); //Not done yet
        public IntReference m_ID = new IntReference();         //Not done yet


        public BoolEvent OnFocused = new BoolEvent();
        public UnityEvent OnPicked = new UnityEvent();
        public UnityEvent OnDropped = new UnityEvent();

        private Rigidbody rb;
        public Collider m_collider;


        /// <summary>Is this Object being picked </summary>
        public bool IsPicked { get; set; }
        public float Value { get => m_Value.Value; set => m_Value.Value = value; }
        public int ID { get => m_ID.Value; set => m_ID.Value = value; }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }


        public virtual void Picked()
        {
            if (rb)
            {
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.isKinematic = true;
            }

            m_collider.enabled = false;
            IsPicked = true;

            OnPicked.Invoke();
        }

        public virtual void Dropped()
        {
            IsPicked = false;

            if (rb)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            m_collider.enabled = true;
            OnDropped.Invoke();
        }

        [HideInInspector] public bool ShowEvents = true;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, AlignDistance);
        }

        private void Reset()
        {
            m_collider = GetComponent<Collider>();
            PickUpMode = MalbersTools.GetInstance<ModeID>("PickUp");
            DropMode = PickUpMode;
        }
#endif
    }
}