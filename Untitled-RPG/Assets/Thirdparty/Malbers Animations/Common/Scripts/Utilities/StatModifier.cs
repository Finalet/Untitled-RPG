using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations 
{
    [System.Serializable]
    public class StatModifier
    {
        //public bool active = true;
        public StatID ID;
        public StatOption modify = StatOption.None;
        public FloatReference Value = new FloatReference(1);


        /// <summary>There's No ID stat</summary>
        public bool IsNull  { get { return ID == null; } }

      
        /// <summary>Modify the Stats on an animal </summary>
        public void ModifyStat(Stats stats)
        {
            if (ID == null) return;                       //Means there's nothing to modify
            if (modify ==  StatOption.None) return;          //Means there's nothing to modify
            if (stats == null) return;

            Stat s = stats.Stat_Get(ID.ID);    //Get the Stat using the ID

            if (s == null) return;      //Means that there's no Stat

            switch (modify)
            {
                case StatOption.AddValue:
                    s.Modify(Value);
                    break;
                case StatOption.SetValue:
                    s.Value = Value;
                    break;
                case StatOption.SubstractValue:
                    s.Modify(-Value);
                    break;
                case StatOption.ModifyMaxValue:
                    s.ModifyMAX(Value);
                    break;
                case StatOption.SetMaxValue:
                    s.MaxValue = Value;
                    break;
                case StatOption.Degenerate:
                    s.DegenRate = Value;
                    s.Degenerate = true;
                    break;
                case StatOption.StopDegenerate:
                    s.DegenRate = Value;
                    s.Degenerate = false;
                    break;
                case StatOption.Regenerate:
                    s.Regenerate = true;
                    s.RegenRate = Value;
                    break;
                case StatOption.StopRegenerate:
                    s.Regenerate = false;
                    s.RegenRate = Value;
                    break;
                case StatOption.Reset:
                    s.Reset();
                    break;
                case StatOption.ReduceByPercent:
                    s.Modify(-(s.MaxValue * Value / 100));
                    break;
                case StatOption.IncreaseByPercent:
                    s.Modify(s.MaxValue * Value / 100);
                    break;
                default:
                    break;
            }
        }
    }

    public enum StatOption
    {
        None,
        /// <summary>Add to the Stat Value </summary>
        AddValue,
        /// <summary>Set a new Stat Value </summary>
        SetValue,
        /// <summary>Remove to the Stat Value </summary>
        SubstractValue,
        /// <summary>Modify Add or Remove to the Stat MAX Value </summary>
        ModifyMaxValue,
        /// <summary>Set a new Stat MAX Value </summary>
        SetMaxValue,
        /// <summary>Enable the Degeneration </summary>
        Degenerate,
        /// <summary>Disable the Degeneration </summary>
        StopDegenerate,
        /// <summary>Enable the Regeneration </summary>
        Regenerate,
        /// <summary>Disable the Regeneration </summary>
        StopRegenerate,
        /// <summary>Reset the Stat to the Default Min or Max Value </summary>
        Reset,
        /// <summary>Increase the Value of the Stat by a percent</summary>
        ReduceByPercent,
        /// <summary>Increase the Value of the Stat by a percent</summary>
        IncreaseByPercent,

    }
}