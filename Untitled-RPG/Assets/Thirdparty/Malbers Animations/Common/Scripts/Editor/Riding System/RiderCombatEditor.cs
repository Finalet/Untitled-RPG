using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MalbersAnimations.HAP
{
    [CustomEditor(typeof(RiderCombat))]
    public class RiderCombatEditor : Editor
    {
        private MonoScript script;
        RiderCombat M;

        private SerializedProperty
            /*InputWeapon, InputAttack1, InputAttack2, InputAim, Reload, HBack, HLeft,  HRight,HitMask,*/ CombatAbilities,
            HolderLeft, HolderRight, HolderBack, debug, LeftHandEquipPoint, RightHandEquipPoint, ForceMountNormalUpdate,
            OnEquipWeapon, OnUnequipWeapon, OnCombatMode, OnWeaponAction, OnAttack, OnAimSide, OnAiming,/* OnTarget,*/ CombatLayerPath, CombatLayerName,
         StrafeOnTarget, toggleAim, CloneAbilities,/* AimDot,  MainCamera,   Target , */
           activeHolderSide, UseInventory, UseHolders, AlreadyInstantiated, Active, Editor_Tabs1, Editor_Tabs2, ForceNormalUpdate, m_WeaponAim,
            m_WeaponType, m_WeaponHold, m_WeaponHolder, m_WeaponAction, ExitCombatOnDismount;

        private void OnEnable()
        {
            M = (RiderCombat)target;

            script = MonoScript.FromMonoBehaviour(M); 

            m_WeaponAim = serializedObject.FindProperty("m_WeaponAim");
            m_WeaponType = serializedObject.FindProperty("m_WeaponType");
            m_WeaponHold = serializedObject.FindProperty("m_WeaponHold");
            m_WeaponHolder = serializedObject.FindProperty("m_WeaponHolder");
            m_WeaponAction = serializedObject.FindProperty("m_WeaponAction");
            ExitCombatOnDismount = serializedObject.FindProperty("ExitCombatOnDismount");

            CloneAbilities = serializedObject.FindProperty("CloneAbilities");


            CombatAbilities = serializedObject.FindProperty("CombatAbilities");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");
            //HitMask = serializedObject.FindProperty("HitMask"); 
          

            HolderLeft = serializedObject.FindProperty("HolderLeft");
            HolderRight = serializedObject.FindProperty("HolderRight");
            HolderBack = serializedObject.FindProperty("HolderBack");
            LeftHandEquipPoint = serializedObject.FindProperty("LeftHandEquipPoint");
            RightHandEquipPoint = serializedObject.FindProperty("RightHandEquipPoint");

            OnCombatMode = serializedObject.FindProperty("OnCombatMode");
            OnEquipWeapon = serializedObject.FindProperty("OnEquipWeapon");
            OnUnequipWeapon = serializedObject.FindProperty("OnUnequipWeapon");
            OnWeaponAction = serializedObject.FindProperty("OnWeaponAction");
            OnAttack = serializedObject.FindProperty("OnAttack");

            OnAimSide = serializedObject.FindProperty("OnAimSide");
            OnAiming = serializedObject.FindProperty("OnAiming");
            toggleAim = serializedObject.FindProperty("toggleAim");

            //OnTarget = serializedObject.FindProperty("OnTarget");
            CombatLayerPath = serializedObject.FindProperty("CombatLayerPath");
            CombatLayerName = serializedObject.FindProperty("CombatLayerName");
            //Target = serializedObject.FindProperty("Target");
           // AimDot = serializedObject.FindProperty("AimDot");
            StrafeOnTarget = serializedObject.FindProperty("StrafeOnTarget");
           // MainCamera = serializedObject.FindProperty("MainCamera");
            activeHolderSide = serializedObject.FindProperty("activeHolderSide");
            UseInventory = serializedObject.FindProperty("UseInventory");
            UseHolders = serializedObject.FindProperty("UseHolders");
            AlreadyInstantiated = serializedObject.FindProperty("AlreadyInstantiated");
            Active = serializedObject.FindProperty("active");
            ForceNormalUpdate = serializedObject.FindProperty("ForceNormalUpdate");
            ForceMountNormalUpdate = serializedObject.FindProperty("ForceMountNormalUpdate");

            debug = serializedObject.FindProperty("debug");
        }

        /// <summary> Draws all of the fields for the selected ability. </summary>
       
        private void DrawAbility(RiderCombatAbility ability)
        {
            if (ability == null) return;

            SerializedObject abilitySerializedObject;
            abilitySerializedObject = new SerializedObject(ability);
            abilitySerializedObject.Update();

            EditorGUI.BeginChangeCheck();

            var property = abilitySerializedObject.GetIterator();
            property.NextVisible(true);
            property.NextVisible(true);
            do
            {
                EditorGUILayout.PropertyField(property, true);
            } while (property.NextVisible(false));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(ability, "Ability Changed");
                abilitySerializedObject.ApplyModifiedProperties();
                if (ability != null)
                {
                    EditorUtility.SetDirty(ability);
                   // MalbersEditor.SetObjectDirty(ability);
                }
            }
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("The Combat while Riding is controlled here");

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {

                    MalbersEditor.DrawScript(script);


                    Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Holders", "Abilities"/*,"References" */ });

                    if (Editor_Tabs1.intValue != 4)
                    {
                        Editor_Tabs2.intValue = 4;
                    }

                    Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, new string[] { "Advanced", "Animator", "Events", "Debug" });

                    if (Editor_Tabs2.intValue != 4)
                    {
                        Editor_Tabs1.intValue = 4;
                    }

                    //First Tabs
                    int Selection = Editor_Tabs1.intValue;

                    if (Selection == 0) DrawGeneral();
                    else if (Selection == 1) ShowHolders();
                    else if (Selection == 2) ShowAbilities();
                  //  else if (Selection == 1) DrawReferences();


                    //2nd Tabs
                    Selection = Editor_Tabs2.intValue;

                    if (Selection == 0) DrawAdvanced();
                    else if (Selection == 1) DrawAnimator();
                    else if (Selection == 2) DrawEvents();
                    else if (Selection == 3) DrawDebug();

                }


                Animator anim = M.GetComponent<Animator>();

                UnityEditor.Animations.AnimatorController controller = null;
                if (anim) controller = (UnityEditor.Animations.AnimatorController)anim.runtimeAnimatorController;

                if (controller)
                {
                    List<UnityEditor.Animations.AnimatorControllerLayer> layers = controller.layers.ToList();

                    if (layers.Find(layer => layer.name == "Mounted") == null)
                    {
                        EditorGUILayout.HelpBox("No Mounted Layer Found, Add it the Mounted Layer using the Rider 3rd Person Script", MessageType.Warning);
                    }
                    else
                    {
                        if (layers.Find(layer => layer.name == M.CombatLayerName) == null)
                        //if (anim.GetLayerIndex("Rider Combat") == -1)
                        {
                            if (GUILayout.Button(new GUIContent("Add Rider Combat Layers", "Used for adding the parameters and Layer from the Mounted Animator to your custom character controller animator ")))
                            {
                                AddLayerMountedCombat(controller);
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Rider Combat Change");
                //EditorUtility.SetDirty(target);
            }

           
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAnimator()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Animator Parameters", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_WeaponAim);
                EditorGUILayout.PropertyField(m_WeaponAction);
                EditorGUILayout.PropertyField(m_WeaponHolder);
                EditorGUILayout.PropertyField(m_WeaponType);
                EditorGUILayout.PropertyField(m_WeaponHold);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawDebug()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(debug, new GUIContent("Debug", ""));

                if (Application.isPlaying)
                {
                    EditorGUI.BeginDisabledGroup(true);


                    EditorGUILayout.Toggle("Is In Combat mode", M.CombatMode);
                    EditorGUILayout.Toggle("Is Aiming", M.Aim);
                    EditorGUILayout.Space();
                     EditorGUILayout.LabelField("Active Ability:  " , (M.ActiveAbility == null ? "None" : M.ActiveAbility.name) );
                    EditorGUILayout.LabelField("Active Holder Side:", M.ActiveHolderSide.ToString());
                    EditorGUILayout.LabelField(" Active Weapon:  " , (M.ActiveWeapon != null ? M.ActiveWeapon.WeaponType.name :"Null"));
                    EditorGUILayout.ObjectField("Weapon GO:  ", M.ActiveWeaponGameObject, typeof(GameObject), false);
                    EditorGUILayout.LabelField("Camera is on the ", (M.CameraSide ? "Right":"Left") + " side of the Rider ");
                    EditorGUILayout.LabelField("Weapon Action: ", "("+ (int)M.WeaponAction + ")" + M.WeaponAction.ToString());
                    EditorGUI.EndDisabledGroup();
                }

            }
            EditorGUILayout.EndVertical();
        }

        private void ShowAbilities()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(CloneAbilities, new GUIContent("Clone Abilities", "If you are using more than one rider, enable this option"));
                MalbersEditor.Arrays(CombatAbilities, new GUIContent("Rider Combat Abilities", ""));
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUI.indentLevel++;
                M.Editor_ShowAbilities = EditorGUILayout.Foldout(M.Editor_ShowAbilities, "Abilities Properties");
                EditorGUI.indentLevel--;

                if (M.Editor_ShowAbilities)
                {
                    if (M.CombatAbilities != null)
                        foreach (var combatAbility in M.CombatAbilities)
                        {
                            if (combatAbility != null)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                EditorGUILayout.LabelField(combatAbility.name, EditorStyles.toolbarButton);
                                //EditorGUILayout.Separator();
                                DrawAbility(combatAbility);
                                EditorGUILayout.EndVertical();
                            }
                        }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowHolders()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(activeHolderSide, new GUIContent("Active Holder Side", "Holder to draw weapons from, When weapons dont have specific holder"));
            }
            EditorGUILayout.EndVertical();

            ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
            //Inventory and Holders
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.indentLevel++;
                UseInventory.boolValue = GUILayout.Toggle(UseInventory.boolValue, new GUIContent("Use Inventory", "Get the Weapons from an Inventory"), EditorStyles.toolbarButton);
                UseHolders.boolValue = !UseInventory.boolValue;

                UseHolders.boolValue = GUILayout.Toggle(UseHolders.boolValue, new GUIContent("Use Holders", "The Weapons are child of the Holders Transform"), EditorStyles.toolbarButton);
                UseInventory.boolValue = !UseHolders.boolValue;
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndHorizontal();

            if (UseInventory.boolValue)
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGreen);
                {
                    EditorGUILayout.HelpBox("The weapons gameobjects are received by the method 'SetWeaponByInventory(GameObject)'", MessageType.None);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    AlreadyInstantiated.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Already Instantiated", "The weapon is already instantiated before entering 'GetWeaponByInventory'"), AlreadyInstantiated.boolValue);
                }
                EditorGUILayout.EndVertical();
            }

            //Holder Stufss
            if (M.UseHolders)
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGreen);
                {
                    EditorGUILayout.HelpBox("The weapons are child of the Holders", MessageType.None);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(HolderLeft, new GUIContent("Holder Left", "The Tranform that has the weapons on the Left  Side"));
                    EditorGUILayout.PropertyField(HolderRight, new GUIContent("Holder Right", "The Tranform that has the weapons on the Right Side"));
                    EditorGUILayout.PropertyField(HolderBack, new GUIContent("Holder Back", "The Tranform that has the weapons on the Back  Side"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
        }


        private void DrawReferences()
        {
            
        }



        private void DrawGeneral()
        {

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(Active);
                EditorGUILayout.PropertyField(ExitCombatOnDismount, new GUIContent("Exit On Dismount", "Exit the Combat Mode on Dismount"));
               // EditorGUILayout.PropertyField(HitMask, new GUIContent("Hit Mask", "What to Hit"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Aiming", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(toggleAim, new GUIContent("Toggle Aim", "Changes the Aiming  from Pressing the Aim input to One Touch"));
               // EditorGUILayout.PropertyField(Target, new GUIContent("Target", "If the Rider has a Target to aim"));
               // EditorGUILayout.PropertyField(MainCamera, new GUIContent("Main Camera", "Used for Main Player to Aim with weapons"));
              //  EditorGUILayout.PropertyField(AimDot, new GUIContent("Aim Dot", "UI for Aiming"));
                EditorGUILayout.PropertyField(StrafeOnTarget, new GUIContent("Strafe on Target", "If is there a Target change the mount Input to Camera Input "));
            }
            EditorGUILayout.EndVertical();

            EquipWeaponPoints();
        }

        private void EquipWeaponPoints()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Weapon Equip Points", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(LeftHandEquipPoint, new GUIContent("Left Hand"));
                EditorGUILayout.PropertyField(RightHandEquipPoint, new GUIContent("Right Hand"));

                Animator Anim = M.GetComponent<Animator>();
                if (Anim)
                {
                    if (LeftHandEquipPoint.objectReferenceValue == null)
                    {
                        M.LeftHandEquipPoint = Anim.GetBoneTransform(HumanBodyBones.LeftHand);
                    }

                    if (RightHandEquipPoint.objectReferenceValue == null)
                    {
                        M.RightHandEquipPoint = Anim.GetBoneTransform(HumanBodyBones.RightHand);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        private void DrawAdvanced()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                ForceNormalUpdate.boolValue =  EditorGUILayout.ToggleLeft(new GUIContent("Force Normal Update", "When the Rider is Aiming the 'Animator Update mode' will change to 'Normal', So I can modify de bones on Late UPDATE"), ForceNormalUpdate.boolValue);

                if (ForceNormalUpdate.boolValue)
                {
                    ForceMountNormalUpdate.boolValue =  EditorGUILayout.ToggleLeft( new GUIContent("Force Normal Update Mount", "When the Rider is Aiming the 'Animator Update Mode' of the MOUNT Animal will  will change to 'Normal' too"), ForceMountNormalUpdate.boolValue);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField(new GUIContent("Combat Animator", "Location and Name of the Combat Layer, on the Resource folder"), EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(CombatLayerName, new GUIContent("Layer Name", "Name of the Riding Combat Layer"));
                EditorGUILayout.PropertyField(CombatLayerPath, new GUIContent("Animator Path", "Path of the Combat Layer on the Resource Folder"));
            }
            EditorGUILayout.EndVertical();
        }

        bool EventHelp = false;

        void DrawEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(new GUIContent("Events"), EditorStyles.boldLabel);
                    EventHelp = GUILayout.Toggle(EventHelp, "?", EditorStyles.miniButton, GUILayout.Width(18));
                }
                EditorGUILayout.EndHorizontal();
                {
                    if (EventHelp)
                        EditorGUILayout.HelpBox("On Equip Weapon: Invoked when the rider equip a weapon. \n\nOn Unequip Weapon: Invoked when the rider unequip a weapon." +
                            "\nOn Weapon Action: Gets invoked when a new WeaponAction is set. (See Weapon Actions enum for more detail). \n" +
                            "\nOn Attack: Invoked when the rider is about to Attack(Melee) or Fire(Range)\n\nOn AimSide: Invoked when the rider is Aiming\n " +
                            "1:The camera is on the Right Side\n-1 The camera is on the Left Side\n 0:The Aim is Reseted\n\nOn Target: Invoked when the Target is changed",
                            MessageType.None);

                    EditorGUILayout.PropertyField(OnCombatMode);
                    EditorGUILayout.PropertyField(OnAttack);
                    EditorGUILayout.PropertyField(OnEquipWeapon);
                    EditorGUILayout.PropertyField(OnUnequipWeapon);
                    EditorGUILayout.PropertyField(OnWeaponAction);
                    EditorGUILayout.PropertyField(OnAttack);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(OnAiming);
                    EditorGUILayout.PropertyField(OnAimSide);
                  //  EditorGUILayout.PropertyField(OnTarget);
                }

            }
            EditorGUILayout.EndVertical();
        }

        void AddLayerMountedCombat(UnityEditor.Animations.AnimatorController CurrentAnimator)
        {
            UnityEditor.Animations.AnimatorController MountAnimator = Resources.Load<UnityEditor.Animations.AnimatorController>(M.CombatLayerPath);

            MalbersEditor.AddParametersOnAnimator(CurrentAnimator, MountAnimator);

            foreach (var item in MountAnimator.layers)
            {
                CurrentAnimator.AddLayer(item);
            }
        } 
    }
}