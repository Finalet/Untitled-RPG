using UnityEngine;

namespace MalbersAnimations.Controller.Reactions
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/Animal Reactions/Mode Reaction"/*, order = 1*/)]
    public class ModeReaction : MReaction
    {
        public Mode_Reaction type = Mode_Reaction.Activate;
        public ModeID ID;
        
        [Hide("ShowAction", true,false)]
        public MAction action;

        [Hide("ShowAbilityIndex", true,false), Tooltip("If set to -99 it will do a random ability from the Mode")]
        public int ModeAbility = -99;

        [Hide("ShowCooldown", true,false)]
        public float coolDown = 0;

       [Hide("ShowStatus", true, false)]
        public AbilityStatus abilityStatus = AbilityStatus.PlayOneTime;


       [Hide("ShowAbilityTime", true, true)]
        public float AbilityTime = 3f;

        int AbilityIndex => ID.ID == 4 ? (action != null ? action.ID : -1) : ModeAbility;

        protected override void _React(MAnimal animal)
        {
            var mode = animal.Mode_Get(ID);
            if (mode == null || ID == null) return;
            _TryReact(animal);
        }

        protected override bool _TryReact(MAnimal animal)
        {
            var mode = animal.Mode_Get(ID);
            if (mode == null || ID == null) return false;

            switch (type)
            {
                case Mode_Reaction.Activate:
                    return animal.Mode_TryActivate(ID, AbilityIndex, abilityStatus,AbilityTime);
                case Mode_Reaction.Interrupt:
                    if (animal.ActiveMode.ID == ID)
                    {
                        animal.Mode_Interrupt();
                        return true;
                    }
                    return false;
                case Mode_Reaction.SetActiveIndex:
                    mode.SetAbilityIndex(AbilityIndex);
                    return true;
                case Mode_Reaction.ResetActiveIndex:
                    mode.ResetAbilityIndex();
                    return true;
                case Mode_Reaction.Enable:
                    animal.Mode_Enable(ID);
                    return true;
                case Mode_Reaction.Disable:
                    animal.Mode_Disable(ID);
                    return true;
                case Mode_Reaction.CoolDown:
                    mode.CoolDown.Value = coolDown;
                    return true;
                case Mode_Reaction.ForceActivate:
                    animal.Mode_ForceActivate(ID, AbilityIndex);
                    return true;
                case Mode_Reaction.ActivateForever:
                    return animal.Mode_TryActivate(ID, AbilityIndex,  AbilityStatus.Forever);
                case Mode_Reaction.Stop:
                    animal.Mode_Stop();
                    return true;
                case Mode_Reaction.EnableAbility:
                    return animal.Mode_Ability_Enable(ID, AbilityIndex, true);
                case Mode_Reaction.DisableAbility:
                    return animal.Mode_Ability_Enable(ID, AbilityIndex, false);
                default:
                    return false;
            }
        }

        public enum Mode_Reaction
        {
            /// <summary>Tries to Activate the State of the Zone</summary>
            Activate,
            /// <summary>Activate  a Mode Forever</summary>
            ActivateForever,
            /// <summary>If the Animal will set the Mode Status to Interrput -2</summary>
            Interrupt,
            /// <summary>The Mode will be Stopped and its Input Reseted</summary>
            Stop,
            /// <summary>Force the State of the Zone to be enable even if it cannot be activate at the moment</summary>
            SetActiveIndex,
            /// <summary>Enable a  Disabled State </summary>
            Enable,
            /// <summary>Disable State </summary>
            Disable,
            ///// <summary>Change the Status ID of the State in case the State uses its</summary>
            //ChangeStatus,
            /// <summary>Change the Status ID of the State in case the State uses its</summary>
            CoolDown,
            /// <summary>Change the Status ID of the State in case the State uses its</summary>
            ResetActiveIndex,
            /// <summary>Force a Mode to be activated.. ignoring if another mode is playing</summary>
            ForceActivate,
            /// <summary>Force a Mode to be activated.. ignoring if another mode is playing</summary>
            EnableAbility,
            /// <summary>Force a Mode to be activated.. ignoring if another mode is playing</summary>
            DisableAbility,
        }

         


        /// 
        /// VALIDATIONS
        /// 
        private void OnEnable() { Validation(); }

        private void OnValidate() { Validation(); }

        [HideInInspector] public bool ShowAction;
        [HideInInspector] public bool ShowCooldown;
        [HideInInspector] public bool ShowStatus;
        [HideInInspector] public bool ShowAbilityIndex;
        [HideInInspector] public bool ShowAbilityTime;


        private const string reactionName = "Mode → ";

       public void Validation()
        {
            ShowCooldown = false;
            ShowStatus = false;

            ShowAbilityTime = abilityStatus == AbilityStatus.PlayOneTime;

            fullName = reactionName + type.ToString() + " [" + (ID != null ? ID.name : "None") + "]";

            switch (type)
            {
                case Mode_Reaction.Activate:
                    description = "Activate a Mode on the Animal";
                    if (ID && ID.ID == 4 && action == null)
                        description += " If Action is Empty it will play any Action";
                    ShowAction = (ID && ID.ID == 4);
                    ShowAbilityIndex = !ShowAction;
                    ShowStatus = true;
                    break;
                case Mode_Reaction.ActivateForever:
                    description = "Activate Forever a Mode";
                    if (ID && ID.ID == 4 && action == null)
                        description += " If Action is Empty it will play any Action";
                    ShowAction = (ID && ID.ID == 4);
                    ShowAbilityIndex = !ShowAction;
                    break;
                case Mode_Reaction.Interrupt:
                    description = "If the Animal is Playing " + (ID ? "the " + ID.name : "any") + " Mode it will interrupt it";
                    fullName = reactionName + type.ToString() + " [" + (ID != null ? ID.name : "any") + "]";
                    ShowAction = ShowAbilityIndex = false;
                    break;
                case Mode_Reaction.SetActiveIndex:
                    ShowAction = (ID && ID.ID == 4);
                    ShowAbilityIndex = !ShowAction;
                    description = "Changes the Active Index of a Mode";
                    break;
                case Mode_Reaction.ResetActiveIndex:
                    description = "Reset the Active Index of a Mode";
                    ShowAction = ShowAbilityIndex = false;
                    break;
                case Mode_Reaction.Enable:
                    description = "Enable a Mode on an Animal";
                    ShowAction = ShowAbilityIndex = false;
                    break;
                case Mode_Reaction.Disable:
                    description = "Disable a Mode on an Animal";
                    ShowAction = ShowAbilityIndex = false;
                    break;
                case Mode_Reaction.CoolDown:
                    description = "Change the CoolDown of a Mode ";
                    ShowCooldown = true;
                    ShowAction = ShowAbilityIndex = false;
                    break;
                case Mode_Reaction.ForceActivate:
                    description = "Force a Mode on the Animal, If the Animal was making a mode then it will interrupted for a new one";
                    if (ID && ID.ID == 4 && action == null)
                        description += " If Action is Empty it will play any Action";
                    ShowAction = (ID && ID.ID == 4);
                    ShowAbilityIndex = !ShowAction;
                    break;
                case Mode_Reaction.Stop:
                    description = "If is Playing a Mode it will Stop it!";
                    fullName = reactionName + type.ToString();
                    ShowAction = false;
                    ShowAbilityIndex = false;
                    break;
                case Mode_Reaction.EnableAbility:
                    description = "Enable an Ability on the Animal";
                    if (ID && ID.ID == 4 && action == null)
                        description += "\nPlease Select an Action";
                    ShowAction = (ID && ID.ID == 4);
                    ShowAbilityIndex = !ShowAction;
                    break;
                case Mode_Reaction.DisableAbility:
                    description = "Disable an Ability on the Animal";
                    if (ID && ID.ID == 4 && action == null)
                        description += "\nPlease Select an Action";
                    ShowAction = (ID && ID.ID == 4);
                    ShowAbilityIndex = !ShowAction;
                    break;
                default:
                    break;
            }
        }
    }
}
