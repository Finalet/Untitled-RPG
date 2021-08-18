using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    public class Holsters : MonoBehaviour
    {
        public HolsterID DefaultHolster;
        public List<Holster> holsters = new List<Holster>();
        public float HolsterTime = 0.3f;

        public Holster ActiveHolster { get; set; }

        /// <summary> Used to change to the Next/Previus Holster</summary>
        private int ActiveHolsterIndex;

        private void Start()
        {
            for (int i = 0; i < holsters.Count; i++)
            {
                holsters[i].Index = i;
            }

            SetActiveHolster(DefaultHolster);
            PrepareWeapons();
        }

        private void PrepareWeapons()
        {
            foreach (var h in holsters)
                h.PrepareWeapon();
        }

        public void SetActiveHolster(int ID)
        {
            ActiveHolster = holsters.Find(x => x.GetID == ID);
            ActiveHolsterIndex = ActiveHolster != null ? ActiveHolster.Index : 0;
        }

        public void SetNextHolster()
        {
            ActiveHolsterIndex = (ActiveHolsterIndex + 1) % holsters.Count;
            ActiveHolster = holsters[ActiveHolsterIndex];
        }

        public void EquipWeapon(GameObject newWeapon)
        {
            var nextWeapon = newWeapon.GetComponent<MWeapon>();

            if (nextWeapon != null)
            {
                var holster = holsters.Find(x=> x.ID ==  nextWeapon.HolsterID);

                if (holster != null)
                {
                    Debug.Log(holster.ID.name +"   "+ holster.Weapon);

                    if (holster.Weapon != null)
                    {
                        if (!holster.Weapon.IsEquiped)
                        {
                           var IsCollectable = holster.Weapon.GetComponent<ICollectable>();

                            IsCollectable.Drop();
                            if (holster.Weapon)
                            //Destroy(holster.Weapon.gameObject);
                            holster.Weapon = null;
                        }
                        else
                        {
                            //DO THE WEAPON EQUIPED STUFF
                            return;
                        }
                    }


                    if (newWeapon.IsPrefab()) newWeapon = Instantiate(newWeapon);       //if is a prefab instantiate on the scene

                    newWeapon.transform.parent = holster.Transform;                     //Parent the weapon to his original holster          
                    newWeapon.transform.SetLocalTransform(nextWeapon.HolsterOffset);

                    holster.Weapon = nextWeapon;
                }
            }
        }

        public void SetPreviousHolster()
        {
            ActiveHolsterIndex = (ActiveHolsterIndex - 1) % holsters.Count;
            ActiveHolster = holsters[ActiveHolsterIndex];
        }

        [ContextMenu("Validate Holster Child Weapons")]
        internal void ValidateWeaponsChilds()
        {
            foreach (var h in holsters)
            {
                if (h.Weapon == null && h.Transform != null && h.Transform.childCount > 0 )
                {
                    h.Weapon = (h.Transform.GetChild(0).GetComponent<MWeapon>()); ;
                }
            }
        }
    }

#region Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(Holsters))]
    public class HolstersEditor : Editor
    {
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));
        public static GUIStyle StyleGreen => MTools.Style(new Color(0f, 1f, 0.5f, 0.3f));

        private SerializedProperty holsters, DefaultHolster, /*m_active_Holster, */HolsterTime;
        private ReorderableList holsterReordable;

        private Holsters m;

        private void OnEnable()
        {
            m = (Holsters)target;

            holsters = serializedObject.FindProperty("holsters");
            DefaultHolster = serializedObject.FindProperty("DefaultHolster");
            HolsterTime = serializedObject.FindProperty("HolsterTime");


            holsterReordable = new ReorderableList(serializedObject, holsters, true, true, true, true)
            {
                drawElementCallback = DrawHolsterElement,
                drawHeaderCallback = DrawHolsterHeader
            };
        }

        private void DrawHolsterHeader(Rect rect)
        {
            var IDRect = new Rect(rect);
            IDRect.height = EditorGUIUtility.singleLineHeight;
            IDRect.width *= 0.5f;
            IDRect.x += 18;
            EditorGUI.LabelField(IDRect, "   Holster ID");
            IDRect.x += IDRect.width-10;
            IDRect.width -= 18;
            EditorGUI.LabelField(IDRect, "   Holster Transform ");

            var buttonRect = new Rect(rect) { x = rect.width - 30, width = 55 , y = rect.y-1, height = EditorGUIUtility.singleLineHeight +3};

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0, 0.5f, 1f, 0.6f);
            if (GUI.Button(buttonRect,new GUIContent("Weapon","Check for Weapons on the Holsters"), EditorStyles.miniButton))
            {
                m.ValidateWeaponsChilds();
            }
            GUI.backgroundColor = oldColor;
        }

        private void DrawHolsterElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;

            var holster = holsters.GetArrayElementAtIndex(index);

            var ID = holster.FindPropertyRelative("ID");
            var t = holster.FindPropertyRelative("Transform");

            var IDRect = new Rect(rect);
            IDRect.height = EditorGUIUtility.singleLineHeight;
            IDRect.width *= 0.5f;
            IDRect.x += 18;
            EditorGUI.PropertyField(IDRect, ID, GUIContent.none);
            IDRect.x += IDRect.width;
            IDRect.width -= 18;
            EditorGUI.PropertyField(IDRect, t, GUIContent.none);
        }

        /// <summary> Draws all of the fields for the selected ability. </summary>

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(StyleBlue);
            EditorGUILayout.HelpBox("Holster Manager", MessageType.None);
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(DefaultHolster);
            EditorGUILayout.PropertyField(HolsterTime);
            holsterReordable.DoLayoutList();

            if (holsterReordable.index != -1)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var element = holsters.GetArrayElementAtIndex(holsterReordable.index);
                var Weapon = element.FindPropertyRelative("Weapon");

                var pre = "";
                var oldColor = GUI.backgroundColor;
                var newColor = oldColor;

                var weaponObj = Weapon.objectReferenceValue as Component;
                if (weaponObj && weaponObj.gameObject != null)
                {
                    if (weaponObj.gameObject.IsPrefab())
                    {
                        newColor = Color.green;
                        pre = "[Prefab]";
                    }
                    else pre = "[in Scene]";
                }


                EditorGUILayout.LabelField("Holster Weapon " + pre, EditorStyles.boldLabel);
                GUI.backgroundColor = newColor;
                EditorGUILayout.PropertyField(Weapon);
                GUI.backgroundColor = oldColor;
                EditorGUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
#endregion
}