using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MalbersAnimations.HAP
{
    /// <summary> Rider Combat Reset Values</summary>
    public partial class RiderCombat 
    {
#if UNITY_EDITOR

        private void Reset()
        {
            BoolVar RiderCombatMode = MalbersTools.GetInstance<BoolVar>("RC Combat Mode");

            MEvent RCCombatMode = MalbersTools.GetInstance<MEvent>("RC Combat Mode");
            MEvent RCEquipedWeapon = MalbersTools.GetInstance<MEvent>("RC Equiped Weapon");
            MEvent SetCameraSettings = MalbersTools.GetInstance<MEvent>("Set Camera Settings");

            if (RiderCombatMode != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCombatMode, RiderCombatMode.SetValue);
            if (RCCombatMode != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCombatMode, RCCombatMode.Invoke);
            if (RCEquipedWeapon != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnEquipWeapon, RCEquipedWeapon.Invoke);

            if (SetCameraSettings != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnAiming, SetCameraSettings.Invoke);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnAimSide, SetCameraSettings.Invoke);
            }

            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(OnEquipWeapon, DisableMountInput, "Attack1");
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(OnEquipWeapon, DisableMountInput, "Attack2");

            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(OnUnequipWeapon, EnableMountInput, "Attack1");
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(OnUnequipWeapon, EnableMountInput, "Attack2");

            var aimer = GetComponent<Aim>();

            if (aimer == null)
            {
                aimer = gameObject.AddComponent<Aim>();
            }
        }

        [ContextMenu("Create Combat Inputs")]
        void ConnectToInput()
        {
            MInput input = GetComponent<MInput>();

            if (input == null)
            { input = gameObject.AddComponent<MInput>(); }

            BoolVar RiderCombatMode = MalbersTools.GetInstance<BoolVar>("RC Combat Mode");

            BoolVar RCWeaponInput = MalbersTools.GetInstance<BoolVar>("RC Weapon Input");
            var inv = GetComponent<MInventory>();

            #region AIM INPUT       

            var AIM = input.FindInput("Aim");
            if (AIM == null)
            {
                AIM = new InputRow("Aim", "Aim", KeyCode.Mouse1, InputButton.Press, InputType.Key);
                input.inputs.Add(AIM);

                AIM.active.Variable = RiderCombatMode;
                AIM.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(AIM.OnInputChanged, SetAim);

                Debug.Log("<B>Aim</B> Input created and connected to RiderCombat.SetAim()");
            }
            #endregion

            #region RiderAttack1 INPUT
            var RCA1 = input.FindInput("RiderAttack1");
            if (RCA1 == null)
            {
                RCA1 = new InputRow("RiderAttack1", "RiderAttack1", KeyCode.Mouse0, InputButton.Press, InputType.Key);
                input.inputs.Add(RCA1);

                RCA1.active.Variable = RiderCombatMode;
                RCA1.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(RCA1.OnInputDown, MainAttack);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(RCA1.OnInputUp, MainAttackReleased);

                Debug.Log("<B>RiderAttack1</B> Input created and connected to RiderCombat.MainAttack() and RiderCombat.MainAttackReleased() ");
            }
            #endregion

            #region RiderAttack2 INPUT
            var RCA2 = input.FindInput("RiderAttack2");
            if (RCA2 == null)
            {
                RCA2 = new InputRow("RiderAttack2", "RiderAttack2", KeyCode.Mouse1, InputButton.Press, InputType.Key);
                input.inputs.Add(RCA2);

                RCA2.active.Variable =RiderCombatMode ;
                RCA2.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(RCA2.OnInputDown, SecondAttack);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(RCA2.OnInputUp, SecondAttackReleased);

                Debug.Log("<B>RiderAttack2</B> Input created and connected to RiderCombat.SecondAttack() and RiderCombat.SecondAttackReleased() ");
            }
            #endregion

            #region Reload INPUT
            var Reload = input.FindInput("Reload");
            if (Reload == null)
            {
                Reload = new InputRow("Reload", "Reload", KeyCode.R, InputButton.Down, InputType.Key);
                input.inputs.Add(Reload);

                Reload.active.Variable = RiderCombatMode;
                Reload.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(Reload.OnInputDown, ReloadWeapon);

                Debug.Log("<B>Reload</B> Input created and connected to RiderCombat.ReloadWeapon() ");
            }
            #endregion

            #region Weapon1 INPUT
            var w1 = input.FindInput("Weapon1");
            if (w1 == null)
            {
                w1 = new InputRow("Weapon1", "Weapon1", KeyCode.Alpha4, InputButton.Down, InputType.Key);
                input.inputs.Add(w1);

                w1.active.Variable = RCWeaponInput;
                w1.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(w1.OnInputDown, Change_Weapon_Holder_Back);

                if (inv)
                {
                    UnityEditor.Events.UnityEventTools.AddIntPersistentListener(w1.OnInputDown, inv.EquipItem, 0);
                }

                Debug.Log("<B>Weapon1</B> Input created and connected to RiderCombat.Change_Weapon_Holder_Back() ");
            }
            #endregion

            #region Weapon2 INPUT
            var w2 = input.FindInput("Weapon2");
            if (w2 == null)
            {
                w2 = new InputRow("Weapon2", "Weapon2", KeyCode.Alpha5, InputButton.Down, InputType.Key);
                input.inputs.Add(w2);

                w2.active.Variable = RCWeaponInput;
                w2.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(w2.OnInputDown, Change_Weapon_Holder_Left);

                if (inv)
                {
                    UnityEditor.Events.UnityEventTools.AddIntPersistentListener(w2.OnInputDown, inv.EquipItem, 1);
                }

                Debug.Log("<B>Weapon2</B> Input created and connected to RiderCombat.Change_Weapon_Holder_Left() ");
            }
            #endregion

            #region Weapon3 INPUT
            var w3 = input.FindInput("Weapon3");
            if (w3 == null)
            {
                w3 = new InputRow("Weapon3", "Weapon3", KeyCode.Alpha6, InputButton.Down, InputType.Key);
                input.inputs.Add(w3);

                w3.active.Variable = RCWeaponInput;
                w3.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(w3.OnInputDown, Change_Weapon_Holder_Right);

                if (inv)
                {
                    UnityEditor.Events.UnityEventTools.AddIntPersistentListener(w3.OnInputDown, inv.EquipItem, 2);
                }

                Debug.Log("<B>Weapon3</B> Input created and connected to RiderCombat.Change_Weapon_Holder_Right() ");
            }
            #endregion

        }


        [ContextMenu("Create Event Listeners")]
        void CreateEventListeners()
        {
            MEvent RCSetAim = MalbersTools.GetInstance<MEvent>("RC Set Aim");
            MEvent RCMainAttack = MalbersTools.GetInstance<MEvent>("RC Main Attack");
            MEvent RCSecondaryAttack = MalbersTools.GetInstance<MEvent>("RC Secondary Attack");

            MEventListener listener = GetComponent<MEventListener>();

            if (listener == null)listener = gameObject.AddComponent<MEventListener>();
            if (listener.Events == null) listener.Events = new List<MEventItemListener>();


            //*******************//
            if (listener.Events.Find(item => item.Event == RCSetAim) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = RCSetAim,
                    useVoid = false,
                    useBool = true,
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseBool, SetAim);
                listener.Events.Add(item);

                Debug.Log("<B>RC Set Aim</B> Added to the Event Listeners");

            }

            //*******************//
            if (listener.Events.Find(item => item.Event == RCMainAttack) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = RCMainAttack,
                    useVoid = false,
                    useBool = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseBool, MainAttack);
                listener.Events.Add(item);

                Debug.Log("<B>RC MainAttack</B> Added to the Event Listeners");
            }

            //*******************//
            if (listener.Events.Find(item => item.Event == RCSecondaryAttack) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = RCSecondaryAttack,
                    useVoid = false,
                    useBool = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseBool, SecondAttack);
                listener.Events.Add(item);

                Debug.Log("<B>RC SecondaryAttack</B> Added to the Event Listeners");
            }

        }
#endif
    }
}