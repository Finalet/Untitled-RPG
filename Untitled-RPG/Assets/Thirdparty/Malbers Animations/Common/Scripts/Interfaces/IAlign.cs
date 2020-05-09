using UnityEngine;
namespace MalbersAnimations
{
    /// <summary>Interface used to Align a Game object to Another</summary>
    public interface IAlign
    {
        /// <summary>Is the Aiming Logic Active?</summary>
        bool Active { get; set; }

        ///// <summary>Is the Aiming Logic Active?</summary>
        //float Radius { get; set; }

        /// <summary>Aimer Origin Transform</summary>
        Transform MainPoint { get; }

        /// <summary>Align Logic</summary>
        void Align(Transform Target);


        /// <summary>Align Logic using GameObject</summary>
        void Align(GameObject Target);
    }
}