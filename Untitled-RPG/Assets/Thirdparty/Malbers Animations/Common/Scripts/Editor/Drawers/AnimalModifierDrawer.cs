using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(AnimalModifier))]
    public class AnimalModifierDrawer : PropertyDrawer
    {

        private float Division;
        int activeProperties;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);

            position.x += 2;
            position.width -= 2;

            position.y += 2;
            position.height -= 2;


            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;

            #region Serialized Properties
            var modify = property.FindPropertyRelative("modify");
            var Colliders = property.FindPropertyRelative("Colliders");
            var RootMotion = property.FindPropertyRelative("RootMotion");
            var Sprint = property.FindPropertyRelative("Sprint");
            var Gravity = property.FindPropertyRelative("Gravity");
            var OrientToGround = property.FindPropertyRelative("OrientToGround");
            var CustomRotation = property.FindPropertyRelative("CustomRotation");
            var IgnoreLowerStates = property.FindPropertyRelative("IgnoreLowerStates");
           var AdditivePositionSpeed = property.FindPropertyRelative("AdditivePosition");
          //  var AdditiveRotation = property.FindPropertyRelative("AdditiveRotation");
            var Grounded = property.FindPropertyRelative("Grounded");
            var FreeMovement = property.FindPropertyRelative("FreeMovement");
            var Persistent = property.FindPropertyRelative("Persistent");
            var LockInput = property.FindPropertyRelative("LockInput");
            var LockMovement = property.FindPropertyRelative("LockMovement");
            #endregion

            var line = position;
            var lineLabel = line;
            line.height = height;

            var foldout = lineLabel;
            foldout.width = 10;
            foldout.x += 10;

            EditorGUIUtility.labelWidth = 16;
            EditorGUIUtility.labelWidth = 0;


#if UNITY_2018_1_OR_NEWER
            modify.intValue = (int)(modifier)EditorGUI.EnumFlagsField(line, label, (modifier)(modify.intValue));
#else
            modify.intValue = (int)(modifier)EditorGUI.EnumMaskField(line, label, (modifier)(modify.intValue));
