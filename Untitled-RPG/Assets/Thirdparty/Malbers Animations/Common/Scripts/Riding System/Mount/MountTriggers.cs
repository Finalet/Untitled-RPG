using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.HAP
{
    /// <summary>This Enable the mounting System</summary> 
    [AddComponentMenu("Malbers/Riding/Mount Trigger")]
    public class MountTriggers : MonoBehaviour
    {
        [SerializeField, RequiredField]
        protected Mount Montura;

        [Tooltip("If true when the Rider enter the Trigger it will mount automatically")]
        public BoolReference AutoMount = new BoolReference(false);

        [Tooltip("Can be used also for dismounting")]
        public BoolReference Dismount = new BoolReference(true);

        /// <summary>Avoids Automount again after Dismounting and Automount was true</summary>
        public bool WasAutomounted { get; internal set; }
         
        /// <summary>The Transition ID value to dismount this kind of Montura.. (is Located on the Animator)</summary>
        [Tooltip("The Transition ID value to Mount the Animal, to Play the correct Mount Animation"),UnityEngine.Serialization.FormerlySerializedAs("DismountID")]
        public IntReference MountID;
        /// <summary>The Transition ID value to dismount this kind of Montura.. (is Located on the Animator)</summary>
        [Tooltip("The Transition ID value to Dismount the Animal, to Play the correct Mount Animation"), UnityEngine.Serialization.FormerlySerializedAs("DismountID")]
        public IntReference m_DismountID;

        public int DismountID => m_DismountID == 0 ? MountID : m_DismountID;

        [Tooltip("If the Rider has set the Dismount Option to Direction, it will use this parameter to find the Closest Direction")]
        /// <summary>The Local Direction of the Mount Trigger compared with the animal</summary>
        public Vector3Reference Direction;

        [CreateScriptableAsset] public TransformAnimation Adjustment;

        /// <summary>Rider that is inside the Trigger</summary>
        public  MRider NearbyRider
        { 
            get => Montura.NearbyRider;
            internal set => Montura.NearbyRider = value;
        }

        // Use this for initialization
        void OnEnable()
        {
            if (Montura == null)
                Montura = GetComponentInParent<Mount>(); //Get the Mountable in the parents
        }

        //private void OnDisable()
        //{
        //}

        void OnTriggerEnter(Collider other)
        {
            if (!gameObject.activeInHierarchy || other.isTrigger) return; // Do not allow disable triggers
            GetAnimal(other);

            if (PeaceCanvas.instance && other.CompareTag("Player")) PeaceCanvas.instance.ShowKeySuggestion(KeyCodeDictionary.keys[KeybindsManager.instance.currentKeyBinds["Interact"]], InterractionIcons.Horse);
        }


        private void GetAnimal(Collider other)
        {
            if (!Montura)
            {
                Debug.LogError("No Mount Script Found... please add one");
                return;
            }

            if (!Montura.Mounted && Montura.CanBeMounted)          //If there's no other Rider on the Animal or the the Animal isn't death
            {
                var newRider = other.FindComponent<MRider>();

                if (newRider != null)
                {
                    if (newRider.IsMountingDismounting) return;     //Means the Rider is already mounting/Dismounting the animal so skip
                    if (newRider.MainCollider != other) return;     //Check if we are entering the Trigger with the Riders Main Collider Only (Not Body Parts)


                    if (NearbyRider == null || NearbyRider.MountTrigger != this)  //If we are checking the same Rider or a new rider
                    {
                        newRider.MountTriggerEnter(Montura, this);   //Set Everything Requiered on the Rider in order to Mount
                    
                        if (AutoMount.Value && !WasAutomounted)
                        {
                            newRider.MountAnimal();
                        }
                    }
                }
            }
        }



        void OnTriggerExit(Collider other)
        {
            if (!gameObject.activeInHierarchy || other.isTrigger) return;       // Do not allow triggers

            var newRider = other.FindComponent<MRider>();

            if (newRider != null && NearbyRider == newRider)
            {
                if (NearbyRider.IsMountingDismounting) return;             //You Cannot Mount if you are already mounted

                //When exiting if we are exiting From the same Mount Trigger means that there's no mountrigger Nearby
                if (NearbyRider.MountTrigger == this
                    && !Montura.Mounted &&
                    newRider.MainCollider == other)      //Check if we are exiting the Trigger with the Main Collider Only (Not Body Parts)
                {
                    NearbyRider.MountTriggerExit();

                    if (PeaceCanvas.instance) PeaceCanvas.instance.HideKeySuggestion();
                }
            }
        }

        private void Reset()
        {
            if (Montura == null)
                Montura = GetComponentInParent<Mount>(); //Get the Mountable in the parents
        }
    }
}