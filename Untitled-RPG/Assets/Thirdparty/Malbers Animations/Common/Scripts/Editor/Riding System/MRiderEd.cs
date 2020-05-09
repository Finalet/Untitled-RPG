using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace MalbersAnimations.HAP
{
     [CustomEditor(typeof(MRider),true)] 
    public class MRiderEd : Editor
    {
        protected MRider M;
        private MonoScript script;
        private SerializedProperty MountStored, StartMounted, Parent, CreateColliderMounted, animator,/* Editor_RiderCallAnimal,*/
            MountLayer, LayerPath, OnCanMount, OnCanDismount, OnStartMounting, OnEndMounting,
            OnStartDismounting, OnEndDismounting, OnFindMount, CanCallMount, OnAlreadyMounted,   DisableList,
            Col_Center, Col_height, Col_radius, Col_Trigger, CallAnimalA, StopAnimalA, RiderAudio,
        /*  MainCamera, Target,*/ LinkUpdate, debug, AlingMountTrigger, DismountType, DisableComponents, Editor_Tabs1;
          

        //bool EventHelp = false;
        //bool CallHelp = false;

        protected virtual void OnEnable()
        {
            M = (MRider)target;
            script = MonoScript.FromMonoBehaviour(M);

            MountStored = serializedObject.FindProperty("MountStored");
            animator = serializedObject.FindProperty("animator");
            StartMounted = serializedObject.FindProperty("StartMounted");
            Parent = serializedObject.FindProperty("Parent");
            MountLayer = serializedObject.FindProperty("MountLayer");
            LayerPath = serializedObject.FindProperty("LayerPath");
           // Editor_RiderCallAnimal = serializedObject.FindProperty("Editor_RiderCallAnimal");


            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");

            OnCanMount = serializedObject.FindProperty("OnCanMount");
            OnCanDismount = serializedObject.FindProperty("OnCanDismount");
            OnStartMounting = serializedObject.FindProperty("OnStartMounting");
            OnEndMounting = serializedObject.FindProperty("OnEndMounting");
            OnStartDismounting = serializedObject.FindProperty("OnStartDismounting");
            OnEndDismounting = serializedObject.FindProperty("OnEndDismounting");
            OnFindMount = serializedObject.FindProperty("OnFindMount");
            CanCallMount = serializedObject.FindProperty("CanCallMount");
            OnAlreadyMounted = serializedObject.FindProperty("OnAlreadyMounted");

            CreateColliderMounted = serializedObject.FindProperty("CreateColliderMounted");
            Col_Center = serializedObject.FindProperty("Col_Center");
            Col_height = serializedObject.FindProperty("Col_height");
            Col_radius = serializedObject.FindProperty("Col_radius");
            Col_Trigger = serializedObject.FindProperty("Col_Trigger");

            CallAnimalA = serializedObject.FindProperty("CallAnimalA");
            StopAnimalA = serializedObject.FindProperty("StopAnimalA");
            RiderAudio = serializedObject.FindProperty("RiderAudio");

            LinkUpdate = serializedObject.FindProperty("LinkUpdate");
            //CanMount = serializedObject.FindProperty("CanMount");
            //CanDismount = serializedObject.FindProperty("CanDismount");
            debug = serializedObject.FindProperty("debug");
            AlingMountTrigger = serializedObject.FindProperty("AlingMountTrigger");
            DismountType = serializedObject.FindProperty("DismountType");
            //MainCamera = serializedObject.FindProperty("MainCamera");
            //Target = serializedObject.FindProperty("Target");

            DisableComponents = serializedObject.FindProperty("DisableComponents");
            DisableList = serializedObject.FindProperty("DisableList");
           // DismountMountOnDeath = serializedObject.FindProperty("DismountMountOnDeath");
        }

        #region GUICONTENT
        private readonly GUIContent G_DisableComponents = new GUIContent("Disable Components", "If some of the scripts are breaking the Rider Script: disable them");
        private readonly GUIContent G_DisableList = new GUIContent("Disable List", "Monobehaviours that will be disabled while mounted");
        private readonly GUIContent G_CreateColliderMounted = new GUIContent("Create capsule collider while Mounted", "This collider is for hit the Rider while mounted");
        private readonly GUIContent G_Parent = new GUIContent("Parent to Mount", "Parent the Rider to the Mount Point on the Mountable Animal");
        private readonly GUIContent G_DismountType = new GUIContent("Dismount Type", "Changes the Dismount animation on the Rider.\nRandom: Randomly select a Dismount Animation.\nInput: Select the Dismount Animation by the Horizontal and Vertical Input Axis.\n Last: Uses the Last Mount Animation as a reference for the Dismount Animation.");
        // private readonly GUIContent G_DismountMountOnDeath = new GUIContent("Dismount if mount dies", "The Rider will automatically dismount if the Animal Dies");
        #endregion

        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Riding Logic");
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            MalbersEditor.DrawScript(script);

            serializedObject.Update();

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Events", "Advanced", "Debug" });


            int Selection = Editor_Tabs1.intValue;

            if (Selection == 0) DrawGeneral();
            else if (Selection == 1) DrawEvents();
            else if (Selection == 2) DrawAdvanced();
            else if (Selection == 3) DrawDebug();

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();

            AddMountLayer();

        }

        private void AddMountLayer()
        {
            Animator anim = M.GetComponentInChildren<Animator>();

            UnityEditor.Animations.AnimatorController controller = null;

            if (anim) controller = (UnityEditor.Animations.AnimatorController)anim.runtimeAnimatorController;


            if (controller)
            {
                List<UnityEditor.Animations.AnimatorControllerLayer> layers = controller.layers.ToList();

                if (layers.Find(layer => layer.name == M.MountLayer) == null)

                //if (anim.GetLayerIndex("Mounted") == -1)
                {
                    if (GUILayout.Button(new GUIContent("Add Mounted Layer", "Used this to add the Parameters and 'Mounted' Layer from the Mounted Animator to your custom TCP animator ")))
                    {
                        AddLayerMounted(controller);
                    }
                }
            }
        }

      

        private void DrawDebug()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(debug);

                if (Application.isPlaying && debug.boolValue)
                {
                    EditorGUI.BeginDisabledGroup(true);

                    EditorGUILayout.ToggleLeft("Can Mount", M.CanMount);
                    EditorGUILayout.ToggleLeft("Can Dismount", M.CanDismount);
                    EditorGUILayout.ToggleLeft("Can Call Animal", M.CanCallAnimal);
                    EditorGUILayout.Space();
                    EditorGUILayout.ToggleLeft("Mounted", M.Mounted);

                    EditorGUILayout.ToggleLeft("Is on Horse", M.IsOnHorse);
                    EditorGUILayout.ToggleLeft("Is Mounting", M.IsMounting);
                    EditorGUILayout.ToggleLeft("Is Riding", M.IsRiding);
                    EditorGUILayout.ToggleLeft("Is Dismounting", M.IsDismounting);
                    //EditorGUILayout.FloatField("Straight Spine", M.SP_Weight);
                    EditorGUILayout.Space();
                    EditorGUILayout.ObjectField("Current Mount", M.Montura, typeof(Mount),false);
                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvanced()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(AlingMountTrigger, new GUIContent("Align MTrigger Time", "Time to Align to the Mount Trigger Position while is playing the Mount Animation"));
                EditorGUILayout.PropertyField(animator);
                EditorGUILayout.PropertyField(LayerPath);
                EditorGUILayout.PropertyField(MountLayer);
            }
            EditorGUILayout.EndVertical();
        } 

        private void DrawEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                //    if (EventHelp)
                //    {
                //        EditorGUILayout.HelpBox(
                //            "On Start Mounting: Invoked when the rider start the mount animation. " +
                //            "\n\nOn End Mounting: Invoked when the rider finish the mount animation.\n" +
                //            "\nOn Start Dismounting: Invoked when the rider start the dismount animation.\n" +
                //            "\nOn End Dismounting: Invoked when the rider finish the dismount animation." +
                //            "\nOn Find Mount: Invoked when the rider founds something to mount.", MessageType.None);
                //    }

                    EditorGUILayout.PropertyField(OnCanMount);
                    EditorGUILayout.PropertyField(OnCanDismount);

                    EditorGUILayout.PropertyField(OnStartMounting);
                    EditorGUILayout.PropertyField(OnEndMounting);
                    EditorGUILayout.PropertyField(OnStartDismounting);
                    EditorGUILayout.PropertyField(OnEndDismounting);

                    EditorGUILayout.PropertyField(OnFindMount);
                    EditorGUILayout.PropertyField(CanCallMount);

                    if (M.StartMounted)
                    {
                        EditorGUILayout.PropertyField(OnAlreadyMounted);
                    }
                
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(StartMounted, new GUIContent("Start Mounted", "Set an animal to start mounted on it"));
                EditorGUILayout.PropertyField(MountStored, new GUIContent("Stored Mount", "If Start Mounted is Active this will be the Animal to mount"));

                if (M.StartMounted && MountStored.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Select a Animal with 'IMount' interface from the scene if you want to start mounted on it", MessageType.Warning);
                }
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(Parent, G_Parent);
                EditorGUILayout.PropertyField(LinkUpdate, new GUIContent("Link Update", "Updates Everyframe the position and rotation of the rider to the Animal Mount Point"));
                EditorGUILayout.PropertyField(DismountType, G_DismountType);
            }
            EditorGUILayout.EndVertical();



            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                CreateColliderMounted.boolValue = EditorGUILayout.ToggleLeft(G_CreateColliderMounted, CreateColliderMounted.boolValue);

                if (CreateColliderMounted.boolValue)
                {

                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Center Y", GUILayout.MinWidth(40));
                    EditorGUILayout.LabelField("Height", GUILayout.MinWidth(40));
                    EditorGUILayout.LabelField("Radius", GUILayout.MinWidth(40));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(Col_Center, GUIContent.none);
                    EditorGUILayout.PropertyField(Col_height, GUIContent.none);
                    EditorGUILayout.PropertyField(Col_radius, GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(Col_Trigger, new GUIContent("Is Trigger"));

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(DisableComponents, G_DisableComponents);

                if (M.DisableComponents)
                {
                    MalbersEditor.Arrays(DisableList, G_DisableList);

                    if (M.DisableList != null && M.DisableList.Length == 0)
                    {
                        EditorGUILayout.HelpBox("If 'Disable List' is empty , it will disable all Monovehaviours while riding", MessageType.Info);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(CallAnimalA, new GUIContent("Call Animal", "Sound to call the Stored Animal"));
                EditorGUILayout.PropertyField(StopAnimalA, new GUIContent("Stop Animal", "Sound to stop calling the Stored Animal"));
                EditorGUILayout.PropertyField(RiderAudio, new GUIContent("Audio Source", "The reference for the audio source"));
            }
            EditorGUILayout.EndVertical();
        }

        void AddLayerMounted(UnityEditor.Animations.AnimatorController AnimController)
        {
            var MountAnimator = Resources.Load<UnityEditor.Animations.AnimatorController>(M.LayerPath);

            AddParametersOnAnimator(AnimController, MountAnimator);

            foreach (var item in MountAnimator.layers)
            {
                AnimController.AddLayer(item);
            }
        }

        public static void AddParametersOnAnimator(UnityEditor.Animations.AnimatorController AnimController, UnityEditor.Animations.AnimatorController Mounted)
        {
            AnimatorControllerParameter[] parameters = AnimController.parameters;
            AnimatorControllerParameter[] Mountedparameters = Mounted.parameters;

            foreach (var param in Mountedparameters)
            {
                if (!SearchParameter(parameters, param.name))
                {
                    AnimController.AddParameter(param);
                }
            }
        }

        public static bool SearchParameter(AnimatorControllerParameter[] parameters, string name)
        {
            foreach (AnimatorControllerParameter item in parameters)
            {
                if (item.name == name) return true;
            }
            return false;
        }
    }
}
