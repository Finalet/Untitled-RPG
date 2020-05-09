using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [HelpURL("https://docs.google.com/document/d/1QBLQVWcDSyyWBDrrcS2PthhsToWkOayU0HUtiiSWTF8/edit#heading=h.kraxblx9518t")]
    public class Death : State
    {
        [Header("Death Parameters")]
        public bool DisableAllComponents = true;
        public bool RemoveAllTriggers = false;

        public bool QueueOnFly = true;

        public override void Activate()
        {
            if (QueueOnFly)
            {
                if (animal.ActiveStateID == 6)//Means is  Flying
                {
                    if (animal.debugStates) Debug.Log(ID.name + " State has being Queue");
                    animal.ActiveState.AllowExit();
                    OnQueue = true;
                    animal.QueueState(this);
                    return;
                }
            }

            base.Activate();

            IgnoreLowerStates = true;

            animal.Mode_Interrupt(); //Stop all modes;
            //animal.Mode_Stop(); //Stop all modes;

            if (DisableAllComponents)
            {
                var AllComponents = animal.GetComponents<MonoBehaviour>();
                foreach (var comp in AllComponents)
                {
                    if (comp == animal) continue;

                    comp.enabled = false;
                }

            }
            if (RemoveAllTriggers)
            {
                var AllTriggers = animal.GetComponentsInChildren<Collider>();

                foreach (var trig in AllTriggers)
                {
                    if (trig.isTrigger)
                    {
                        Destroy(trig.gameObject);
                    }
                }
            }

            animal.StopMoving();
        }


#if UNITY_EDITOR        
        void Reset()
        {
           
            ID = MalbersTools.GetInstance<StateID>("Death");

            General = new AnimalModifier()
            {
                modify = (modifier)(-1),
                Persistent = true,
                LockInput = true,
                LockMovement = true,
                Colliders = false,
            };
        }
#endif
    }
}