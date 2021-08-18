using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Mode/Directional Damage")]
    public class MDirectionalDamage : ModeModifier
    {
        public enum HitDirection { TwoSides, FourSides, SixSides }

        [Header("Damage Abilities")]
        public HitDirection hitDirection = HitDirection.SixSides;
       
        [Hide("show6Sides",true)]
        public int FrontRight = 4;
        public int Right  = 2;
        [Hide("show6Sides",true)]
        public int BackRight = 5;
        [Hide("show6Sides",true)]
        public int FrontLeft = 3;
        public int Left  = 1;
        [Hide("show6Sides",true)]
        public int BackLeft = 6;
        [Hide("show4Sides",true)]
        public int Front = 3;
        [Hide("show4Sides",true)]
        public int Back = 4;


        public bool debug = false;

        public override void OnModeEnter(Mode mode)
        {
            MAnimal animal = mode.Animal;

            Vector3 HitDirection = animal.GetComponent<IMDamage>().HitDirection;

            if (HitDirection == Vector3.zero)  return; //Set it to random if there's no hit direction
          

            HitDirection = Vector3.ProjectOnPlane(HitDirection, animal.UpVector);     //Remove the Y on the Direction
            float angle = Vector3.Angle(animal.Forward, HitDirection);                           //Get The angle
            bool left = Vector3.Dot(animal.Right, HitDirection) < 0;            //Calculate which directions comes the hit Left or right

            var Colordeb = Color.blue;
            float mult = 4;

            int Side = -99;

            switch (hitDirection)
            {
                case MDirectionalDamage.HitDirection.TwoSides:
                    Side = left ? Left : Right;

                    if (debug)
                    {
                        Debug.DrawRay(animal.transform.position,  animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position,  -animal.transform.forward * mult, Colordeb, 3f);
                        Debug.DrawRay(animal.transform.position, Quaternion.Euler(0, angle * (left ? -1 : 1), 0) * animal.transform.forward * mult, Color.red, 3f);
                    }



                    break;
                case MDirectionalDamage.HitDirection.FourSides:
                        
                    if (angle <= 45)
                    {
                        Side = Front;
                    }
                    else if (angle >= 45 && angle <= 135)
                    {
                        Side = left ? Right : Left;
                    }
                    else if (angle >= 135)
                    {
                        Side = Back;
                    }


                    if (debug)
                    {
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
                        if (angle >= 0 && angle <= 60) Side = FrontRight;
                        else if (angle > 60 && angle <= 120) Side = Right;
                        else if (angle > 120 && angle <= 180) Side = BackRight;
                    }
                    else
                    {
                        if (angle >= 0 && angle <= 60) Side = FrontLeft;
                        else if (angle > 60 && angle <= 120) Side = Left;
                        else if (angle > 120 && angle <= 180) Side = BackLeft;
                    }
                    break;
                default:
                    break;
            }



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