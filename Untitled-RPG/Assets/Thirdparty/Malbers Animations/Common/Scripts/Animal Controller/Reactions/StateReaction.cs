using UnityEngine;


namespace MalbersAnimations.Controller.Reactions
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/Animal Reactions/State Reaction"/*, order = 0*/)]
    public class StateReaction : MReaction
    {
        public State_Reaction type = State_Reaction.Activate;
        public StateID ID;

        [Hide("ShowStatus",true,false)]
        public int StateStatus;

        protected override void _React(MAnimal animal)
        {
            switch (type)
            {
                case State_Reaction.Activate:
                    animal.State_Activate(ID);
                    break;
                case State_Reaction.AllowExit:
                    if (animal.ActiveStateID == ID) animal.ActiveState.AllowExit();
                    break;
                case State_Reaction.ForceActivate:
                    animal.State_Force(ID);
                    break;
                case State_Reaction.Enable: 
                    animal.State_Enable(ID);
                    break;
                case State_Reaction.Disable:
                    animal.State_Disable(ID);
                    break;
                case State_Reaction.SetStatus:
                    animal.State_SetStatus(StateStatus);
                    break;
                default:
                    break;
            }
        }


        protected override bool _TryReact(MAnimal animal)
        {
            if (!animal.HasState(ID)) return false;

            switch (type)
            {
                case State_Reaction.Activate:
                    return animal.State_TryActivate(ID);
                case State_Reaction.AllowExit:
                    if (animal.ActiveStateID == ID)
                    { 
                        animal.ActiveState.AllowExit(); 
                        return true;
                    }
                    return false;
                case State_Reaction.ForceActivate:
                    animal.State_Force(ID);
                    return true;
                case State_Reaction.Enable:
                    animal.State_Enable(ID);
                    return true;
                case State_Reaction.Disable:
                    animal.State_Disable(ID);
                    return true;
                case State_Reaction.SetStatus:
                    animal.State_SetStatus(StateStatus);
                    return true;
                default:
                    return false;
            }
        }


        public enum State_Reaction
        {
            /// <summary>Tries to Activate the State of the Zone</summary>
            Activate,
            /// <summary>If the Animal is already on the state of the zone it will allow to exit and activate states below the Active one</summary>
            AllowExit,
            /// <summary>Force the State of the Zone to be enable even if it cannot be activate at the moment</summary>
            ForceActivate,
            /// <summary>Enable a  Disabled State </summary>
            Enable,
            /// <summary>Disable State </summary>
            Disable,
            /// <summary>Change the Status ID of the State in case the State uses its</summary>
            SetStatus
        }




        /// 
        /// VALIDATIONS
        /// 



        private void OnEnable() { Validation(); }

        private void OnValidate() { Validation(); }

        [HideInInspector] public bool ShowStatus;
        private const string reactionName = "State → ";

        void Validation()
        {
            fullName = reactionName + type.ToString() + " [" + (ID != null ? ID.name : "None") + "]";
            ShowStatus = false;

            switch (type)
            {
                case State_Reaction.Activate:
                    description = "Tries to activate a State on the Animal";
                    break;
                case State_Reaction.AllowExit:
                    description = "If the Animal is on the [" + (ID != null ? ID.name : "None") + "] state,\nThen it will enable [Exit] the state, Allowing low priority States to try to activate themselves";
                    break;
                case State_Reaction.ForceActivate:
                    description = "Activate a state, regardless the activation Conditions";
                    break;
                case State_Reaction.Enable:
                    description = "Enable a State on the Animal";
                    break;
                case State_Reaction.Disable:
                    description = "Disable a State on the Animal";
                    break;
                case State_Reaction.SetStatus:
                    description = "Set the Status value of a state";
                    ShowStatus = true;
                    break;
                default:
                    break;
            }
        }
    }


}
