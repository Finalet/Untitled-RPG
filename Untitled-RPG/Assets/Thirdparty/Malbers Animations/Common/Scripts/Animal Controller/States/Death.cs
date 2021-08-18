using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [HelpURL("https://docs.google.com/document/d/1QBLQVWcDSyyWBDrrcS2PthhsToWkOayU0HUtiiSWTF8/edit#heading=h.kraxblx9518t")]
    public class Death : State
    {
        public override string StateName => "Death";

        [Header("Death Parameters")]
        public bool DisableAllComponents = true;
        public bool RemoveAllColliders = false;
        public bool RemoveAllTriggers = true;
        public float RigidbodyDrag = 5;
        public float RigidbodyAngularDrag = 15;

        [Space]
        public bool disableAnimal = true;
        
        [Hide("disableAnimal",true,false)] 
        public float disableAnimalTime = 5f; 

  

        public override void Activate()
        {
            base.Activate();

            if (OnQueue) return;

            animal.Mode_Interrupt();
            animal.Mode_Disable_All();
           

            if (DisableAllComponents)
            {
                var AllComponents = animal.GetComponentsInChildren<MonoBehaviour>();
                foreach (var comp in AllComponents)
                {
                    if (comp == animal) continue;
                    if (comp != null)comp.enabled = false;
                }
            }

            var AllTriggers = animal.GetComponentsInChildren<Collider>();

            foreach (var trig in AllTriggers)
            {
                if (RemoveAllColliders || RemoveAllTriggers &&  trig.isTrigger)
                {
                    Destroy(trig);
                }
            }


            animal.SetCustomSpeed(new MSpeed("Death"));

            animal.StopMoving();
            animal.Mode_Interrupt();
            animal.Mode_Stop();

            if (animal.RB)
            {
                animal.RB.drag = RigidbodyDrag;
                animal.RB.angularDrag = RigidbodyAngularDrag;
            }

           if (disableAnimal) animal.DisableAnimalComponent(disableAnimalTime); //Disable the Animal Component after x time
        }


#if UNITY_EDITOR        
        void Reset()
        {
           
            ID = MTools.GetInstance<StateID>("Death");

            General = new AnimalModifier()
            {
                modify = (modifier)(-1),
                Persistent = true,
                LockInput = true,
                LockMovement = true,
                AdditiveRotation = true,
            };
        }
#endif
    }
}