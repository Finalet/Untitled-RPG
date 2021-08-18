 using UnityEngine;

namespace MalbersAnimations.Controller.Reactions
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/Animal Reactions/Speed Reaction"/*, order = 3*/)]
    public class SpeedReaction : MReaction
    {
        public enum Speed_Reaction { Activate, Increase, Decrease, LockSpeedChange, TopSpeed, AnimationSpeed , GlobalAnimatorSpeed, SetRandomSpeed ,  Sprint }

        public Speed_Reaction type = Speed_Reaction.Activate;

        [Hide("showSpeed_Set", true,false),Tooltip("Speed Set on the Animal to make the changes (E.g. 'Ground' 'Fly')")]
        public string SpeedSet = "Ground";
        [Hide("showSpeed_Index", true, false), Tooltip("Index of the Speed Set on the Animal to make the changes (E.g. 'Walk-1' 'Trot-2', 'Run-3')")]
        public int Index = 1;

        [Hide("ShowBoolValue",true,false)]
        public bool Value = true;
        [Hide("showAnimSpeed", true, false)]
        public float animatorSpeed = 1;

        protected override void _React(MAnimal animal)
        {
            switch (type)
            {
                case Speed_Reaction.LockSpeedChange:
                    animal.Speed_Change_Lock(Value);
                    break;
                case Speed_Reaction.Increase:
                    animal.SpeedUp();
                    break;
                case Speed_Reaction.Decrease:
                    animal.SpeedUp();
                    break;
                case Speed_Reaction.Activate:
                    animal.SpeedSet_Set_Active(SpeedSet, Index);
                    break;
                case Speed_Reaction.TopSpeed:
                    animal.Speed_SetTopIndex(SpeedSet, Index);
                    break;
                case Speed_Reaction.AnimationSpeed:
                    var Set = animal.SpeedSet_Get(SpeedSet);
                    Set[Index - 1].animator.Value = animatorSpeed;
                    break;
                case Speed_Reaction.GlobalAnimatorSpeed:
                    animal.AnimatorSpeed = animatorSpeed;
                    break;
                case Speed_Reaction.SetRandomSpeed:
                    var topspeed = animal.SpeedSet_Get(SpeedSet);
                    if (topspeed != null) animal.SpeedSet_Set_Active(SpeedSet, Random.Range(1, topspeed.TopIndex + 1));
                    break;
                case Speed_Reaction.Sprint:
                    animal.Sprint = Value;
                    break;
                default:
                    break;
            }
        }

        protected override bool _TryReact(MAnimal animal)
        {
            _React(animal);
            return true;
        }


        #region Validations
        /// 
        /// VALIDATIONS
        ///  
        private const string reactionName = "Speed → ";

        private void OnEnable() { Validation(); }
        private void OnValidate() { Validation(); }

        void Validation()
        {
            fullName = reactionName + type.ToString();

            ShowBoolValue = false;
            showAnimSpeed = false;
            showSpeed_Set = false;
            showSpeed_Index = false;

            switch (type)
            {
                case Speed_Reaction.Activate:
                    description = "Activate a Speed by its Index on a Speed Set";
                    showSpeed_Set = true;
                    showSpeed_Index = true;
                    fullName += " [" + SpeedSet + "(" + Index + ")]";
                    break;
                case Speed_Reaction.Increase:
                    description = "Increase a Speed on the Active Speed Set";
                    fullName += "+1";
                    break;
                case Speed_Reaction.Decrease:
                    description = "Decrease a Speed on the Active Speed Set";
                    fullName += "-1";
                    break;
                case Speed_Reaction.LockSpeedChange:
                    fullName += " [" + Value + "]";
                    ShowBoolValue = true;
                    description = "Lock Speed changes";
                    break;
                case Speed_Reaction.TopSpeed:
                    showSpeed_Set = true;
                    showSpeed_Index = true;
                    fullName += " [" + SpeedSet + "] Top Index[" + Index + "]";
                    description = "Changes the Top Speed on a Speed Set";
                    break;
                case Speed_Reaction.AnimationSpeed:
                    showSpeed_Set = true;
                    showSpeed_Index = true;
                    showAnimSpeed = true;
                    fullName += "[" + animatorSpeed + "] - " + SpeedSet + "[" + Index + ")";
                    description = "Modify the Animator multiplier Speed for a Speed Set.";
                    break;
                case Speed_Reaction.GlobalAnimatorSpeed:
                    showAnimSpeed = true;
                    fullName += "[" + animatorSpeed + "]";
                    break;
                case Speed_Reaction.SetRandomSpeed:
                    showSpeed_Set = true;
                    // showAnimSpeed = true;
                    fullName += " [" + SpeedSet + "(?)]";
                    description = "Set a Random Spede modifier on the SpeedSet";
                    break;
                case Speed_Reaction.Sprint:
                    fullName += " [" + Value + "]";
                    ShowBoolValue = true;
                    description = "Set the Sprint on the Animal";
                    break;
                default:
                    break;
            }
        }

        [HideInInspector] public bool ShowBoolValue;
        [HideInInspector] public bool showAnimSpeed;
        [HideInInspector] public bool showSpeed_Set;
        [HideInInspector] public bool showSpeed_Index;

        #endregion
    }
}
