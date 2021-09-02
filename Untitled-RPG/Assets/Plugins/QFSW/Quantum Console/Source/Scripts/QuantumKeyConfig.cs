using UnityEngine;

namespace QFSW.QC
{
    [CreateAssetMenu(fileName = "Untitled Key Config", menuName = "Quantum Console/Key Config")]
    public class QuantumKeyConfig : ScriptableObject
    {
        public KeyCode SubmitCommandKey = KeyCode.Return;
        public KeyCode SecondarySubmitCommandKey = KeyCode.KeypadEnter;
        public ModifierKeyCombo ShowConsoleKey = KeyCode.None;
        public ModifierKeyCombo HideConsoleKey = KeyCode.None;
        public ModifierKeyCombo ToggleConsoleVisibilityKey = KeyCode.Escape;

        public ModifierKeyCombo AutocompleteKey = KeyCode.Tab;
        public ModifierKeyCombo SuggestNextCommandKey = KeyCode.Tab;
        public ModifierKeyCombo SuggestPreviousCommandKey = new ModifierKeyCombo { Key = KeyCode.Tab, Shift = true };

        // public KeyCode NextCommandKey = KeyCode.UpArrow;
        // public KeyCode PreviousCommandKey = KeyCode.DownArrow;

        public ModifierKeyCombo NextCommandKey = KeyCode.UpArrow;
        public ModifierKeyCombo PreviousCommandKey = KeyCode.DownArrow;

        public ModifierKeyCombo CancelActionsKey = new ModifierKeyCombo { Key = KeyCode.C, Ctrl = true };
    }
}
