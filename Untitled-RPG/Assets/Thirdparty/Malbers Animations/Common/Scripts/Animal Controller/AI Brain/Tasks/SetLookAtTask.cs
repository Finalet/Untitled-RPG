using MalbersAnimations.Utilities;
using UnityEngine;
namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Look At")]
    public class SetLookAtTask : MTask
    {
        [Tooltip("Check the Look At Component on the Target or on Self")]
        public Affected SetLookAtOn = Affected.Self;


        [Tooltip("If true .. it will Look for a gameObject on the Target with the Tag[tag].... else it will look for the gameObject name")]
        public bool UseTag = false; 

        [ConditionalHide("UseTag",true,true), Tooltip("Search for the Target Child gameObject name")]
        public string BoneName = "Head";
        [ConditionalHide("UseTag",true), Tooltip("Look for a child gameObject on the Target with the Tag[tag]")]
        public Tag tag;
        [Tooltip("When the Task ends it will Remove the Target on the Aim Component")]
        public bool DisableOnExit = true;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            Transform child = null;

            if (SetLookAtOn == Affected.Self)
            {
                if (UseTag)
                    child = GetGameObjectByTag(brain.Target);
                else
                    child = GetChildName(brain.Target);

                brain.GetComponentInParent<IAim>()?.SetTarget(child);
            }
            else
            {
                if (UseTag)
                    child = GetGameObjectByTag(brain.Animal.transform);
                else
                    child = GetChildName(brain.Animal.transform);

                brain.Target?.GetComponentInParent<IAim>()?.SetTarget(child);
            }
        }


        private Transform GetChildName(Transform Target)
        {
            Transform child = null;

            if (Target && !string.IsNullOrEmpty(BoneName))
                child = Target.FindGrandChild(BoneName);

            return child;
        }

        private Transform GetGameObjectByTag(Transform Target)
        {
            if (Target )
            {
                var allTags = Target.root.GetComponentsInChildren<Tags>();

                if (allTags == null) return null;

                foreach (var item in allTags)
                {
                    if (item.HasTag(tag))
                        return item.transform;
                }
            }
            return null;
        }

        public override void ExitTask(MAnimalBrain brain, int index)
        {
            if (DisableOnExit)
            {
                brain.Animal.GetComponentInParent<IAim>()?.SetTarget(null);
                brain.Target?.GetComponentInParent<IAim>()?.SetTarget(null);
            }
        }


        private void Reset() { Description = "Find a child gameObject with the name given on the Target and set it as the Target for the Look At and the Aim Component on the Animal "; }
    }
}
