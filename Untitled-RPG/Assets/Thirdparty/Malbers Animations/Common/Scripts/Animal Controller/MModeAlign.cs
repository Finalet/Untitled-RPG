using MalbersAnimations.Controller;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Animal Controller/Mode Align")]
    public class MModeAlign : MonoBehaviour
    {
        [RequiredField] public MAnimal animal;
        [Tooltip("Which mode should listen this script, so it can start Aligning when the mode start")]
        public ModeID ModeID;

        public List<ModeID> extraModes;


        [Tooltip("Search only for Animals on the Radius. If is set to false then it will search for all colliders using the Layer Mask: Hit Layer from this Animal")]
        public bool AnimalsOnly = true;
        public LayerReference Layer = new LayerReference(-1);
        [Tooltip("Radius used for the Search")]
        public float LookRadius = 2f;
        [Tooltip("Radius used push closer/farther the Target when playing the Mode")]
        public float DistanceRadius = 0;
        [Tooltip("Time needed to complete the Position aligment")]
        public float AlignTime = 0.3f;
        [Tooltip("Time needed to complete the Rotation aligment")]
        public float LookAtTime = 0.15f;
        public Color debugColor = new Color(1, 0.5f, 0, 0.2f);

        void Awake()
        { if (animal == null)  animal = this.FindComponent<MAnimal>(); }

        void OnEnable()
        { animal.OnModeStart.AddListener(FindMode); }

        void OnDisable()
        { animal.OnModeStart.RemoveListener(FindMode); }

        void FindMode(int ModeID, int ability)
        {
            if (!isActiveAndEnabled) return;

            if (this.ModeID.ID == ModeID || (extraModes?.Find(x => x.ID == ModeID) != null))
            {
                if (AnimalsOnly)
                    AlignAnimalsOnly();
                else
                    Align();
            }
        }

        private void AlignAnimalsOnly()
        {
            MAnimal ClosestAnimal = null;
            float ClosestDistance = float.MaxValue;

            foreach (var a in MAnimal.Animals)
            {
                if (a == animal || a.ActiveStateID.ID == StateEnum.Death || a.Sleep) continue; //Don't Find yourself or don't find death animals

                var animalsDistance = Vector3.Distance(transform.position, a.Center);

                if (LookRadius >= animalsDistance && ClosestDistance >= animalsDistance)
                {
                    ClosestDistance = animalsDistance;
                    ClosestAnimal = a;
                }
            }

            if (ClosestAnimal)
                StartAligning(ClosestAnimal.Center);
        }

        private void Align()
        {
            var pos = animal.Center;

            var AllColliders = Physics.OverlapSphere(pos, LookRadius,  Layer.Value);

            Collider ClosestCollider = null;
            float ClosestDistance = float.MaxValue;

            foreach (var col in AllColliders)
            {
                if (col.transform.root == animal.transform.root) continue; //Don't Find yourself

                var DistCol = Vector3.Distance(transform.position, col.bounds.center);

                if (ClosestDistance > DistCol)
                {
                    ClosestDistance = DistCol;
                    ClosestCollider = col;
                }
            }
            if (ClosestCollider) StartAligning(ClosestCollider.bounds.center);
        }

        private void StartAligning(Vector3 TargetCenter)
        {
            TargetCenter.y = animal.transform.position.y;
            Debug.DrawLine(transform.position, TargetCenter, Color.red, 3f);
            StartCoroutine(MTools.AlignLookAtTransform(animal.transform, TargetCenter, LookAtTime));
            if (DistanceRadius > 0) StartCoroutine(MTools.AlignTransformRadius(animal.transform, TargetCenter, AlignTime, DistanceRadius*animal.ScaleFactor));  //Align Look At the Zone
        }

#if UNITY_EDITOR
        void Reset()
        {
            ModeID modeID = MTools.GetInstance<ModeID>("Attack1");
            animal = GetComponent<MAnimal>();

            if (modeID != null)
            {
                this.ModeID = modeID;
            }
        }


        void OnDrawGizmosSelected()
        {
            if (animal)
            {
                UnityEditor.Handles.color = debugColor;
                UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, LookRadius);
                var c = debugColor; c.a = 1;
                UnityEditor.Handles.color =c;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, LookRadius);

                UnityEditor.Handles.color = (c + Color.white)/2;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, DistanceRadius);

            }
        }
#endif
    }
}