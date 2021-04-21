using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class FTailAnimator2_Editor
    {
        private bool drawHeaderFoldout = false;
        private void HeaderBoxMain(string title, ref bool drawGizmos, ref bool defaultInspector, Texture2D scrIcon, MonoBehaviour target, int height = 22)
        {
            EditorGUILayout.BeginVertical(FGUI_Resources.HeaderBoxStyle);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent(scrIcon), EditorStyles.label, new GUILayoutOption[2] { GUILayout.Width(height - 2), GUILayout.Height(height - 2) }))
            {
                MonoScript script = MonoScript.FromMonoBehaviour(target);
                if (script) EditorGUIUtility.PingObject(script);
                drawHeaderFoldout = !drawHeaderFoldout;
            }

            if (GUILayout.Button(title, FGUI_Resources.GetTextStyle(14, true, TextAnchor.MiddleLeft), GUILayout.Height(height)))
            {
                MonoScript script = MonoScript.FromMonoBehaviour(target);
                if (script) EditorGUIUtility.PingObject(script);
                drawHeaderFoldout = !drawHeaderFoldout;
            }

            if (EditorGUIUtility.currentViewWidth > 326)
                // Youtube channel button
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Tutorials, "Open FImpossible Creations Channel with tutorial videos in your web browser"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    Application.OpenURL("https://www.youtube.com/c/FImpossibleCreations");
                }

            if (EditorGUIUtility.currentViewWidth > 292)
                // Store site button
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Website, "Open FImpossible Creations Asset Store Page inside your web browser"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    Application.OpenURL("https://assetstore.unity.com/publishers/37262");
                }

            // Manual file button
            if (_manualFile == null) _manualFile = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(target))) + "/Tail Animator - User Manual.pdf");
            if (_manualFile)
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Manual, "Open .PDF user manual file for Tail Animator"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    EditorGUIUtility.PingObject(_manualFile);
                    Application.OpenURL(Application.dataPath + "/" + AssetDatabase.GetAssetPath(_manualFile).Replace("Assets/", ""));
                }

            FGUI_Inspector.DrawSwitchButton(ref drawGizmos, FGUI_Resources.Tex_GizmosOff, FGUI_Resources.Tex_Gizmos, "Toggle drawing gizmos on character in scene window", height, height, true);
            FGUI_Inspector.DrawSwitchButton(ref drawHeaderFoldout, FGUI_Resources.Tex_LeftFold, FGUI_Resources.Tex_DownFold, "Toggle to view additional options for foldouts", height, height);

            EditorGUILayout.EndHorizontal();

            if (drawHeaderFoldout)
            {
                FGUI_Inspector.DrawUILine(0.07f, 0.1f, 1, 4, 0.99f);

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                choosedLang = (ELangs)EditorGUILayout.EnumPopup(choosedLang, new GUIStyle(EditorStyles.layerMaskField) { fixedHeight = 0 }, new GUILayoutOption[2] { GUILayout.Width(80), GUILayout.Height(22) });
                if (EditorGUI.EndChangeCheck())
                {
                    PlayerPrefs.SetInt("FimposLang", (int)choosedLang);
                    SetupLangs();
                }

                GUILayout.FlexibleSpace();


                bool hierSwitchOn = PlayerPrefs.GetInt("AnimsH", 1) == 1;
                FGUI_Inspector.DrawSwitchButton(ref hierSwitchOn, FGUI_Resources.Tex_HierSwitch, null, "Switch drawing small icons in hierarchy", height, height, true);
                PlayerPrefs.SetInt("AnimsH", hierSwitchOn ? 1 : 0);


                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rename, "Change component title to yours (current: '" + Get._editor_Title + "'"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(height), GUILayout.Height(height) }))
                {
                    string filename = EditorUtility.SaveFilePanelInProject("Type your title (no file will be created)", Get._editor_Title, "", "Type your title (no file will be created)");
                    if (!string.IsNullOrEmpty(filename))
                    {
                        filename = System.IO.Path.GetFileNameWithoutExtension(filename);
                        if (!string.IsNullOrEmpty(filename))
                        { Get._editor_Title = filename; serializedObject.ApplyModifiedProperties(); }
                    }
                }

                // Old new UI Button
                //FGUI_Inspector.DrawSwitchButton(ref drawNewInspector, FGUI_Resources.Tex_AB, null, "Switch GUI Style to old / new", height, height, true);
                //if (!drawNewInspector && drawDefaultInspector) drawDefaultInspector = false;

                // Default inspector switch
                FGUI_Inspector.DrawSwitchButton(ref defaultInspector, FGUI_Resources.Tex_Default, null, "Toggle inspector view to default inspector.\n\nIf you ever edit source code of Look Animator and add custom variables, you can see them by entering this mode, also sometimes there can be additional/experimental variables to play with.", height, height);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }


        public bool hideSkin = false;
        void El_DrawOptimizeWithMesh()
        {
            // Drawing box informing if spine animator is working by mesh visibility factor
            if (Get.OptimizeWithMesh)
            {
                if (Application.isPlaying)
                {
                    GUI.color = new Color(1f, 1f, 1f, .5f);
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    if (Get.OptimizeWithMesh.isVisible)
                        EditorGUILayout.LabelField("Spine Animator Is Active", FGUI_Resources.HeaderStyle);
                    else
                    {
                        GUI.enabled = false;
                        EditorGUILayout.LabelField("Spine Animator Is Inactive", FGUI_Resources.HeaderStyle);
                        GUI.enabled = true;
                    }

                    EditorGUILayout.EndHorizontal();
                    GUI.color = c;
                }
            }


            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 144;
            EditorGUILayout.PropertyField(sp_OptimizeWithMesh);
            EditorGUIUtility.labelWidth = 0;

            if (Get.OptimizeWithMesh == null)
            {
                if (GUILayout.Button("Find", new GUILayoutOption[1] { GUILayout.Width(44) }))
                {
                    if (Get.OptimizeWithMesh == null)
                    {
                        Get.OptimizeWithMesh = Get.transform.GetComponent<Renderer>();
                        if (!Get.OptimizeWithMesh) Get.OptimizeWithMesh = Get.transform.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) Get.OptimizeWithMesh = Get.transform.parent.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) if (Get.transform.parent.parent != null) Get.OptimizeWithMesh = Get.transform.parent.parent.GetComponentInChildren<Renderer>();
                        if (!Get.OptimizeWithMesh) if (Get.transform.parent != null) if (Get.transform.parent.parent != null) if (Get.transform.parent.parent.parent != null) Get.OptimizeWithMesh = Get.transform.parent.parent.parent.GetComponentInChildren<Renderer>();
                    }
                }
            }
            else
            {
                if (GUILayout.Button(hideSkin ? "Show" : "Hide", new GUILayoutOption[1] { GUILayout.Width(44) }))
                {
                    hideSkin = !hideSkin;

                    if (hideSkin)
                        for (int i = 0; i < skins.Count; i++) skins[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    else
                        for (int i = 0; i < skins.Count; i++) skins[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
            }

            EditorGUILayout.EndHorizontal();
        }


        void El_DrawSpringiness()
        {
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            EditorGUILayout.BeginVertical();

            string balAdd = "";
            string balTip = "";

            if (!Get.UseCurlingCurve && !Get.UseSpringCurve && !Get.UseSlitheryCurve)
                if (Get.Curling > 0.3f)
                {
                    if (Get.Springiness > Mathf.Epsilon)
                    {
                        float treshold = Mathf.Lerp(0.3f, 0.08f, Get.Slithery);

                        if (Get.Springiness + treshold < Get.Curling)
                        {
                            balAdd = " ► ";
                            balTip = "Springiness should be set higher to notice bouncy motion";
                        }
                        else
                        if (Get.Springiness - treshold * 0.7f > Get.Curling)
                        {
                            balAdd = " ◄ ";
                            balTip = "Springiness should be set lower to avoid too rapid bounces";
                        }
                    }
                }


            // Tooltip texts
            EditorGUILayout.BeginHorizontal();
            if (Get.UseSpringCurve)
                GUILayout.Label("                                      Tail Start", smallStyle);
            else
                GUILayout.Label(new GUIContent("                                       Balanced" + " " + balAdd, balTip), smallStyle);

            GUILayout.FlexibleSpace();

            if (Get.UseSpringCurve)
                GUILayout.Label("Tail End         ", smallStyle);
            else
                GUILayout.Label(new GUIContent(balAdd + " " + "Bouncy                      ", balTip), smallStyle);

            EditorGUILayout.EndHorizontal();

            if (Get.UseSpringCurve)
                GUILayout.Space(-0f);
            else
                GUILayout.Space(-4f);

            //if (Get.Springiness == 0.0f) GUI.color = defaultValC; else 
            GUI.color = c;

            if (Get.UseSpringCurve)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_SpringCurve, new GUIContent(sp_Springiness.displayName, sp_Springiness.tooltip), GUILayout.MaxHeight(18)); GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSpringCurve, "Spread springiness speed parameter weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++)
                    {
                        lastSelected[i].UseSpringCurve = Get.UseSpringCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField(sp_Springiness); EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSpringCurve, "Spread springiness speed parameter weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseSpringCurve = Get.UseSpringCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }


            GUI.color = c;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
        }


        void El_DrawSlippery()
        {
            EditorGUILayout.BeginVertical();

            if (Get.UseSlipperyCurve)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = new Color(1f, 1f, 1f, 0.7f);

                GUILayout.Label("                              Tail Start", smallStyle);

                GUILayout.FlexibleSpace();

                GUILayout.Label("Tail End         ", smallStyle);
                GUI.color = c;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_SlipperyCurve, new GUIContent("Collision Slippery", sp_SlipperyCurve.tooltip), GUILayout.MaxHeight(18)); GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSlipperyCurve, "Spread collision slippery parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseSlipperyCurve = Get.UseSlipperyCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField(sp_CollisionSlippery); EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSlipperyCurve, "Spread collision slippery parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseSlipperyCurve = Get.UseSlipperyCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            //EditorGUILayout.BeginVertical();
        }


        void El_DrawSlithery()
        {
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            EditorGUILayout.BeginHorizontal();
            if (Get.UseSlitheryCurve)
                GUILayout.Label("                                      Tail Start", smallStyle);
            else
                GUILayout.Label("                                       Stiff", smallStyle);

            GUILayout.FlexibleSpace();

            if (Get.UseSlitheryCurve)
                GUILayout.Label("Tail End         ", smallStyle);
            else
                GUILayout.Label("Smooth                      ", smallStyle);

            EditorGUILayout.EndHorizontal();
            GUI.color = c;

            if (Get.UseSlitheryCurve)
                GUILayout.Space(0f);
            else
                GUILayout.Space(-3f);


            if (Get.UseSlitheryCurve)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_SlitCurve, new GUIContent(sp_Slithery.displayName, sp_Slithery.tooltip), GUILayout.MaxHeight(18)); GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSlitheryCurve, "Spread sensitivity speed parameter weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseSlitheryCurve = Get.UseSlitheryCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField(sp_Slithery); EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseSlitheryCurve, "Spread sensitivity speed parameter weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseSlitheryCurve = Get.UseSlitheryCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = c;
        }


        void El_DrawCurling()
        {

            GUI.color = new Color(1f, 1f, 1f, 0.7f);

            EditorGUILayout.BeginHorizontal();



            string straiAdd = "";
            string straiTip = "";

            if (!Get.UseCurlingCurve && !Get.UseSpringCurve && !Get.UseSlitheryCurve)
                if (Get.Springiness > Mathf.Epsilon)
                {
                    //float treshold = Mathf.Lerp(0.3f, 0.08f, Get.Slithery);

                    if (Get.Curling < Get.Springiness)
                    {
                        straiAdd = " ► ";
                        straiTip = "Feel free to boost curling high when you're using springiness";
                    }
                }


            if (Get.UseCurlingCurve)
                GUILayout.Label("                                      Tail Start", smallStyle);
            else
                GUILayout.Label(new GUIContent("                                       Straightened " + straiAdd, straiTip), smallStyle);

            GUILayout.FlexibleSpace();

            if (Get.UseCurlingCurve)
                GUILayout.Label("Tail End         ", smallStyle);
            else
                GUILayout.Label("Tangled                      ", smallStyle);

            EditorGUILayout.EndHorizontal();
            GUI.color = c;

            if (Get.UseCurlingCurve)
                GUILayout.Space(0f);
            else
                GUILayout.Space(-2f);


            //if (Get.Curling == 0.5f) GUI.color = defaultValC; else 
            GUI.color = c;


            if (Get.UseCurlingCurve)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_CurlingCurve, new GUIContent(sp_Curling.displayName, sp_Curling.tooltip), GUILayout.MaxHeight(18)); GUILayout.Space(3f);
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseCurlingCurve, "Spread curling over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseCurlingCurve = Get.UseCurlingCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                EditorGUILayout.PropertyField(sp_Curling); EditorGUIUtility.fieldWidth = 0;
                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseCurlingCurve, "Spread curling parameter over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseCurlingCurve = Get.UseCurlingCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = c;
        }


        void El_DrawLimitingAngle()
        {
            if (Get.AngleLimit > 180) GUI.color = defaultValC;
            EditorGUILayout.PropertyField(sp_AngleLimit);
            GUI.color = c;

            if (Get.AngleLimit < 181)
            {
                if (Get.AngleLimitAxis == Vector3.zero) GUI.color = c * new Color(1f, 1f, 1f, 0.6f);
                EditorGUILayout.PropertyField(sp_AngleLimitAxis);
                GUI.color = c;

                if (Get.AngleLimitAxis != Vector3.zero)
                {
                    if (Get.LimitAxisRange.x == Get.LimitAxisRange.y) GUI.color = c * new Color(1f, 1f, 1f, 0.6f);
                    EditorGUILayout.MinMaxSlider(new GUIContent("Range", "If you want limit axes symmetrically leave this parameter unchanged, if you want limit one direction of axis more than reversed, tweak this parameter"),
                        ref Get.LimitAxisRange.x, ref Get.LimitAxisRange.y, -90f, 90f);
                    GUI.color = c;
                }

                EditorGUILayout.PropertyField(sp_LimitSmoothing);

                GUILayout.Space(5f);
            }

            GUI.color = c;
        }


        static bool drawInclud = true;
        void El_DrawSelectiveCollisionBox()
        {
            Get._editor_IsInspectorViewingIncludedColliders = drawInclud;

            GUILayout.Space(1f);
            GUI.color = new Color(0.85f, 1f, 0.85f, 1f);
            EditorGUILayout.BeginHorizontal(FGUI_Resources.HeaderBoxStyleH);
            string f = FGUI_Resources.GetFoldSimbol(drawInclud); int inclC = Get.IncludedColliders.Count;
            GUI.color = c;

            GUILayout.Label(new GUIContent(" "), GUILayout.Width(1));
            string inclColFoldTitle = "";

            if (Get.DynamicWorldCollidersInclusion)
            {
                if (Application.isPlaying)
                    inclColFoldTitle = Lang("Collide With") + " (Dynamic" + " : " + inclC + ")";
                else
                    inclColFoldTitle = "Always Include (" + inclC + ")";
            }
            else
                inclColFoldTitle = Lang("Collide With") + " (" + (inclC == 0 ? "0 !!!" : inclC.ToString()) + ")";

            if (GUILayout.Button(new GUIContent(" " + f + "  " + inclColFoldTitle, FGUI_Resources.TexBehaviourIcon), FGUI_Resources.FoldStyle, GUILayout.Height(24))) drawInclud = !drawInclud;

            //if (!Application.isPlaying)
            //    if (Get.DynamicWorldCollidersInclusion) drawInclud = false;

            //bool checkNullIncludColls = true;
            if (drawInclud)
            {
                //if (GUILayout.Button("+", new GUILayoutOption[2] { GUILayout.MaxWidth(24), GUILayout.MaxHeight(22) }))
                //{
                //    Get.IncludedColliders.Add(null);
                //    //Get._editor_checkInclCollidersForNulls = -10;
                //    //checkNullIncludColls = false;
                //    serializedObject.Update();
                //    serializedObject.ApplyModifiedProperties();
                //}
            }

            EditorGUILayout.EndHorizontal();

            if (drawInclud)
            {
                FGUI_Inspector.VSpace(-3, -5);
                GUI.color = new Color(0.6f, .9f, 0.6f, 1f);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
                GUI.color = c;
                GUILayout.Space(5f);


                // Drawing colliders from list
                if (Get.IncludedColliders.Count == 0)
                {
                    EditorGUILayout.LabelField("Please add here colliders", FGUI_Resources.HeaderStyle);
                    GUILayout.Space(2f);
                }
                else
                {
                    Get.CheckForColliderDuplicatesAndNulls();

                    EditorGUI.BeginChangeCheck();
                    for (int i = 0; i < Get.IncludedColliders.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (Get.IncludedColliders[i] != null)
                        {
                            if (!Get.IncludedColliders[i].gameObject.activeInHierarchy) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                            Get.IncludedColliders[i] = (Collider)EditorGUILayout.ObjectField(Get.IncludedColliders[i], typeof(Collider), true);
                            if (!Get.IncludedColliders[i].gameObject.activeInHierarchy) GUI.color = c;
                        }

                        if (GUILayout.Button("X", new GUILayoutOption[2] { GUILayout.MaxWidth(22), GUILayout.MaxHeight(16) }))
                        {
                            Get.IncludedColliders.RemoveAt(i);
                            serializedObject.Update();
                            serializedObject.ApplyModifiedProperties();
                            return;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        Get.CheckForColliderDuplicatesAndNulls();
                        serializedObject.Update();
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                GUILayout.Space(6f);

                // Lock button
                GUILayout.BeginVertical();
                if (ActiveEditorTracker.sharedTracker.isLocked) GUI.color = new Color(0.44f, 0.44f, 0.44f, 0.8f); else GUI.color = new Color(0.95f, 0.95f, 0.99f, 0.9f);
                if (GUILayout.Button(new GUIContent("Lock Inspector for Drag & Drop Colliders", "Drag & drop colliders to 'Included Colliders' List from the hierarchy"), FGUI_Resources.ButtonStyle, GUILayout.Height(18))) ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
                GUI.color = c;
                GUILayout.EndVertical();

                // Drag and drop box
                El_DrawDragAndDropCollidersBox();

                GUILayout.Space(3f);

                if (Get.IncludedColliders.Count > 0)
                {
                    EditorGUILayout.HelpBox("You can disable collider components on the objects - tail animator will still detect collision. If you deactivate the Game Object with collider - tail animator will not detect collision with it.", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
            }

        }

        void El_DrawSelectiveCollisionBox2D()
        {
            GUILayout.Space(1f);
            GUI.color = new Color(0.85f, 1f, 0.85f, 1f);
            EditorGUILayout.BeginHorizontal(FGUI_Resources.HeaderBoxStyleH);
            string f = FGUI_Resources.GetFoldSimbol(drawInclud); int inclC = Get.IncludedColliders2D.Count;
            GUI.color = c;

            GUILayout.Label(new GUIContent(" "), GUILayout.Width(1));
            string inclColFoldTitle = "";

            inclColFoldTitle = "2D " + Lang("Collide With") + " (" + (inclC == 0 ? "None" : inclC.ToString()) + ")";

            if (GUILayout.Button(new GUIContent(" " + f + "  " + inclColFoldTitle, FGUI_Resources.TexBehaviourIcon), FGUI_Resources.FoldStyle, GUILayout.Height(24))) drawInclud = !drawInclud;

            if (drawInclud)
                if (GUILayout.Button("+", new GUILayoutOption[2] { GUILayout.MaxWidth(24), GUILayout.MaxHeight(22) }))
                {
                    Get.IncludedColliders2D.Add(null);
                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();
                }

            EditorGUILayout.EndHorizontal();

            if (drawInclud)
            {
                FGUI_Inspector.VSpace(-3, -5);
                GUI.color = new Color(0.6f, .9f, 0.6f, 1f);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
                GUI.color = c;
                GUILayout.Space(5f);

                EditorGUILayout.HelpBox("2D Collision is supported ONLY FOR CIRCLE, CAPSULE, BOX and POLYGON colliders for now!", MessageType.None);

                // Drawing colliders from list
                if (Get.IncludedColliders2D.Count == 0)
                {
                    EditorGUILayout.LabelField("Please add here 2D colliders", FGUI_Resources.HeaderStyle);
                    GUILayout.Space(2f);
                }
                else
                {
                    Get.CheckForColliderDuplicatesAndNulls2D();

                    EditorGUI.BeginChangeCheck();
                    for (int i = 0; i < Get.IncludedColliders2D.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (Get.IncludedColliders2D[i] != null)
                        {
                            if (!Get.IncludedColliders2D[i].gameObject.activeInHierarchy) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                            Get.IncludedColliders2D[i] = (Collider2D)EditorGUILayout.ObjectField(Get.IncludedColliders2D[i], typeof(Collider2D), true);
                            if (!Get.IncludedColliders2D[i].gameObject.activeInHierarchy) GUI.color = c;
                        }
                        else
                        {
                            Get.IncludedColliders2D[i] = (Collider2D)EditorGUILayout.ObjectField(Get.IncludedColliders2D[i], typeof(Collider2D), true);
                        }

                        if (GUILayout.Button("X", new GUILayoutOption[2] { GUILayout.MaxWidth(22), GUILayout.MaxHeight(16) }))
                        {
                            Get.IncludedColliders2D.RemoveAt(i);
                            serializedObject.Update();
                            serializedObject.ApplyModifiedProperties();
                            return;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        Get.CheckForColliderDuplicatesAndNulls();
                        serializedObject.Update();
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                GUILayout.Space(3f);

                // Lock button
                GUILayout.BeginVertical();
                if (ActiveEditorTracker.sharedTracker.isLocked) GUI.color = new Color(0.44f, 0.44f, 0.44f, 0.8f); else GUI.color = new Color(0.95f, 0.95f, 0.99f, 0.9f);
                if (GUILayout.Button(new GUIContent("Lock Inspector for Drag & Drop Colliders", "Drag & drop colliders to 'Included Colliders' List from the hierarchy"), FGUI_Resources.ButtonStyle, GUILayout.Height(18))) ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
                GUI.color = c;
                GUILayout.EndVertical();

                // Drag and drop box
                El_DrawDragAndDropCollidersBox();

                GUILayout.Space(3f);

                EditorGUILayout.EndVertical();
            }

        }


        void El_DrawCollidersDynamicInclusion()
        {
            if (Get.DynamicWorldCollidersInclusion)
            {
                if (!Application.isPlaying)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.HelpBox("Using trigger colliders to automatically fill 'Collide With' list", MessageType.Info);
                    GUILayout.Space(3f);

                    EditorGUILayout.PropertyField(sp_InclusionRadius);
                    if ( Get.CollisionMode == TailAnimator2.ECollisionMode.m_3DCollision) EditorGUILayout.PropertyField(sp_IgnoreMeshColliders);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sp_colSameLayer, new GUIContent("Trigger Layer", sp_colSameLayer.tooltip));
                    if (Get.CollidersSameLayer)
                    {
                        EditorGUILayout.LabelField(" ", GUILayout.Width(2));
                        GUI.enabled = false; EditorGUILayout.LayerField(Get.gameObject.layer); GUI.enabled = true; EditorGUIUtility.labelWidth = 0;
                    }
                    else
                    {
                        EditorGUILayout.LabelField(" ", GUILayout.Width(2));
                        EditorGUILayout.PropertyField(sp_colCustomLayer, new GUIContent(""));
                    }
                    EditorGUILayout.EndHorizontal();

                }

                GUILayout.Space(4f);
                EditorGUI.indentLevel++;

                if (Get.CollisionMode == TailAnimator2.ECollisionMode.m_3DCollision)
                    EditorGUILayout.PropertyField(sp_colIgnored, true); 
                
                EditorGUI.indentLevel--;
            }
        }

        void El_DrawDragAndDropCollidersBox()
        {
            GUILayout.Space(3);

            var drop = GUILayoutUtility.GetRect(0f, 38f, new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
            GUI.color = new Color(0.5f, 1f, 0.5f, 0.9f);
            GUI.Box(drop, "Drag & Drop New Colliders Here", new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter, fixedHeight = 38 });
            GUI.color = c;
            var dropEvent = Event.current;

            switch (dropEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop.Contains(dropEvent.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (dropEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var dragged in DragAndDrop.objectReferences)
                        {
                            GameObject draggedObject = dragged as GameObject;

                            if (draggedObject)
                            {
                                if (Get.CollisionMode == TailAnimator2.ECollisionMode.m_3DCollision)
                                {
                                    Collider[] coll = draggedObject.GetComponents<Collider>();
                                    for (int ci = 0; ci < coll.Length; ci++) Get.AddCollider(coll[ci]);
                                }
                                else
                                {
                                    Collider2D[] coll = draggedObject.GetComponents<Collider2D>();
                                    for (int ci = 0; ci < coll.Length; ci++) Get.AddCollider(coll[ci]);
                                }

                                EditorUtility.SetDirty(target);
                            }
                        }

                    }

                    Event.current.Use();
                    break;
            }
        }

    }
}
