using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Used for identify Interactables</summary>
    public interface IInteractable
    {
        /// <summary>Reset the Interactable </summary>
        void ResetInteraction();


        /// <summary>Applies the Interaction Logic</summary>
        void Interact();
    }

    public interface IDestination { }
}