namespace MalbersAnimations 
{
    /// <summary> Interface used to Identify the Riding Component </summary>
    public interface IRider
    {
        /// <summary>Returns true if the Rider is on the horse and Mount animations are finished</summary>
        bool IsRiding { get; }
        /// <summary>True: Rider starts Mounting. False: Rider starts Dismounting This value goes to the Animator </summary>
        bool Mounted { get; }
        /// <summary>This is true (Finish Mounting) False (Finish Dismounting)</summary>
        bool IsOnHorse { get; }
        /// <summary> Check if can mount an Animal </summary>
        bool CanMount { get; }
        /// <summary>Check if we can dismount the Animal</summary>
        bool CanDismount { get; }
        /// <summary>Returns true if the Rider is between the Start and the End of the Mount Animations</summary>
        bool IsMounting { get; }
       
        /// <summary>Returns true if the Rider is between the Start and the End of the Dismount Animations</summary>
        bool IsDismounting { get; }

        /// <summary>Is the Rider using Aiming (Used for the Straight Spine Option)</summary>
        bool IsAiming { get; set; } 

        /// <summary>Animal Input Script</summary>
        IInputSource MountInput { get; }
    }
}


