using UnityEngine;
using System.Collections;
using MalbersAnimations.Events;
using UnityEngine.Events;

namespace MalbersAnimations.Controller
{
    /// <summary>
    /// Script to manage the logic of the Egg
    /// </summary>
    public class DragonEgg : MonoBehaviour 
    {
        public enum HatchType { None, Input, Time };         //Type of Hatch you want to do with the little dragon
        protected Animator anim;

        protected MAnimal animal;

        public Vector3 preHatchOffset;
       
        public GameObject Dragon;                               //The Dragon to Come out of the egg
        public float removeShells = 10f;                
        bool crack_egg;

        [HideInInspector]
        public InputRow input = new InputRow("CrackEgg", KeyCode.Alpha0, InputButton.Down);

        [HideInInspector]  public float seconds;

        public HatchType hatchtype;

        public UnityEvent OnEggCrack = new UnityEvent();

        void Start()
        {
            anim = GetComponent<Animator>();

            if (Dragon)
            {
                if (!Dragon.activeInHierarchy) Dragon = Instantiate(Dragon);

                animal = Dragon.GetComponent<MAnimal>();

                if (animal)
                {
                    animal.transform.position = transform.position;
                    animal.Anim.Play("Hatch");                                       //Set the egg State (This set on the animator INT -10 which is the transition for the EggHatching Start Animation
                    animal.LockInput = true;
                    animal.LockMovement = true;
                    animal.EnableColliders(false);
                    animal.transform.localPosition += preHatchOffset;
                }

                var skinnedMeshes = Dragon.GetComponentsInChildren<Renderer>();

                foreach (var item in skinnedMeshes)
                    item.enabled = false;
            }
          

            if (hatchtype == HatchType.Time)
            {
                StartCoroutine(TimeCrackEgg());
            }
        }


      

        void Update()
        {
            switch (hatchtype)
            {
                case HatchType.Input:
                    if (input.GetInput) crack_egg = true;
                    break;
                default:
                    break;
            }

            if (crack_egg)
            {
                CrackEgg();
            }
        }

        IEnumerator TimeCrackEgg()
        {
            yield return new WaitForSeconds(seconds);
            CrackEgg();
        }

        public void CrackEgg()
        {
            var col = GetComponent<Collider>();
            anim.SetInteger("State", 1); //Crak the Egg Animation
            if (col)
                Destroy(col);

            if (animal)
            {
                animal.State_Force(StateEnum.Idle);

                var skinnedMeshes = animal.GetComponentsInChildren<Renderer>();
                foreach (var item in skinnedMeshes) item.enabled = true;

                animal.SetIntID(Random.Range(1, 4)); //Set a random Out of the Egg animation
            }

            OnEggCrack.Invoke();
          
            StartCoroutine(EggDisapear(removeShells));
        }

        void EnableAnimalScript()
        {
           if (animal) animal.enabled = true;
        }

        //Destroy the Game Object
        IEnumerator EggDisapear(float seconds)
        {
            yield return null;
            yield return null;
            if (Dragon)  Dragon.transform.position = transform.position; //Restore the position to the egg
            yield return new WaitForSeconds(seconds);
            anim.SetInteger("State", 2);
            yield return new WaitForSeconds(1f);
            Destroy(transform.gameObject);
        }

        //void OnMouseDown()
        //{
        //    if (hatchtype == HatchType.Click && !crack_egg)
        //    {
        //        CrackEgg();
        //    }
        //}
    }
}
