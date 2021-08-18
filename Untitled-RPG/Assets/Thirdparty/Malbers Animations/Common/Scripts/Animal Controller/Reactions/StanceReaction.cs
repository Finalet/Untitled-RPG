using UnityEngine;

namespace MalbersAnimations.Controller.Reactions
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/Animal Reactions/Stance Reaction"/*, order = 2*/)]
    public class StanceReaction : MReaction
    {
        public Stance_Reaction type = Stance_Reaction.Set;
        [Hide("ShowID",true,false)]
        public StanceID ID;

        protected override void _React(MAnimal animal)
        {
            switch (type)
            {
                case Stance_Reaction.Set:
                    animal.Stance_Set(ID);
                    break;
                case Stance_Reaction.Reset:
                    animal.Stance_Reset();
                    break;
                case Stance_Reaction.Toggle:
                    animal.Stance_Toggle(ID);
                    break;
                case Stance_Reaction.SetDefault:
                    animal.DefaultStance = ID;
                    break;
            }     
        }

        protected override bool _TryReact(MAnimal animal)
        {
            _React(animal);
            return true;
        }

        public enum Stance_Reaction
        {
            /// <summary>Enters a Stance</summary>
            Set,
            /// <summary>Exits a Stance</summary>
            Reset,
            /// <summary>Toggle a Stance</summary>
            Toggle,
            /// <summary>Set the Default stance</summary>
            SetDefault,
        }






        /// 
        /// VALIDATIONS
        /// 

        private void OnEnable() { Validation(); }

        private void OnValidate() { Validation(); }

        [HideInInspector] public bool ShowID;
        private const string reactionName = "Stance → ";

        void Validation()
        {
            fullName = reactionName + type.ToString() + " [" + (ID != null ? ID.name : "None") + "]";
            ShowID = true;

            switch (type)
            {
                case Stance_Reaction.Set:
                    description = "Set a new Stance on an Animal";
                    break;
                case Stance_Reaction.Reset:
                    description = "Reset a Stance on an Animal. (Changes the Stance Valueto the Animal Default Value)";
                    ShowID = false;
                    break;
                case Stance_Reaction.Toggle:
                    description = "Toggle the Stance of an Animal (Between active and default)";
                    break;
                case Stance_Reaction.SetDefault:
                    description = "Set the Default Stance, Used on the Reset and Toogle Stance Methods";
                    break;
            }
        }
    }
}
