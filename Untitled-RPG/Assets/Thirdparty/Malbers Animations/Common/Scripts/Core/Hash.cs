using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Faster way to work with Animator Controller Parameters</summary>
    public static class Hash 
    {
        public static readonly int Vertical = Animator.StringToHash("Vertical");
        public static readonly int Horizontal = Animator.StringToHash("Horizontal");
        public static readonly int UpDown = Animator.StringToHash("UpDown");

        public static readonly int Stand = Animator.StringToHash("Stand");
        public static readonly int Grounded = Animator.StringToHash("Grounded");

        public static readonly int Jump = Animator.StringToHash("Jump");

        public static readonly int Dodge = Animator.StringToHash("Dodge");
        public static readonly int Fall = Animator.StringToHash("Fall");
        public static readonly int Type = Animator.StringToHash("Type");


        public static readonly int Slope = Animator.StringToHash("Slope");

        public static readonly int Shift = Animator.StringToHash("Shift");

        public static readonly int Fly = Animator.StringToHash("Fly");
        public static readonly int Locomotion = Animator.StringToHash("Locomotion");

        public static readonly int Attack1 = Animator.StringToHash("Attack1");
        public static readonly int Attack2 = Animator.StringToHash("Attack2");

        public static readonly int Death = Animator.StringToHash("Death");

        public static readonly int Damaged = Animator.StringToHash("Damaged");
        public static readonly int Stunned = Animator.StringToHash("Stunned");

        public static readonly int IDInt = Animator.StringToHash("IDInt");
        public static readonly int IDFloat = Animator.StringToHash("IDFloat");

        public static readonly int Swim = Animator.StringToHash("Swim");
        public static readonly int Underwater = Animator.StringToHash("Underwater");

        public static readonly int IDAction = Animator.StringToHash("IDAction");
        public static readonly int Action = Animator.StringToHash("Action");


        public static readonly int Null = Animator.StringToHash("Null");
        public static readonly int Empty = Animator.StringToHash("Empty");


        public static readonly int State = Animator.StringToHash("State");
        public static readonly int Stance = Animator.StringToHash("Stance");
        public static readonly int Mode = Animator.StringToHash("Mode");
        public static readonly int StateTime = Animator.StringToHash("StateTime");



        ////---------------------------HAP-----------------------------------------


        //public readonly static int IKLeftFoot = Animator.StringToHash("IKLeftFoot");
        //public readonly static int IKRightFoot = Animator.StringToHash("IKRightFoot");

        //public readonly static int Mount = Animator.StringToHash("Mount");
        //public readonly static int MountSide = Animator.StringToHash("MountSide");

        //public readonly static int Tag_Mounting= Animator.StringToHash("Mounting");
        //public readonly static int Tag_Unmounting = Animator.StringToHash("Unmounting");
        //public readonly static int Tag_Mount= Animator.StringToHash("Mount");
        //public readonly static int Tag_Dismount = Animator.StringToHash("Dismount");

    }

    public static class Int_ID
    {
        /// <summary>Any Ability Can be Activated = 0 </summary>
        public readonly static int Available = 0;
        /// <summary>The Ability is Interrupted = -2 </summary>
        public readonly static int Interrupted = -2;
        /// <summary>The Ability is Loopable = -1 </summary>
        public readonly static int Loop = -1;
        /// <summary>The Ability is Play one Time only = 1 </summary>
        public readonly static int OneTime = 1;
        /// <summary>Status of the States Allow Exit = 1 </summary>
        public readonly static int AllowExit = 1;
    }

    public static class ModeEnum
    {
        /// <summary>Mode ID for Attack1: 1 </summary>
        public readonly static int Attack1 = 1;
        /// <summary>Mode ID for Attack2: 2 </summary>
        public readonly static int Attack2 = 2;
        /// <summary>Mode ID for Damage: 3 </summary>
        public readonly static int Damage = 3;
        /// <summary>Mode ID for Action: 4 </summary>
        public readonly static int Action = 4;
        /// <summary>Mode ID for Dodge: 5 </summary>
        public readonly static int Dodge = 5;
        /// <summary>Mode ID for Attack1Air: 6 </summary>
        public readonly static int Attack1Air = 6;
        /// <summary>Mode ID for PickUp: 7 </summary>
        public readonly static int PickUp = 7;
    }

    public static class StateEnum
    {
        /// <summary>States ID for Idle: 0</summary>
        public readonly static int Idle = 0;
        /// <summary>States ID for Locomotion: 1</summary>
        public readonly static int Locomotion = 1;
        /// <summary>States ID for Jump: 2</summary>
        public readonly static int Jump = 2;
        /// <summary>States ID for Fall: 3</summary>
        public readonly static int Fall = 3;
        /// <summary>States ID for Swim: 4</summary>
        public readonly static int Swim = 4;
        /// <summary>States ID for UndweWater: 5</summary>
        public readonly static int UnderWater = 5;
        /// <summary>States ID for Fly: 6</summary>
        public readonly static int Fly = 6;
        /// <summary>States ID for Climb: 7</summary>
        public readonly static int Climb = 7;
        /// <summary>States ID for Slide: 8</summary>
        public readonly static int Slide = 8;
        /// <summary>States ID for Death: 10</summary>
        public readonly static int Death = 10;
    }

    public static class StanceEnum
    {
        /// <summary>States ID for Default: 0</summary>
        public readonly static int Default = 0;
        /// <summary>States ID for Sneak: 1</summary>
        public readonly static int Sneak = 1;
        /// <summary>States ID for Combat: 2</summary>
        public readonly static int Combat = 2;
        /// <summary>States ID for Wounded: 3</summary>
        public readonly static int Wounded = 3;
        /// <summary>States ID for Stand: 4</summary>
        public readonly static int Stand = 4;
        /// <summary>States ID for Strafe: 5</summary>
        public readonly static int Strafe = 5;
        /// <summary>States ID for Roll: 6</summary>
        public readonly static int Roll = 6;
        /// <summary>States ID for Crouch: 7</summary>
        public readonly static int Crouch = 7;
    } 
}