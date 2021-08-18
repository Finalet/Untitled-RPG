using System.Collections;
using UnityEngine;

namespace MalbersAnimations.Controller.Reactions
{
    /// <summary> Reaction Script for Making the Animal do something </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Stat",  fileName = "New Stat Modifier",order = -100)]
    public class ModifyStatSO : ScriptableObject
    {
        public StatModifier modifier;
        /// <summary>Instant Reaction ... without considering Active or Delay parameters</summary>
        public void Modify(Stats stats) => modifier.ModifyStat(stats);
        public void Modify(Component stats) => Modify(stats.FindComponentInRoot<Stats>());
        public void Modify(GameObject stats) => Modify(stats.FindComponentInRoot<Stats>());
    }
}