using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MalbersAnimations.Weapons;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.HAP
{
    /// <summary> Rider Combat Mode</summary>

    [AddComponentMenu("Malbers/Weapons/Weapon Manager")]

    public partial class MWeaponManager : MonoBehaviour, IAnimatorListener, IMAnimator, IMWeaponOwner, IMDamagerSet
    {
        ///This was left blank intentionally
        /// Callbacks: all the public functions and methods
        /// Logic: all Combat logic is there, Equip, Unequip, Aim Mode...
        /// Variables: All Variables and Properties


        #region RESET COMBAT VALUES WHEN THE SCRIPT IS CREATED ON THE EDITOR

#if UNITY_EDITOR

        private void Reset()
        {
            var m_Aim = GetComponent<Aim>();
            if (m_Aim == null) m_Aim = gameObject.AddComponent<Aim>();

            BoolVar RiderCombatMode = MTools.GetInstance<BoolVar>("RC Combat Mode");

            //MEvent RCCombatMode = MTools.GetInstance<MEvent>("RC Combat Mode");
            MEvent RCEquipedWeapon = MTools.GetInstance<MEvent>("RC Equiped Weapon");
            MEvent SetCameraSettings = MTools.GetInstance<MEvent>("Set Camera Settings");

            if (RiderCombatMode != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCombatMode, RiderCombatMode.SetValue);
          //  if (RCCombatMode != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnCombatMode, RCCombatMode.Invoke);
            if (RCEquipedWeapon != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnEquipWeapon, RCEquipedWeapon.Invoke);

            if (SetCameraSettings != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(m_Aim.OnAimSide, SetCameraSettings.Invoke);
            }
        }

        [ContextMenu("Create Combat Inputs", false, 1)]
        void ConnectToInput()
        {
            MInput input = GetComponent<MInput>();
            
            if (input == null)
            { input = gameObject.AddComponent<MInput>(); }

            BoolVar RiderCombatMode = MTools.GetInstance<BoolVar>("RC Combat Mode");

            BoolVar RCWeaponInput = MTools.GetInstance<BoolVar>("RC Weapon Input");
           

            #region AIM INPUT       

            var AIM = input.FindInput("Aim");
            if (AIM == null)
            {
                AIM = new InputRow("Aim", "Aim", KeyCode.Mouse1, InputButton.Press, InputType.Key);
                input.inputs.Add(AIM);

                AIM.active.Variable = RiderCombatMode;
                AIM.active.UseConstant = false;

                UnityEditor.Events.UnityEventTools.AddPersistentListener(AIM.OnInputChanged, Aim_Set);
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

                Debug.Log("<B>RiderAttack1</B> Input created and connected to RiderCombat.MainAttack() and RiderCombat.MainAttackReleased()");
            }
            #endregion

            #region RiderAttack2 INPUT
            var RCA2 = input.FindInput("RiderAttack2");
            if (RCA2 == null)
            {
                RCA2 = new InputRow("RiderAttack2", "RiderAttack2", KeyCode.Mouse1, InputButton.Press, InputType.Key);
                input.inputs.Add(RCA2);

                RCA2.active.Variable = RiderCombatMode;
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

            EditorUtility.SetDirty(input);
        } 


        [ContextMenu("Create Event Listeners", false, 2)]
        void CreateEventListeners()
        {
            MEvent RCSetAim = MTools.GetInstance<MEvent>("RC Set Aim");
            MEvent RCMainAttack = MTools.GetInstance<MEvent>("RC Main Attack");
            MEvent RCSecondaryAttack = MTools.GetInstance<MEvent>("RC Secondary Attack");

            MEventListener listener = GetComponent<MEventListener>();

            if (listener == null) listener = gameObject.AddComponent<MEventListener>();
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

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseBool, Aim_Set);
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


                EditorUtility.SetDirty(listener);
            }
        }


        [ContextMenu("Create Weapons on Holsters")]
        void InstantiateWeaponsHolsters()
        {
            foreach (var h in holsters)
            {
                if (h.Weapon != null && h.Weapon.gameObject.IsPrefab())
                {
                    h.Weapon = GameObject.Instantiate(h.Weapon, h.Transform);
                    h.Weapon.name = h.Weapon.name.Replace("(Clone)", "");
                    h.Weapon.transform.SetLocalTransform(h.Weapon.HolsterOffset);
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }


        [ContextMenu("Find Holster Weapons")]
        internal void ValidateWeaponsChilds()
        {
            foreach (var h in holsters)
            {
                if (h.Weapon == null && h.Transform != null && h.Transform.childCount > 0)
                {
                    h.Weapon = (h.Transform.GetChild(0).GetComponent<MWeapon>()); ;
                }
            }

            InstantiateWeaponsHolsters();
        }

#endif
        #endregion
    }

    #region Inspector

#if UNITY_EDITOR
    [CustomEditor(typeof(MWeaponManager))]
    public class MWeaponManagerEditor : Editor
    {
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));
        public static GUIStyle StyleGreen => MTools.Style(new Color(0f, 1f, 0.5f, 0.3f));

        MWeaponManager M;

        private SerializedProperty
             debug, LeftHandEquipPoint, RightHandEquipPoint, UseDefaultIK, UseWeaponsOnlyWhileRiding, DisableAim,
            OnEquipWeapon, OnUnequipWeapon, OnCombatMode, OnWeaponAction, CombatLayerPath, CombatLayerName, OnMainAttackStart,
         StrafeOnTarget,  holsters, UseInventory, UseHolsters, DefaultHolster, HolsterTime,AlreadyInstantiated, StoreAfter,
          Editor_Tabs1, Editor_Tabs2, m_WeaponAim, m_WeaponType, m_WeaponHand, m_WeaponHold, m_WeaponAction;//, ExitCombatOnDismount;

        private ReorderableList holsterReordable;

        private void OnEnable()
        {
            M = (MWeaponManager)target;

            FindProperties();

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
            IDRect.x += IDRect.width - 10;
            IDRect.width -= 18;
            EditorGUI.LabelField(IDRect, "   Holster Transform ");

            var buttonRect = new Rect(rect) { x = rect.width - 30, width = 63, y = rect.y - 1};
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(.8f, .8f, 1f, 1f);
           
            if (GUI.Button(buttonRect, new GUIContent("Weapon", "Check for Weapons on the Holsters.\nIf the weapons are prefab it will instantiate them on the scene"), EditorStyles.miniButton))
            {
                M.ValidateWeaponsChilds();
                EditorUtility.SetDirty(target);
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

        private void FindProperties()
        {
            UseDefaultIK = serializedObject.FindProperty("UseDefaultIK");
            StoreAfter = serializedObject.FindProperty("StoreAfter");
            DisableAim = serializedObject.FindProperty("DisableAim");

            m_WeaponAim = serializedObject.FindProperty("m_WeaponAim");
            m_WeaponType = serializedObject.FindProperty("m_WeaponType");
            m_WeaponHold = serializedObject.FindProperty("m_WeaponCharge");
            m_WeaponAction = serializedObject.FindProperty("m_WeaponAction");

            m_WeaponHand = serializedObject.FindProperty("m_WeaponHand");
            UseWeaponsOnlyWhileRiding = serializedObject.FindProperty("UseWeaponsOnlyWhileRiding");



            holsters = serializedObject.FindProperty("holsters");
            DefaultHolster = serializedObject.FindProperty("DefaultHolster");
            HolsterTime = serializedObject.FindProperty("HolsterTime");



            // ExitCombatOnDismount = serializedObject.FindProperty("ExitCombatOnDismount");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");
            //HitMask = serializedObject.FindProperty("HitMask"); 


            LeftHandEquipPoint = serializedObject.FindProperty("LeftHandEquipPoint");
            RightHandEquipPoint = serializedObject.FindProperty("RightHandEquipPoint");

            OnCombatMode = serializedObject.FindProperty("OnCombatMode");
            OnEquipWeapon = serializedObject.FindProperty("OnEquipWeapon");
            OnUnequipWeapon = serializedObject.FindProperty("OnUnequipWeapon");
            OnWeaponAction = serializedObject.FindProperty("OnWeaponAction");
            OnMainAttackStart = serializedObject.FindProperty("OnMainAttackStart");
   

            //OnTarget = serializedObject.FindProperty("OnTarget");
            CombatLayerPath = serializedObject.FindProperty("CombatLayerPath");
            CombatLayerName = serializedObject.FindProperty("CombatLayerName");
            StrafeOnTarget = serializedObject.FindProperty("StrafeOnTarget");
          //  defaultHolster = serializedObject.FindProperty("DefaultHolster");

           
            UseInventory = serializedObject.FindProperty("UseInventory");
            UseHolsters = serializedObject.FindProperty("UseHolsters");
            AlreadyInstantiated = serializedObject.FindProperty("AlreadyInstantiated");
          

            debug = serializedObject.FindProperty("debug");
        }

        /// <summary> Draws all of the fields for the selected ability. </summary>

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(StyleBlue);
            EditorGUILayout.HelpBox("All the Weapons Logic is managed here", MessageType.None);
            EditorGUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
                {

                    Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Holsters/Inv", "Events"/*,"References" */ });

                    if (Editor_Tabs1.intValue != 4) Editor_Tabs2.intValue = 4;

                    Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, new string[] { "Advanced", "Animator", "Debug" });

                    if (Editor_Tabs2.intValue != 4) Editor_Tabs1.intValue = 4;

                    //First Tabs
                    int Selection = Editor_Tabs1.intValue;

                    if (Selection == 0) DrawGeneral();
                    else if (Selection == 1) ShowHolsters();
                    else if (Selection == 2) DrawEvents();


                    //2nd Tabs
                    Selection = Editor_Tabs2.intValue;

                    if (Selection == 0) DrawAdvanced();
                    else if (Selection == 1) DrawAnimator();
                    else if (Selection == 2) DrawDebug();

                    if (!Application.isPlaying)
                        AddLayer();
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

        private void AddLayer()
        {
            Animator anim = M.GetComponent<Animator>();

            if (anim)
            {
                var controller = (UnityEditor.Animations.AnimatorController)anim.runtimeAnimatorController;

                if (controller)
                {
                    var layers = controller.layers.ToList();

                    if (layers.Find(layer => layer.name == "Mounted") == null)
                    {
                        EditorGUILayout.HelpBox("No Mounted Layer Found, Add it the Mounted Layer using the Rider 3rd Person Script", MessageType.Warning);
                    }
                    else
                    {
                        if (layers.Find(layer => layer.name == M.CombatLayerName) == null)
                        {
                            if (GUILayout.Button(new GUIContent("Add Rider Combat Layers", "Used for adding the parameters and Layer from the Mounted Animator to your custom character controller animator ")))
                            {
                                AddLayerMountedCombat(controller);
                            }
                        }
                    }
                }
            }
        }

        private void DrawAnimator()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Animator Parameters", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_WeaponAim);
                EditorGUILayout.PropertyField(m_WeaponAction);
                EditorGUILayout.PropertyField(m_WeaponType);
                EditorGUILayout.PropertyField(m_WeaponHold);
                EditorGUILayout.PropertyField(m_WeaponHand);
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
                    EditorGUILayout.Toggle("Aiming Side", M.AimingSide);
                    EditorGUILayout.FloatField("IK Weight", M.WeaponIKW);
                    EditorGUILayout.Space();
                    EditorGUILayout.ObjectField("Active Weapon:  ", M.Weapon, typeof(MWeapon), false);
                    EditorGUILayout.IntField("Weapon.Action: ", M.WeaponAction);

                    if (M.Weapon)
                    {
                        EditorGUILayout.ObjectField("Weapon.Type:  ", M.Weapon?.WeaponType, typeof(WeaponID), false);
                        EditorGUILayout.Toggle("Weapon.Active: ", M.Weapon.Active);
                        EditorGUILayout.Toggle("Weapon.IsAiming: ", M.Weapon.IsAiming);
                        EditorGUILayout.Toggle("Weapon.RightHand: ", M.Weapon.IsRightHanded);
                        EditorGUILayout.Toggle("Weapon.Ready: ", M.Weapon.IsReady);
                        EditorGUILayout.Toggle("Weapon.CanAttack: ", M.Weapon.CanAttack);
                        EditorGUILayout.Toggle("Weapon.IsAttacking: ", M.Weapon.IsAttacking);
                        EditorGUILayout.Toggle("Weapon.IsReloading: ", M.Weapon.IsReloading);
                        EditorGUILayout.Toggle("Weapon.CanCharge: ", M.Weapon.CanCharge);
                        if (M.Weapon.CanCharge)
                        {
                            EditorGUILayout.Toggle("Weapon.IsCharging: ", M.Weapon.IsCharging);
                            EditorGUILayout.FloatField("Weapon.Power: ", M.Weapon.Power);
                            EditorGUILayout.FloatField("Weapon.ChargeNorm: ", M.Weapon.ChargedNormalized);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }

            }
            EditorGUILayout.EndVertical();
        }

        private void ShowHolsters()
        {
            //Inventory and Holders
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.indentLevel++;
                UseInventory.boolValue = GUILayout.Toggle(UseInventory.boolValue, new GUIContent("Use Inventory", "Get the Weapons from an Inventory"), EditorStyles.toolbarButton);
                UseHolsters.boolValue = !UseInventory.boolValue;

                UseHolsters.boolValue = GUILayout.Toggle(UseHolsters.boolValue, new GUIContent("Use Holsters", "The Weapons are child of the Holsters Transform"), EditorStyles.toolbarButton);
                UseInventory.boolValue = !UseHolsters.boolValue;
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndHorizontal();

            if (UseInventory.boolValue)
            {
                EditorGUILayout.BeginVertical(StyleGreen);
                {
                    EditorGUILayout.HelpBox("The weapons gameobjects are received by the method 'Equip_External(GameObject)'", MessageType.None);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    AlreadyInstantiated.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Already Instantiated", "The weapon is already instantiated before entering 'GetWeaponByInventory'"), AlreadyInstantiated.boolValue);
                }
                EditorGUILayout.EndVertical();
            }

            //Holder Stufss
            if (M.UseHolsters)
            {
                EditorGUILayout.BeginVertical(StyleGreen);
                {
                    EditorGUILayout.HelpBox("The weapons are child of the Holsters", MessageType.None);
                }
                EditorGUILayout.EndVertical();

                 EditorGUILayout.PropertyField(DefaultHolster, new GUIContent("Default Holster", "Default  Holster used when no Holster is selected"));
                 EditorGUILayout.PropertyField(HolsterTime, new GUIContent("Holster Time", "Time to smooth parent the weapon to the Hand and Holster"));

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
            }
        }

        private void DrawGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
           //EditorGUILayout.PropertyField(StrafeOnTarget, new GUIContent("Strafe on Target/Aim", "If is The Rider is aiming or has a Target:\nThe Character will be set to Strafe"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(StoreAfter);
            MalbersEditor.DrawDebugIcon(debug);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EquipWeaponPoints();

            EditorGUILayout.PropertyField(UseWeaponsOnlyWhileRiding, new GUIContent("Weapons while Riding",
                   "The Weapons can only be used when the Rider is Mounting the Animal. (Disable this to test the weapons on ground (EXPERIMENTAL)"));
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
                EditorGUILayout.LabelField(new GUIContent("Combat Riding Animator", "Location and Name of the Combat while Riding Layer, on the Resource folder"), EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(CombatLayerName, new GUIContent("Layer Name", "Name of the Riding Combat Layer"));
                EditorGUILayout.PropertyField(CombatLayerPath, new GUIContent("Animator Path", "Path of the Combat Layer on the Resource Folder"));

                EditorGUILayout.PropertyField(UseWeaponsOnlyWhileRiding, new GUIContent("Weapons while Riding",
                    "The Weapons can only be used when the Rider is Mounting the Animal. (Disable this to test the weapons on ground (EXPERIMENTAL)"));

                EditorGUILayout.PropertyField(UseDefaultIK, new GUIContent("Default IK", 
                    "Weapon Manager uses Default Unity IK, Disable this if you are going to use The Animation Rigging Package or any 3rd Party IK Solution"));

                EditorGUILayout.PropertyField(DisableAim);
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

                    EditorGUILayout.PropertyField(OnEquipWeapon);
                    EditorGUILayout.PropertyField(OnUnequipWeapon);
                    EditorGUILayout.PropertyField(OnMainAttackStart);
                    EditorGUILayout.PropertyField(OnWeaponAction);
                    EditorGUILayout.Space();

                    //EditorGUILayout.PropertyField(OnAiming);
                }
            }
            EditorGUILayout.EndVertical();
        }

        void AddLayerMountedCombat(UnityEditor.Animations.AnimatorController CurrentAnimator)
        {
            UnityEditor.Animations.AnimatorController MountAnimator = Resources.Load<UnityEditor.Animations.AnimatorController>(M.CombatLayerPath);

            MTools.AddParametersOnAnimator(CurrentAnimator, MountAnimator);

            foreach (var item in MountAnimator.layers)
            {
                CurrentAnimator.AddLayer(item);
            }
        }
    }
#endif
    #endregion
}