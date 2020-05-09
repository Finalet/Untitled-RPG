using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events; 

namespace MalbersAnimations.Controller
{
    public class MCheckPoint : MonoBehaviour
    {
        /// <summary>List of all the CheckPoint on the Scene</summary>
        public static List<MCheckPoint> CheckPoints;
        
        /// <summary>Last CheckPoint the Animal use</summary>
        public static MCheckPoint LastCheckPoint;

        public UnityEvent OnActive = new UnityEvent();
        public UnityEvent OnEnter = new UnityEvent();

        public Collider Collider { get; set; } 

        // Use this for initialization
        void Start()
        {
            if (MRespawner.instance == null)
            {
                Debug.LogWarning(name + " has being destroyed since there's no Respawner");
                Destroy(gameObject); //Destroy the CheckPoint if is there no Respawner
            }

            if (MAnimal.MainAnimal == null)
            {
                Debug.LogWarning(name + " has being destroyed since there's no Main Animal Player, Set on your Main Character: Main Player = true");
                Destroy(gameObject); //Destroy the CheckPoint if is there no Respawner
            }

            Collider = GetComponent<Collider>();

            if (Collider)
            {
                Collider.isTrigger = true;
            }
            else
            {
                Debug.LogError(name + " Needs a Collider");
            }
            OnActive.Invoke();
        }


        void OnTriggerEnter(Collider other)
        {
            if (LastCheckPoint == this) return; //Means the animal has already enter this CheckPoint
            var animal = other.GetComponentInParent<MAnimal>();

            if (!animal) return;        //Skip if there's no Animal

            if (animal != MAnimal.MainAnimal) return; //Skip if there's no the Player Animal

            MRespawner.instance.transform.position = transform.position;        //Set on the Respawner to this Position
            MRespawner.instance.transform.rotation = transform.rotation;        //Set on the Respawner to this Position
            MRespawner.instance.RespawnState = animal.ActiveStateID;            //Set on the Respawner the Last Animal State

            if (LastCheckPoint)
            {
                LastCheckPoint.OnActive.Invoke();
            }

            LastCheckPoint = this;                                  //Check that the last check Point of entering was this one
            OnEnter.Invoke();
            Collider.enabled = false;
        }


        public static void ResetCheckPoint()
        {
            if (LastCheckPoint)
            {
                LastCheckPoint.Collider.enabled = true;
                LastCheckPoint.OnActive.Invoke();
                LastCheckPoint = null;
            }
        }

        void OnEnable()
        {
            if (CheckPoints == null) CheckPoints = new List<MCheckPoint>();
            CheckPoints.Add(this);          //Save tis CheckPoint  on the CheckPoint List
        }

        void OnDisable()
        {
            if (CheckPoints != null) CheckPoints.Remove(this);       //Remove this CheckPoint from the CheckPoint List
        }
    }
}