using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Mode/Directional Dodge")]
    public class ModifierForce : ModeModifier
    {
        [HelpBox]
        public string Desc ="Applies a Force to the Animal when the Mode starts. Remove the force when the mode ends";
        public Vector3Reference Direction = new Vector3Reference(Vector3.forward);
       
        public FloatReference Force = new FloatReference(2);
        public FloatReference EnterAceleration = new FloatReference(5);
        public FloatReference ExitAceleration = new FloatReference(5);
        public BoolReference ResetGravity = new BoolReference(true);

        public override void OnModeEnter(Mode mode)
        {
            mode.Animal.Force_Add(mode.Animal.transform.TransformDirection(Direction), Force, EnterAceleration, ResetGravity);
        }

        public override void OnModeExit(Mode mode)
        {
            mode.Animal.Force_Remove(ExitAceleration);
        } 
    }
}