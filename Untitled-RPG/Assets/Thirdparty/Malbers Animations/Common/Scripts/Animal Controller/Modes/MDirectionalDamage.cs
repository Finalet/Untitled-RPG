using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Mode Modifier/Directional Damage")]
    public class MDirectionalDamage : ModeModifier
    {
        public enum HitDirection { TwoSides, FourSides, SixSides }

        public HitDirection hitDirection = HitDirection.SixSides;
       
        [Header("Damage Abilities")]
        public int Left  = 1;
        public int Right  = 2;
        [ConditionalHide("show4Sides",true)]
        public int Front = 3;
        [ConditionalHide("show4Sides",true)]
        public int Back = 4;
        [ConditionalHide("show6Sides",true)]
        public int FrontLeft = 3;
        [ConditionalHide("show6Sides",true)]
        public int FrontRight = 4;
        [ConditionalHide("show6Sides",true)]
        public int BackRight = 5;
        [ConditionalHide("show6Sides",true)]
        public int BackLeft = 6;


        public bool debug = false;

        public override void OnModeEnter(Mode mode)
        {
            MAnimal animal = mode.Animal;

            Vector3 HitDirection = animal.HitDirection;

            if (HitDirection == Vector3.zero) //Set it to random if there's no hit direction
            {
                mode.AbilityIndex = -1;
                return;
            }

            HitDirection = Vector3.ProjectOnPlane(HitDirection, animal.UpVector);     //Remove the Y on the Direction

            float angle = Vector3.Angle(animal.Forward, HitDirection);                           //Get The angle


            bool left = Vector3.Dot(animal.Right, HitDirection) < 0;            //Calculate which directions comes the hit Left or right
                                                                         //Debug.Log(angle  * (left ? 1:-1));

            int Side = -1;

            switch (hitDirection)
            {
                case MDirectionalDamage.HitDirection.TwoSides:
                    mode.AbilityIndex = left ? Left : Right;
                    break;
                case MDirectionalDamage.HitDirection.FourSides:
                        
                    if (angle <= 45)
                    {
                        Side = Front;
                    }
                    else if (angle > 45 && angle <= 135)
                    {
                        Side = left ? Right : Left;
                    }
                    else if (angle > 135)
                    {
                        Side = Back;
                    }


                    if (debug)
                    {
                        var Colordeb = Color.blue;
                        float mult = 4;
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, 45, 0) * animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, -45, 0) * animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, 135, 0) * animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, -135, 0) * animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, angle * (left ? -1 : 1), 0) * animal.transform.forward * mult, Color.red, 3f);
                    }


                    break;
                case MDirectionalDamage.HitDirection.SixSides:

                    if (debug)
                    {
                        var Colordeb = Color.blue;
                        float mult = 4;
                        Debug.DrawRay(animal.transform.position , animal.transform.forward* mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, -animal.transform.forward* mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, 60, 0) * animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, -60, 0) * animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, 120, 0) * animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, -120, 0) * animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, angle * (left ? -1 : 1), 0) * animal.transform.forward * mult, Color.red, 3f);
                    }

                    if (!left)
                    {
                        if (angle > 0 && angle <= 60) Side = FrontRight;
                        else if (angle > 60 && angle <= 120) Side = Right;
                        else if (angle > 120 && angle <= 180) Side = BackRight;
                    }
                    else
                    {
                        if (angle > 0 && angle <= 60) Side = FrontLeft;
                        else if (angle > 60 && angle <= 120) Side = Left;
                        else if (angle > 120 && angle <= 180) Side = BackLeft;
                    }
                    break;
                default:
                    break;
            }

            //if (debug) 
            //{
            //    Debug.Log(animal.name + "[Angle: " + angle + "]");
            //    //Debug.DrawRay(animal.transform.position+HitDirection.normalized, HitDirection.normalized, Color.red, 3f);
            //    //Debug.DrawRay(animal.transform.position, animal.Forward*5, Color.red, 3f);
            //}

            mode.AbilityIndex = Side;
        }


        [HideInInspector] public bool show4Sides;
        [HideInInspector] public bool show6Sides;
        private void OnValidate()
        {
            switch (hitDirection)
            {
                case HitDirection.TwoSides:
                    show4Sides = false;
                    show6Sides = false;
                    break;
                case HitDirection.FourSides:
                    show4Sides = true;
                    show6Sides = false;
                    break;
                case HitDirection.SixSides:
                    show4Sides = false;
                    show6Sides = true;
                    break;
                default:
                    break;
            }
        }
    }
}