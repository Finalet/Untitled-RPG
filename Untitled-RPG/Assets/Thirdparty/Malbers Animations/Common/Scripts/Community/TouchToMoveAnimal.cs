using UnityEngine;
using MalbersAnimations.Controller;

namespace MalbersAnimations.Community
{
    public class TouchToMoveAnimal : MonoBehaviour
    {
        public MAnimalAIControl animalAgent;

        void Update()
        {
            if (Input.touches.Length > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Ray touchRay = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit hit;

                    if (Physics.Raycast(touchRay, out hit))
                    {
                        Vector3 destinationPosition = hit.point;
                        animalAgent.SetDestination(destinationPosition);
                    }
                }
            }
        }
    }
}