using MalbersAnimations.Events;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Basic Entries needed to use Malbers Input Component</summary>
    public interface IInputSource
    {
        void Enable(bool val);
        InputRow GetInput(string input);
        void EnableInput(string input);
        void DisableInput(string input);
    }
   
    

    /// <summary>
    /// Common Entries for all Inputs on the Store
    /// </summary>
    public interface IInputSystem
    {
        float GetAxis(string Axis);
        float GetAxisRaw(string Axis);
        bool GetButtonDown(string button);
        bool GetButtonUp(string button);
        bool GetButton(string button);
    }
     
    /// <summary>Function Needed for moving Characters</summary>
    public interface ICharacterMove
    {
        /// <summary>Move the Character using a Direction</summary>
        void Move(Vector3 move);

        /// <summary>Sends to the the Raw Input Axis </summary>
        void SetInputAxis(Vector3 inputAxis);
    }


    /// <summary> Default Unity Input</summary>
    public class DefaultInput : IInputSystem
    {
        public float GetAxis(string Axis)
        {
            return Input.GetAxis(Axis);
        }

        public float GetAxisRaw(string Axis)
        {
            return Input.GetAxisRaw(Axis);
        }

        public bool GetButton(string button)
        {
            return Input.GetButton(button);
        }

        public bool GetButtonDown(string button)
        {
            return Input.GetButtonDown(button);
        }

        public bool GetButtonUp(string button)
        {
            return Input.GetButtonUp(button);
        }

        /// <summary>
        /// This Gets the Current Input System that is being used... Unity's, CrossPlatform or Rewired
        /// </summary>
        public static IInputSystem GetInputSystem(string PlayerID = "")
        {
            IInputSystem Input_System = null;

#if !CROSS_PLATFORM_INPUT
            Input_System = new DefaultInput();             //Set it as default the Unit Input System
#else
            Input_System = new CrossPlatform();          //Set the Input System to the UNITY STANDARD ASSET CROSS PLATFORM
#endif
#if REWIRED
           Rewired.Player player = Rewired.ReInput.players.GetPlayer(PlayerID);
            if (player != null)
                Input_System = new RewiredInput(player);
            else
                Debug.LogError("NO REWIRED PLAYER WITH THE ID:" + PlayerID + " was found");
#endif
#if OOTII_EI
            Input_System = new EasyInput();
#endif
            return Input_System;
        }
    }

#if CROSS_PLATFORM_INPUT
    public class CrossPlatform : IInputSystem
    {
        public float GetAxis(string Axis)
        {
            return UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetAxis(Axis);
        }

        public float GetAxisRaw(string Axis)
        {
            return UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetAxisRaw(Axis);
        }

        public bool GetButton(string button)
        {
            return UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetButton(button);
        }

        public bool GetButtonDown(string button)
        {
            return UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetButtonDown(button);
        }

        public bool GetButtonUp(string button)
        {
            return UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetButtonUp(button);
        }
    }
#endif

#if REWIRED
    public class RewiredInput : IInputSystem
    {
        Rewired.Player player;

        public float GetAxis(string Axis)
        {
            return player.GetAxis(Axis);
        }

        public float GetAxisRaw(string Axis)
        {
            return player.GetAxisRaw(Axis);
        }

        public bool GetButton(string button)
        {
            return player.GetButton(button);
        }

        public bool GetButtonDown(string button)
        {
            return player.GetButtonDown(button);
        }

        public bool GetButtonUp(string button)
        {
            return player.GetButtonUp(button);
        }

        public RewiredInput(Rewired.Player player)
        {
            this.player = player;
        }
    }
#endif

#if OOTII_EI
    public class EasyInput : IInputSystem
    {
        public float GetAxis(string Axis)
        {
            return com.ootii.Input.InputManager.GetValue(Axis);
        }

        public float GetAxisRaw(string Axis)
        {
           return GetAxis(Axis);
        }

        public bool GetButton(string button)
        {
            return com.ootii.Input.InputManager.IsPressed(button);
        }

        public bool GetButtonDown(string button)
        {
            return com.ootii.Input.InputManager.IsJustPressed(button);
        }

        public bool GetButtonUp(string button)
        {
            return com.ootii.Input.InputManager.IsJustReleased(button);
        }
    }
#endif

}