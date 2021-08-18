namespace MalbersAnimations
{
    public enum InputType { Input, Key }

    public enum WayPointType { Ground, Air, Water }

    public enum FieldColor { Red, Green, Blue, Magenta, Cyan, Yellow, Orange, Gray }

    public enum InputButton { Press = 0, Down = 1, Up = 2, LongPress = 3, DoubleTap = 4 , Toggle = 5}

    public enum StateTransition { First = 0, Last = 1 }

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

    public enum ComparerString
    {
        Equal = 0,
        NotEqual = 1,
        Empty = 2,
    }


    public enum TypeMessage
    {
        Bool = 0,
        Int = 1,
        Float = 2,
        String = 3,
        Void = 4,
        IntVar = 5,
        Transform = 6,
        GameObject = 7,
        Component = 8,
    }

    public enum MStatus
    {
        None = 0,
        Prepared = 1,
        Playing = 2,
        Completed = 3,
        Interrupted = 4,
        ForceExit = 5, 
    }

    public enum EEnterExit
    {
        Enter = 1,
        Exit = 2,
    }

    public enum AxisDirection { None, Right, Left, Up, Down, Forward, Backward }

    [System.Flags]
    public enum UpdateMode                                          // The available methods of updating are:
    {
        Update = 1,
        FixedUpdate = 2,                                            // Update in FixedUpdate (for tracking rigidbodies).
        LateUpdate = 4,                                             // Update in LateUpdate. (for tracking objects that are moved in Update)
    }

    public enum UpdateType                                      // The available methods of updating are:
    {
        FixedUpdate,                                            // Update in FixedUpdate (for tracking rigidbodies).
        LateUpdate,                                             // Update in LateUpdate. (for tracking objects that are moved in Update)
    }

    public enum AimSide {None = 0, Left = 1, Right = 2 }

    public static class WSound
    {
        /// <summary>[0] Draw Weapon Sound</summary>
        public static int Equip => 0;
        /// <summary>[1] Store Weapon Sound</summary>
        public static int Store => 1;
        /// <summary>[2] Draw Weapon Sound</summary>
        public static int Fire => 2;
        /// <summary>[3] Reload Weapon Sound</summary>
        public static int Reload => 3;
        /// <summary>[4] Empty Weapon Sound</summary>
        public static int Empty => 4;
        /// <summary>[4] Charge Weapon Sound</summary>
        public static int Charge => 5;
    }

    /// <summary>Weapons Actions. -100 to 100     +(Right Hand Actions)  -(Left Hand Actions) </summary>
    public static class WA
    {
        /// <summary>[0] No Weapon is on the Hands of the Character</summary>
        public static int None => 0;

        /// <summary>[1000] The Weapon is resting in the Hand</summary>
        public static int Idle => 100;

        /// <summary>[1001] The Weapon is firing a Projectile</summary>
        public static int Fire_Projectile = 101;

        /// <summary>[1001] The Weapon is Released</summary>
        public static int Release = 101;

        /// <summary>[99] The Weapon is draw for the RIGHT Side (Hostler) </summary>
        public static int Draw => 99;

        /// <summary>[98] The Weapon is stored to  the RIGHT Side (Hostler) </summary>
        public static int Store => 98;

        /// <summary>[97] The Weapon is aiming???</summary>
        public static int Aim => 97;
 
        /// <summary>[103] The Character is reloading the weapon with the RIGHT Hand</summary>
        public static int Reload => 96;

        /// <summary>[103] The Character is reloading the weapon with the RIGHT Hand</summary>
        public static int Preparing => 95;
        /// <summary>[95] The Character is Holding/Charging/Preparing the Weapon. E.g. Bow</summary>
        public static int Ready => 95;


        public static string WValue(int v)
        {
            switch (v)
            {
                case 0: return "None";
                case 95: return "Ready|Preparing";
                case 96: return "Reload";
                case 97: return "Aim";
                case 98: return "Store";
                case 99: return "Draw";
                case 100: return "Idle";
                case 101: return "Fire_Projectile";
                default: return v.ToString(); ;
            }
        }
    }
}