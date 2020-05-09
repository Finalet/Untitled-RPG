using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    public class ScriptableVar : ScriptableObject
    {
#if UNITY_EDITOR
        [TextArea(3, 20)]
        public string Description = "";
#endif
    }
}