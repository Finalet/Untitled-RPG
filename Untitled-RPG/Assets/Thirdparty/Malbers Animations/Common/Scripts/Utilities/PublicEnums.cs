namespace MalbersAnimations
{
    public enum InputType
    {
        Input, Key
    }

    public enum WayPointType
    {
        Ground, Air, Water
    }


    public enum FieldColor
    {
        Red, Green, Blue, Magenta, Cyan, Yellow, Orange, Gray
    }

    public enum InputButton
    {
        Press = 0, Down = 1, Up = 2, LongPress = 3, DoubleTap =4
    }

    public enum StateTransition
    {
        First = 0,
        Last = 1
    }

    public enum ComparerInt
    {
        Equal = 0,
        Greater = 1,
        Less = 2,
        NotEqual = 3,
    }

    public enum ComparerBool
    {
        Equal = 0,
        NotEqual = 1,
    }


    public enum TypeMessage
    {
        Bool = 0,
        Int = 1,
        Float = 2,
        String = 3,
        Void = 4,
        IntVar = 5,
        Transform = 6
    }

    ////This is mainly use for the Rider Combat Animator nothing more
    //public enum WeaponType 
    //{
    //    None = 0,
    //    Melee = 1,
    //    Bow = 2,
    //    Spear = 3,
    //    Pistol = 4,
    //    Rifle = 5
    //}

    public enum WeaponHolder
    {
        None = 0,
        Left = 1,
        Right = 2,
        Back = 3
    }

    public enum MStatus
    {
        None = 0,
        Prepared = 1,
        Playing = 2,
        Completed = 3,
        Interrupted = 4,
    }

    public enum EEnterExit
    {
        Enter = 1,
        Exit = 2,
    }

    public enum AxisDirection
    {
        None,
        Right,
        Left,
        Up,
        Down,
        Forward,
        Backward
    }

    public enum UpdateMode                                          // The available methods of updating are:
    {
        Update = 1,
        FixedUpdate = 2,                                            // Update in FixedUpdate (for tracking rigidbodies).
        LateUpdate = 4,                                             // Update in LateUpdate. (for tracking objects that are moved in Update)
    }


    /// <summary>Weapons Actions ... Positive values are Attacks</summary>
    public enum WA
    {
        None = 0,
        DrawFromRight = -1,
        DrawFromLeft = -2,
        StoreToRight = -3,
        StoreToLeft = -7,
        /// <summary>The Weapon is resting in the Hand</summary>
        Idle = -4,
        AimRight = -5,
        AimLeft = -6,
        ReloadRight  = -8,
        ReloadLeft = -9,
        Hold = -10,         //Used for the Bow when holding the String
        MeleeAttack = 100,         //Set That is Doing a Melee Attack
        //Equip = -100,
        //Unequip = -101,

        //Positive Values are the Attack IDs
        
        //Melee
        Atk_RSide_RHand_Forward = 1,                //Attack Right Side with Right Hand Forward
        Atk_RSide_RHand_Backward = 2,               //Attack Right Side with Right Hand Backward

        Atk_LSide_RHand_Forward = 3,                //Attack Left Side with Right Hand Forwards
        Atk_LSide_RHand_Backward = 4,               //Attack Left Side with Right Hand Backward

        Atk_RSide_LHand_Forward = 5,                //Attack Right Side with Left Hand Forward
        Atk_RSide_LHand_Backward = 6,               //Attack Right Side with Left Hand Backward

        Atk_LSide_LHand_Forward = 7,                //Attack Left Side with Left Hand Forward
        Atk_LSide_LHand_Backward = 8,               //Attack Left Side with Left Hand Backward

        //Ranged
        Fire_Proyectile = 9,
    }
}