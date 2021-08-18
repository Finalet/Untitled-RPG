using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using UnityEngine.Events;
using MalbersAnimations.Scriptables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/Melee Weapon")]
    public class MMelee : MWeapon 
    {
        [RequiredField] public Collider meleeTrigger;
        protected List<Collider> AlreadyHitted = new List<Collider>();
        public BoolEvent OnCauseDamage = new BoolEvent();
        public Color DebugColor = new Color(1, 0.25f, 0, 0.5f);

        public bool UseCameraSide;
        public bool InvertCameraSide;
        public int Attacks = 1;

        protected bool canCauseDamage;                      //The moment in the Animation the weapon can cause Damage 
        public bool CanCauseDamage
        {
            get =>  canCauseDamage; 
            set
            {
               // Debugging($"Can Cause Damage {value} ");

                canCauseDamage = value;
                enemy = null;
                AlreadyHitted = new List<Collider>();  //Reset the list of transform that I already Hit

                trigerProxy.Active = value;
                meleeTrigger.enabled = value;         //Enable/Disable the Trigger
            }
        }

        protected TriggerProxy trigerProxy;

        /// <summary>Enemy that can be Damaged</summary>
        private IMDamage enemy;


        /// <summary>Damager from the Attack Triger Behaviour</summary>
        public override void ActivateDamager(int value)
        {
            if (value == 0)
            {
                CanCauseDamage = false;
                OnCauseDamage.Invoke(CanCauseDamage);
            }
            else if (value == -1 || value == Index)
            {
                CanCauseDamage = true;
                OnCauseDamage.Invoke(CanCauseDamage);
            }
        }

        public override void DoDamage(bool value)
        {
            ActivateDamager(value ? -1 : 0);
        }

        private void Awake()
        {
            Initialize();   
            FindMeleeTrigger();
        }

        void OnEnable()
        {
            trigerProxy.OnTrigger_Enter.AddListener(AttackTriggerEnter);
            trigerProxy.OnTrigger_Exit.AddListener(AttackTriggerExit);
            CanCauseDamage = false;
            Find_Owner();
        }

        /// <summary>Disable Listeners </summary>
        void OnDisable()
        {
            trigerProxy.OnTrigger_Enter.RemoveListener(AttackTriggerEnter);
            trigerProxy.OnTrigger_Exit.RemoveListener(AttackTriggerExit);
        }


        #region Main Attack 
        internal override void MainAttack_Start(IMWeaponOwner RC)
        {
            base.MainAttack_Start(RC);
            if (CanAttack)
            {
                ResetCharge();
                var Side = UseCameraSide && (InvertCameraSide ? RC.AimingSide : !RC.AimingSide);
                RiderMeleeAttack(Side);
            }
        }


        internal override void SecondaryAttack_Start(IMWeaponOwner RC)
        {
            MainInput = false;
            SecondInput = true;
            if (CanAttack)
            {
                DoDamage(false);
                ResetCharge();
                if (!UseCameraSide) RiderMeleeAttack(true);                 //Attack with Left Hand
            }
        }



        /// <summary>Set all parameters for Melee Attack </summary>
        /// <param name="rightSide">true = Right Side of the ount.. false = Left Side of the ount</param>
        protected virtual void RiderMeleeAttack(bool rightSide)
        {
            int attackID = Random.Range(1, Attacks + 1) * (rightSide ? 1 : -1);           // Set the Attacks for the RIGHT Side with the 'Right Hand' 1 2
            WeaponAction?.Invoke(attackID); //Convert into Left attack if the weapon is Left Handed
            CanAttack = false;
        }
        #endregion


        public override bool PrepareWeapon(IMWeaponOwner _char)
        {
            trigerProxy?.SetLayer(Layer, TriggerInteraction);
            return base.PrepareWeapon(_char);
        }

        void AttackTriggerEnter(Collider other)
        {
           // Debugging($"Trigger Enter - {other.name} ");

            if (IsInvalid(other)) return;                                               //Check Layers and Don't hit yourself
            if (other.transform.root == IgnoreTransform) return;                        //Check an Extra transform that you cannot hit...e.g your mount

            var Newenemy = other.GetComponentInParent<IMDamage>();                      //Get the Animal on the Other collider

            if (!AlreadyHitted.Find(col => col == other))                               //if the entering collider is not already on the list add it
                AlreadyHitted.Add(other);

            Direction = (Owner.transform.position - other.bounds.center).normalized;              //Calculate the direction of the attack

            TryInteract(other.gameObject);                                              //Get the interactable on the Other collider
            TryPhysics(other.attachedRigidbody, other, Direction, Mathf.Lerp(MinForce, MaxForce, ChargedNormalized));               //If the other has a riggid body and it can be pushed

            if (enemy == Newenemy)
            {
                return;                                              //if the animal is the same, do nothing we already in one of the Animal Colliders
            }
            else                                                                        //Is a new Animal
            {
                if (enemy != null) AlreadyHitted = new List<Collider>();                               //Clean the colliders if you had a previus animal

                enemy = Newenemy;                                                       //Get the Damager on the Other collider

                ///CALCULATE THE DAMAGE
                var Damage = new StatModifier(statModifier)
                { Value = Mathf.Lerp(MinDamage, MaxDamage, ChargedNormalized) }; //Do the Damage depending the charge

                OnHit.Invoke(other.transform);
                TryDamage(enemy, Damage); //if the other does'nt have the Damagable Interface dont send the Damagable stuff
            }
        }


        void AttackTriggerExit(Collider other)
        {
            if (IsInvalid(other)) return;
            if (other.transform.root == IgnoreTransform) return;                        //Check an Extra transform that you cannot hit...e.g your mount
            if (enemy != other.GetComponentInParent<IMDamage>()) return;                //If is another animal exiting the trigger SKIP

            if (AlreadyHitted.Find(col => col == other))                                //Remove the collider from the list that is exiting the zone.
                AlreadyHitted.Remove(other);

            if (AlreadyHitted.Count == 0) enemy = null;                           //When all the collides are removed from the list..
        }



       
 
        public override void ResetWeapon()
        {
            meleeTrigger.enabled = false; 
            base.ResetWeapon();
        }

        private void FindMeleeTrigger()
        {
            if (!meleeTrigger)
            {
                meleeTrigger = GetComponent<Collider>();
                meleeTrigger.isTrigger = true;
            }

            if (meleeTrigger)
            {
                trigerProxy = meleeTrigger.GetComponent<TriggerProxy>();

                if (trigerProxy == null)
                    trigerProxy = meleeTrigger.gameObject.AddComponent<TriggerProxy>();            //Create a proxy to comunicate the Collision and trigger events in case the melee conllider is on another gameObject

                trigerProxy.SetLayer(Layer, TriggerInteraction);

              
                meleeTrigger.enabled = false;
                trigerProxy.Active = false;
            }

           
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (meleeTrigger)
            {
                MTools.DrawTriggers(meleeTrigger.transform, meleeTrigger, DebugColor);
            }
        }
#endif
    }


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(MMelee))]
    public class MMeleeEditor : MWeaponEditor
    {
        SerializedProperty meleeCollider, OnCauseDamage, UseCameraSide, InvertCameraSide, Attacks;

        void OnEnable()
        {
            WeaponTab = "Melee";
            SetOnEnable();
            meleeCollider = serializedObject.FindProperty("meleeTrigger");
            OnCauseDamage = serializedObject.FindProperty("OnCauseDamage");
            InvertCameraSide = serializedObject.FindProperty("InvertCameraSide");
            UseCameraSide = serializedObject.FindProperty("UseCameraSide");
            Attacks = serializedObject.FindProperty("Attacks");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDescription("Melee Weapon Properties");

            WeaponInspector();
           
            serializedObject.ApplyModifiedProperties();
        }

        protected override void UpdateSoundHelp()
        {
            SoundHelp = "0:Draw   1:Store   2:Swing   3:Hit \n (Leave 3 Empty, add SoundByMaterial and Invoke 'PlayMaterialSound' for custom Hit sounds)";
        }

        protected override void ChildWeaponEvents()
        {
            EditorGUILayout.PropertyField(OnCauseDamage, new GUIContent("On AttackTrigger Active"));
        }

        protected override void DrawAdvancedWeapon()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(meleeCollider, new GUIContent("Melee Trigger", "Gets the reference of where is the Melee Collider of this weapon (Not Always is in the same gameobject level)"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Riding Behaviour", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(Attacks, new GUIContent("Attacks", "Amount of Attack Animations can do on both sides while mounting"));
            EditorGUILayout.PropertyField(UseCameraSide, new GUIContent("Use Camera Side", "The Attacks are Activated by the Main Attack and It uses the Side of the Camera to Attack on the Right or the Left side of the Mount"));
            
            if (UseCameraSide.boolValue)
            EditorGUILayout.PropertyField(InvertCameraSide, new GUIContent("Invert Camera Side", "Inverts the camera side value"));
            EditorGUILayout.EndVertical();
        }
    }
#endif
}