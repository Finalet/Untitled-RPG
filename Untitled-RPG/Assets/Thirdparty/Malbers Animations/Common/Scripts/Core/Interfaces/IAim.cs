using System;
using UnityEngine;
namespace MalbersAnimations
{
    /// <summary>Interface used to Check if the Player/Animal is Aiming</summary>
    public interface IAim : IMLayer
    {
        /// <summary>Is the Aiming Logic Active?</summary>
        bool Active { get; set; }

        /// <summary>Direction Vector Stored of the Aiming Logic</summary>
        Vector3 AimDirection { get; }

        Transform MainCamera { get; }

        /// <summary>Check if the Character is in the (Right:true) or (Left: False) side of the Target </summary>
        bool AimingSide { get; }

        /// <summary>RaycastHit Data of the Aim logic</summary>
        RaycastHit AimHit { get; }

        /// <summary>Forced Target on the Aiming Logic</summary>
        Transform AimTarget { set; get; }

        /// <summary>Returns the origin of the Aim Logic</summary>
        Transform AimOrigin { set; get; }

        /// <summary>Returns the Point of the Aimer</summary>
        Vector3 AimPoint { get; }

        /// <summary>Forced the Aim to Ignore a Transform</summary>
        Transform IgnoreTransform { set; get; }

        /// <summary>Forced Target on the Aiming Logic</summary>
        void SetTarget(Transform value);

        /// <summary>Calculates the Target Horizontal Angle Normalized </summary>
        float HorizontalAngle { get; }

        /// <summary>Calculates the Target Horizontal Angle Normalized </summary>
        float VerticalAngle { get; }

        /// <summary>Aim Side set on the Aimer (-1:Left; 1:Right; 0:No Aim)</summary>
        AimSide AimSide { set; get; }

        void ExitAim();

        void EnterAim();

        /// <summary>Calculate the Aiming Logic</summary>
        void Calculate();
    }

    public interface IAimTarget
    {
        /// <summary>Is the aim Target an Ai Asist</summary>
        bool AimAssist { get; }

        /// <summary>Reference for the Aim Point</summary>
        Transform AimPoint { get; }

        /// <summary>Is the target been aimed by the Aim Ray of the Aim Script</summary>
        void IsBeenAimed(bool enter);
    }
}