#endif


            line.y += height + 2;
            Division = line.width / 3;

            activeProperties = 0;
            int ModifyValue = modify.intValue;

            if (Modify(ModifyValue, modifier.RootMotion))
                DrawProperty(ref line, RootMotion, new GUIContent("RootMotion", "Root Motion:\nEnable/Disable the Root Motion on the Animator"));

            if (Modify(ModifyValue, modifier.Sprint))
                DrawProperty(ref line, Sprint, new GUIContent("Sprint", "Sprint:\nEnable/Disable Sprinting on the Animal"));

            if (Modify(ModifyValue, modifier.Gravity))
                DrawProperty(ref line, Gravity, new GUIContent("Gravity", "Gravity:\nEnable/Disable the Gravity on the Animal. Used when is falling or jumping"));

            if (Modify(ModifyValue, modifier.Grounded))
                DrawProperty(ref line, Grounded, new GUIContent("Grounded", "Grounded\nEnable/Disable if the Animal is Grounded (If True it will  calculate  the Alignment for Position with the ground ). If False:  Orient to Ground is also disabled."));

            if (Modify(ModifyValue, modifier.CustomRotation))
                DrawProperty(ref line, CustomRotation, new GUIContent("Custom Rot", "Custom Rotation: \nEnable/Disable the Custom Rotations (Used in Fly, Climb, UnderWater, Swim), This will disable Orient to Ground"));

            EditorGUI.BeginDisabledGroup(CustomRotation.boolValue || !Grounded.boolValue);
            if (Modify(ModifyValue, modifier.OrientToGround))
                DrawProperty(ref line, OrientToGround, new GUIContent("Orient Ground", "Orient to Ground:\nEnable/Disable the Rotation Alignment while grounded. (If False the Animal will be aligned with the Up Vector)"));
            EditorGUI.EndDisabledGroup();

            if (Modify(ModifyValue, modifier.IgnoreLowerStates))
                DrawProperty(ref line, IgnoreLowerStates, new GUIContent("Ignore Lower States", "States below will not be able to try to activate themselves"));

            if (Modify(ModifyValue, modifier.Persistent))
                DrawProperty(ref line, Persistent, new GUIContent("Persistent", "Persistent:\nEnable/Disable is Persistent on the Active State ... meaning the Animal will not Try to activate any States"));

            if (Modify(ModifyValue, modifier.LockMovement))
                DrawProperty(ref line, LockMovement, new GUIContent("Lock Move", "Lock Movement:\nLock the Movement on the Animal, does not include Action Inputs for Attack, Jump, Action, etc"));

            if (Modify(ModifyValue, modifier.LockInput))
                DrawProperty(ref line, LockInput, new GUIContent("Lock Input", "Lock Input:\nLock the Inputs, (Jump, Attack, etc) does not include Movement Input (WASD or Axis Inputs)"));
            if (Modify(ModifyValue, modifier.Colliders))
                DrawProperty(ref line, Colliders, new GUIContent("Colliders", "Colliders:\nEnable Disable All the Colliders on the animal... Used for immunity on the animal or entering narrow spaces"));

            if (Modify(ModifyValue, modifier.AdditivePositionSpeed))
                DrawProperty(ref line, AdditivePositionSpeed, new GUIContent("+ Pos Speed", "Additive Position Speed:\nEnable/Disable Additive Position used on the Speed Modifiers or Gravity"));


            if (Modify(ModifyValue, modifier.FreeMovement))
                DrawProperty(ref line, FreeMovement, new GUIContent("Free Move", "Free Movement:\nEnable/Disable the Free Movement... This allow to Use the Pitch direction vector and the Rotator Transform"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private void DrawProperty(ref Rect rect, SerializedProperty property, GUIContent content)
        {
            Rect splittedLine = rect;
            splittedLine.width = Division - 1;

            splittedLine.x += (Division * (activeProperties % 3)) + 1;

           // property.boolValue = GUI.Toggle(splittedLine, property.boolValue, content, EditorStyles.miniButton);
            property.boolValue = EditorGUI.ToggleLeft(splittedLine, content, property.boolValue);

            activeProperties++;
            if (activeProperties % 3 == 0)
            {
                rect.y += EditorGUIUtility.singleLineHeight + 2;
            }
        }


        private bool Modify(int modify, modifier modifier)
        {
            return ((modify & (int)modifier) == (int)modifier);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int activeProperties = 0;

            var modify = property.FindPropertyRelative("modify");
            int ModifyValue = modify.intValue;

            if (Modify(ModifyValue, modifier.RootMotion)) activeProperties++;
            if (Modify(ModifyValue, modifier.Sprint)) activeProperties++;
            if (Modify(ModifyValue, modifier.Gravity)) activeProperties++;
            if (Modify(ModifyValue, modifier.Grounded)) activeProperties++;
            if (Modify(ModifyValue, modifier.CustomRotation)) activeProperties++;
            if (Modify(ModifyValue, modifier.OrientToGround)) activeProperties++;
            if (Modify(ModifyValue, modifier.IgnoreLowerStates)) activeProperties++;
            if (Modify(ModifyValue, modifier.AdditivePositionSpeed)) activeProperties++;
           // if (Modify(ModifyValue, modifier.AdditiveRotationSpeed)) activeProperties++;
            if (Modify(ModifyValue, modifier.Persistent)) activeProperties++;
            if (Modify(ModifyValue, modifier.FreeMovement)) activeProperties++;
            if (Modify(ModifyValue, modifier.LockMovement)) activeProperties++;
            if (Modify(ModifyValue, modifier.LockInput)) activeProperties++;
            if (Modify(ModifyValue, modifier.Colliders)) activeProperties++;

            float lines = (int)((activeProperties+2)/3) +1;

            return base.GetPropertyHeight(property, label) * lines + (2 * lines);
        }
    }
}