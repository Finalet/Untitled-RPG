using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Mode/Attack Aligner")]
    public class MAttackAligner : ModeModifier
    {
        public FloatReference FindRadius = new FloatReference(5);
        public FloatReference AlignTime = new FloatReference(0.15f);
        public LayerReference Layer = new LayerReference(-1);

        public override void OnModeEnter(Mode mode)
        {
            MAnimal animal = mode.Animal;

            var pos = animal.Center;

            var AllColliders = Physics.OverlapSphere(pos, FindRadius,  Layer.Value);

            Collider MinDistanceCol = null;
            float Distance = float.MaxValue;

            foreach (var col in AllColliders)
            {
                if (col.transform.root == animal.transform.root) continue; //Don't Find yourself

                var DistCol = Vector3.Distance(animal.Center, col.transform.position);

                if (Distance > DistCol)
                {
                    Distance = DistCol;
                    MinDistanceCol = col;
                }
            }

            if (MinDistanceCol)
            {
                animal.StartCoroutine(MTools.AlignLookAtTransform(animal.transform, MinDistanceCol.transform, AlignTime));
            }
        } 
    }
}