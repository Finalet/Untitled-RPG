using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using MalbersAnimations.Events;
using UnityEngine.Serialization;


namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/MBow")]
    public class MBow : MWeapon, IBow
    {
        public Transform knot;                                  //Point of the bow to put the arrow (End)
        public Transform arrowPoint;                            //Point of the bow to put the arrow (Front)

        public Transform[] UpperBn;                         //Upper Chain of the bow
        public Transform[] LowerBn;                         //Upper Chain of the bow

        public GameObject arrow;                                //Arrow to Release Prefab
        protected GameObject arrowInstance;                      //Current arrow in Load Instance


        /// <summary>Max Bending the Bow can do</summary>
        public float MaxTension;
        ///// <summary>Max Bending the Bow can do normalized</summary>
        //[SerializeField] private float tensionLimit = 1;

        /// <summary>Time to Tense the Bow</summary>
        public float holdTime = 2f;

        /// <summary> Normalized Bow Tension </summary>
        [Range(0, 1)] public float BowTension;
       
        /// <summary> Default Rotation of the Upper Bow Bones </summary>
        private Quaternion[] UpperBnInitRotation;
        /// <summary> Default Rotation of the Lower Bow Bones </summary>
        private Quaternion[] LowerBnInitRotation;

        /// <summary> Default Position of the Knot </summary>
        [FormerlySerializedAs("InitPosKnot")]
        public Vector3 DefaultPosKnot;

        public Vector3 RotUpperDir = -Vector3.forward;
        public Vector3 RotLowerDir = Vector3.forward;

        #region Events
        public GameObjectEvent OnLoadArrow;
        public FloatEvent OnHold;
        public GameObjectEvent OnReleaseArrow;
        #endregion

        ///<summary> Does not shoot arrows when is false, useful for other controllers like Invector and ootii to let them shoot the arrow themselves </summary>
        [Tooltip(" Does not shoot arrows when is false, useful for other controllers like Invector and ootii to let them shoot the arrow instead")]
        public bool releaseArrow = true;

        /// <summary> Is the Bow Bones Setted Correctly?... Is this bow functional?? </summary>
        public bool BowIsSet = false;

        #region Properties

        /// <summary>
        /// Every time this property  is called the Knot orients towards the ArrowPoint
        /// </summary>
        public Transform KNot
        {
            get
            {
                knot.rotation = Quaternion.LookRotation((arrowPoint.position - knot.position).normalized, -Physics.gravity);
                return knot;
            }
        }
        public Transform ArrowPoint
        {
            get { return arrowPoint; }
        }

        public float HoldTime
        {
            get { return holdTime; }
        }

        public GameObject Arrow
        {
            get { return arrow; }
            set { arrow = value; }
        }

        public GameObject ArrowInstance
        {
            get { return arrowInstance; }
            set { arrowInstance = value; }
        }

        //public float TensionLimit
        //{
        //    get { return tensionLimit; }
        //    set { tensionLimit = value; }
        //}
        #endregion

        void Start()
        {
            InitializeWeapon();
        }


        public override void InitializeWeapon()
        {
            base.InitializeWeapon();
            InitializeBow();
        }

        /// <summary>
        /// Set all the main Parameters to start using a bow;
        /// </summary>
        public virtual void InitializeBow()
        {
            if (UpperBn == null || LowerBn == null)
            {
                BowIsSet = false;
                return;
            }

            if (UpperBn.Length == 0 || LowerBn.Length == 0)
            {
                BowIsSet = false;
                return;
            }

            BowTension = 0;

            UpperBnInitRotation = new Quaternion[UpperBn.Length];   //Get the Initial Upper ChainRotation
            LowerBnInitRotation = new Quaternion[LowerBn.Length];    //Get the Initial Lower ChainRotation

            for (int i = 0; i < UpperBn.Length; i++)
            {
                if (UpperBn[i] == null)
                {
                    BowIsSet = false;
                    return;
                }
                UpperBnInitRotation[i] = UpperBn[i].localRotation;
            }
            for (int i = 0; i < LowerBn.Length; i++)
            {
                if (LowerBn[i] == null)
                {
                    BowIsSet = false;
                    return;
                }
                LowerBnInitRotation[i] = LowerBn[i].localRotation;
            }

            BowIsSet = true;
        }


        /// <summary> Create an arrow ready to shooot </summary>
        public virtual void EquipArrow()
        {
            if (ArrowInstance != null)
            { Destroy(ArrowInstance.gameObject); } //return;                             //If there is no arrow in the game object slot ignore

            ArrowInstance = Instantiate(Arrow, KNot);                                    //Instantiate the Arrow in the Knot of the Bow
            ArrowInstance.transform.localPosition = Vector3.zero;                        //Reset Position
            ArrowInstance.transform.localRotation = Quaternion.identity;                 //Reset Rotation

            var arrow = ArrowInstance.GetComponent<MArrow>();        //Get the IArrow Component

            if (arrow != null)
            {
                ArrowInstance.transform.Translate(0, 0, arrow.TailOffset, Space.Self);  //Translate in the offset of the arrow to put it on the hand
                arrow.AffectStat = AffectStat;
            }
            OnLoadArrow.Invoke(ArrowInstance);

        }

        /// <summary> Destroy the Active Arrow , used when is Stored the Weapon again and it has an arrow ready</summary>
        public virtual void DestroyArrow()
        {
            if (ArrowInstance != null)
                Destroy(ArrowInstance.gameObject);

            ArrowInstance = null; //Clean the Arrow Instance
        }

        /// <summary>Rotate and modify the bow Bones to bend it from Min = 0 to Max = 1</summary>
        /// <param name="normalizedTime">0 = Relax, 1 = Stretch</param>
        public virtual void BendBow(float normalizedTime)
        {
            if (!BowIsSet) return;

            BowTension = Mathf.Clamp01(normalizedTime); // Normalize time from 0 to 1
            OnHold.Invoke(BowTension);                           //Invoke on Hold Event;

            for (int i = 0; i < UpperBn.Length; i++)
            {
                if (UpperBn[i] != null)
                {
                    UpperBn[i].localRotation =
                        Quaternion.Lerp(UpperBnInitRotation[i], Quaternion.Euler(RotUpperDir * MaxTension) * UpperBnInitRotation[i], BowTension);  //Bend the Upper Chain on the Bow
                }
            }

            for (int i = 0; i < LowerBn.Length; i++)
            {
                if (LowerBn[i] != null)
                {
                    LowerBn[i].localRotation =
                        Quaternion.Lerp(LowerBnInitRotation[i], Quaternion.Euler(RotLowerDir * MaxTension) * LowerBnInitRotation[i],  BowTension);  //Bend the Lower Chain of the Bow
                }
            }

            if (knot && arrowPoint)
            {
                Debug.DrawRay(KNot.position, KNot.forward, Color.red);
            }
        }



        public virtual void ReleaseArrow(Vector3 direction)
        {
            if (!releaseArrow)
            {
                DestroyArrow();
                return;
            }

            if (ArrowInstance == null) return;

            ArrowInstance.transform.parent = null;

            IArrow arrow = ArrowInstance.GetComponent<IArrow>();

            arrow.HitMask = HitLayer;                                   //Transfer the same Hit Mask to the Arrow
            arrow.TriggerInteraction = TriggerInteraction;              //Link the Trigger Interatction 

            arrow.Damage = Mathf.Lerp(MinDamage, MaxDamage, BowTension);
            arrow.ShootArrow(Mathf.Lerp(MinForce, MaxForce, BowTension), direction.normalized);

            OnReleaseArrow.Invoke(ArrowInstance);

            ArrowInstance = null;
        }

        /// <summary>
        /// Is called  when the Rider is not holding the string of the bow
        /// </summary>
        public virtual void RestoreKnot()
        {
            KNot.localPosition = DefaultPosKnot;
            DestroyArrow();
        }


        public override void PlaySound(int ID)
        {
            if (ID > Sounds.Length - 1) return;

            if (Sounds[ID] != null && WeaponSound != null)
            {
                if (WeaponSound.isPlaying) WeaponSound.Stop();

                if (ID == 2)                                    //THIS IS THE SOUND FOR BEND THE BOW
                {
                    WeaponSound.pitch = 1.03f / HoldTime;
                    StartCoroutine(BowHoldTimePlay(ID));
                }
                else
                {
                    WeaponSound.pitch = 1;
                    WeaponSound.PlayOneShot(Sounds[ID]);   //Play Draw/ Weapon Clip
                }
            }
        }

        IEnumerator BowHoldTimePlay(int ID)
        {
            while (BowTension == 0) yield return null;

            WeaponSound.PlayOneShot(Sounds[ID]);
        }

        public override void ResetWeapon()
        {
            RestoreKnot();
            BendBow(0);
        }

        //Editor variables
        [HideInInspector] public bool BonesFoldout, proceduralfoldout;
        [HideInInspector] public int LowerIndex, UpperIndex;
    }
}