﻿using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Change Speed")]
    public class ChangeSpeedTask : MTask
    {
        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;

        public string SpeedSet = "Ground";
        public IntReference SpeedIndex = new IntReference(3);

        public override void StartTask(MAnimalBrain brain, int index)
        {
            switch (affect)
            {
                case Affected.Self:
                    ChangeSpeed(brain.Animal);
                    brain.AIMovement.SetDefaultGroundIndex(SpeedIndex.Value);
                    break;
                case Affected.Target:
                    ChangeSpeed(brain.TargetAnimal);
                    brain.TargetAnimal?.GetComponent<MAnimalAIControl>()?.SetDefaultGroundIndex(SpeedIndex.Value);
                    break;
            }
            brain.TaskDone(index); //Set Done to this task
        }

        public void ChangeSpeed(MAnimal animal)
        {
            animal?.SpeedSet_Set_Active(SpeedSet, SpeedIndex);
        }


        
        void Reset()
        { Description = "Change the Speed on the Animal"; }
    }
}