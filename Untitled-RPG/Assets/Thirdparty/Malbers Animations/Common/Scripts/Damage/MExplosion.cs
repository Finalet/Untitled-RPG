using MalbersAnimations.Controller;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [DefaultExecutionOrder(1000)]
    /// <summary> Explosion Logic</summary>
    [AddComponentMenu("Malbers/Damage/Explosion")]

    public class MExplosion : MDamager
    {
        [Tooltip("The Explosion will happen on Start ")]
        public bool ExplodeOnStart;
        [Tooltip("Value needed for the AddExplosionForce method default = 0 ")]
        public float upwardsModifier = 0;
        [Tooltip("Radius of the Explosion")]
        public float radius = 10;
        [Tooltip("Life of the explosion, after ")]
        public float life = 10f;

        void Start() { if (ExplodeOnStart) Explode(); }



        public virtual void Explode()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius, Layer, triggerInteraction);             //Ignore Colliders

            foreach (var nearbyObj in colliders)
            {
                if (dontHitOwner && Owner && nearbyObj.transform.IsChildOf(Owner.transform)) continue;                              //Don't hit yourself

                nearbyObj.attachedRigidbody?.AddExplosionForce(Force, transform.position, radius, upwardsModifier, forceMode);

                var Distance = Vector3.Distance(transform.position, nearbyObj.transform.position);                              //Distance of the collider and the Explosion

                if (statModifier.ID != null)
                {
                    var modif = new StatModifier(statModifier)
                    {
                        Value = statModifier.Value * (1 - (Distance / radius))                                                   //Do Damage depending the distance from the explosion
                    };

                    TryDamage(nearbyObj.gameObject, modif);
                    TryInteract(nearbyObj.gameObject);

                    modif.ModifyStat(nearbyObj.GetComponentInParent<Stats>());                              //Use the Damageable comonent instead!!!!!!!!!!!!!!!!!!!!!!!!!!!
                }
            }
            Destroy(gameObject, life);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = (Color.red);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
   }

#if UNITY_EDITOR
    [CustomEditor(typeof(MExplosion))]
    [CanEditMultipleObjects]
    public class MExposionEd : MDamagerEd
    {
        SerializedProperty ExplodeOnStart, upwardsModifier, radius, life;
         

        private void OnEnable()
        {
            FindBaseProperties();

            ExplodeOnStart = serializedObject.FindProperty("ExplodeOnStart");

            upwardsModifier = serializedObject.FindProperty("upwardsModifier");

            radius = serializedObject.FindProperty("radius");
            life = serializedObject.FindProperty("life");
        } 

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDescription("Explosion Damager");
            DrawScript();

            DrawGeneral();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Explosion", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(ExplodeOnStart, new GUIContent("On Start"));
                EditorGUILayout.PropertyField(radius);
                EditorGUILayout.PropertyField(life);
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                DrawPhysics(false);
                EditorGUILayout.PropertyField(upwardsModifier);
            }
            EditorGUILayout.EndVertical();

            DrawCriticalDamage();

            DrawStatModifier();

            DrawMisc();

            DrawEvents();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}