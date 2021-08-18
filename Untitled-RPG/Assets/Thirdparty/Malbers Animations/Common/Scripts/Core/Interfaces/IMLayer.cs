using UnityEngine;

namespace MalbersAnimations
{
    /// <summary> Interface to check Layers and Layer Interactions</summary>
    public interface IMLayer
    {
        /// <summary>Layers to Interact</summary>
        LayerMask Layer { get; set; }

        /// <summary>What to do with the Triggers ... Ignore them? Use them?</summary>
        QueryTriggerInteraction TriggerInteraction { get; set; }
    }

    ///// <summary>Used to find components with OnTrigerEnter OnTrigger Exit </summary>
    //public interface ITriggerInteract
    //{
    //    /// <summary>Same as OnTrigger Enter</summary>
    //    void EnterTrigger(Collider trigger, int TriggerID);
    //    /// <summary>Same as OnTrigger Exit</summary>
    //    void ExitTrigger(Collider trigger, int TriggerID);
    //}
}