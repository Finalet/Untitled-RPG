using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FTail
{
    public partial class FTailAnimator2_Editor
    {

        bool drawTailTransforms = false;
        private void Fold_TailChainSetup()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawTailTransforms, 10, "►") + "  " + Lang("Tail Chain") + " (" + (Get._TransformsGhostChain.Count) + ")", FGUI_Resources.Tex_Bone, "Adjust count of chain bones"), FGUI_Resources.FoldStyle, new GUILayoutOption[] { GUILayout.Height(24) })) drawTailTransforms = !drawTailTransforms;

            bool refreshChain = Get._GhostChainInitCount != Get._TransformsGhostChain.Count;
            //Debug.Log("_GhostChainInitCount " + Get._GhostChainInitCount + " Get._TransformsGhostChain.Count " + Get._TransformsGhostChain.Count);

            //if (drawTailTransforms)
            //{
            if (refreshChain) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh /*"Refresh"*/, "Chain Bones count changed you can reset it"), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(24), GUILayout.Height(22) })) { Get.GetGhostChain(); serializedObject.ApplyModifiedProperties(); serializedObject.Update(); return; }
            //}

            EditorGUILayout.EndHorizontal();

            if (drawTailTransforms)
            {
                GUILayout.Space(3);

                if (Get._TransformsGhostChain.Count > 1)
                {
                    GUI.enabled = false;

                    GUILayout.Space(1);

                    for (int i = 1; i < Get._TransformsGhostChain.Count - 1; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(new GUIContent(""), Get._TransformsGhostChain[i].transform, typeof(Transform), true);

                        if (!Application.isPlaying) GUI.enabled = true;
                        if (GUILayout.Button(new GUIContent("X", "Remove bone from chain with this button"), new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(14) }))
                        {
                            Get._TransformsGhostChain.RemoveAt(i);
                            EditorUtility.SetDirty(target);
                            break;
                        }
                        GUI.enabled = false;

                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(1);
                    }

                    GUILayout.Space(2);
                    GUI.enabled = true;

                    if (!Application.isPlaying) GUI.enabled = true; else GUI.enabled = false;

                    GUI.color = c;
                }
                else
                {
                    GUILayout.Space(-4f);
                    EditorGUILayout.LabelField("No bones found in list", FGUI_Resources.HeaderStyle);
                    GUILayout.Space(5f);
                }

                DrawEndBoneInChainSetup();

                GUILayout.Space(2f);
            }
            else
            {
                GUI.color = new Color(1f, 1f, 1f, 0.45f);
                GUILayout.Space(-11f);
                EditorGUILayout.LabelField("...", FGUI_Resources.HeaderStyleBig);
                GUILayout.Space(5f);
                GUI.color = c;

                DrawEndBoneInChainSetup();
            }

            GUILayout.Space(9f);
        }


        void DrawEndBoneInChainSetup()
        {
            if (Get._TransformsGhostChain.Count == 0)
            {
                if (Get.StartBone) { Get.GetGhostChain(); }
                return;
            }

            // End bone field -----------------
            Transform endField = Get.EndBone; if (Get.EndBone == null) { endField = Get._TransformsGhostChain[Get._TransformsGhostChain.Count - 1]; GUI.color = new Color(1f, 1f, 1f, 0.7f); }
            //EditorGUI.BeginChangeCheck(); 
            EditorGUIUtility.labelWidth = Get.EndBone == null ? 100 : 74;

            GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            Transform endB = (Transform)EditorGUILayout.ObjectField(new GUIContent(Get.EndBone == null ? "End Bone (Auto)" : "End Bone"), endField, typeof(Transform), true);
            if (EditorGUI.EndChangeCheck())
            {
                Get.EndBone = endB;
                Get.GetGhostChain();
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
            }


            GUILayout.Space(5);

            if (Application.isPlaying) GUI.enabled = false;
            EditorGUILayout.LabelField("Offset", GUILayout.Width(42));
            bool preOff = FEngineering.VIsZero(Get.EndBoneJointOffset);
            bool makeOff = EditorGUILayout.Toggle(!FEngineering.VIsZero(Get.EndBoneJointOffset), GUILayout.Width(18));
            if (Application.isPlaying) GUI.enabled = true;

            if (Application.isPlaying) GUI.enabled = true;

            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            Transform sBone = Get.StartBone; if (sBone == null) sBone = Get.transform;
            if (endB == null) { endB = GetLastChild(Get._TransformsGhostChain[Get._TransformsGhostChain.Count - 1]); }
            //if (EditorGUI.EndChangeCheck()) { Get.EndBone = endB; serializedObject.ApplyModifiedProperties(); serializedObject.Update(); if (IsChildOf(Get.EndBone, sBone)) Get.GetGhostChain(); serializedObject.Update(); }

            GUILayout.Space(4);
            if (sBone) if (Get.EndBone) if (!IsChildOf(Get.EndBone, sBone)) EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "'End Bone' is not child of 'Start Bone'\nStart Bones is: '" + sBone.name + "'"), new GUILayoutOption[] { GUILayout.Height(20), GUILayout.Width(22) });

            GUILayout.EndHorizontal();


            // Offset parameter changed
            if (preOff == makeOff)
            {
                if (makeOff)
                {
                    Vector3 autoOffset = new Vector3(0f, 0f, 0.25f);

                    #region Auto offset

                    if (Get._TransformsGhostChain.Count > 0)
                    {
                        Transform t = Get._TransformsGhostChain[Get._TransformsGhostChain.Count - 1];
                        Transform p = Get._TransformsGhostChain[Get._TransformsGhostChain.Count - 1].parent;

                        if (p) // Reference from parent
                            autoOffset = t.InverseTransformVector(t.position - p.position);
                        else // Reference to child
                            if (t.childCount > 0)
                        {
                            Transform ch = Get._TransformsGhostChain[0].GetChild(0);
                            autoOffset = t.InverseTransformVector(ch.position - t.position);
                        }
                    }

                    #endregion

                    // Auto position
                    Get.EndBoneJointOffset = autoOffset;

                    if (SceneView.lastActiveSceneView)
                    {
                        if (Get._TransformsGhostChain.Count < 3)
                        {
                            SceneView.lastActiveSceneView.FrameSelected();
                        }
                        else
                        {
#if UNITY_2019_1_OR_NEWER
                            Vector3 size = endB.position - endB.parent.position;
                            Vector3 origin = endB.position + size;
                            size = new Vector3(size.magnitude * 2f, size.magnitude * 2f, size.magnitude * 2f);
                            Bounds lastBone = new Bounds(origin, size);
                            SceneView.lastActiveSceneView.Frame(lastBone, false);
#endif
                        }
                    }
                }
                else
                    Get.EndBoneJointOffset = Vector3.zero;
            }

            GUI.color = c;
        }



        //bool drawCoreAnimRules = true;
        void Fold_DrawCoreAnimatorSetup()
        {
            //FGUI_Inspector.FoldHeaderStart(ref drawCoreAnimRules, "Core Animation Rules", FGUI_Resources.BGInBoxStyle, FGUI_Resources.TexMotionIcon, 24);

            //if (drawCoreAnimRules)
            {
                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 0;
                if (Application.isPlaying) GUI.enabled = false;
                if (Get.UpdateRate > 0) { Get.DetachChildren = false; GUI.enabled = false; }
                EditorGUILayout.PropertyField(sp_Detach); GUI.enabled = true;
                if (Get.DetachChildren) EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "Use it only on not animated models!"), GUILayout.Width(16));

                GUILayout.FlexibleSpace();
                if (Application.isPlaying) GUI.enabled = false;
                EditorGUILayout.PropertyField(sp_StartAfterTPose);
                if (Application.isPlaying) GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 100;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4f);
            }

            GUI.color = c;
        }


        bool drawAdditionalSetup = true;
        private void Fold_DrawAdditionalSetup()
        {
            FGUI_Inspector.FoldHeaderStart(ref drawAdditionalSetup, Lang("Optimization And More"), FGUI_Resources.BGInBoxStyle, FGUI_Resources.TexAddIcon, 24);

            if (drawAdditionalSetup)
            {

                // Delta type, update rate
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                EditorGUIUtility.labelWidth = 88; EditorGUIUtility.fieldWidth = 0;
                EditorGUILayout.PropertyField(sp_DeltaType);
                EditorGUILayout.LabelField("", GUILayout.Width(4));
                EditorGUIUtility.labelWidth = 40;
                EditorGUIUtility.fieldWidth = Get.UpdateRate == 0 ? 10 : 30;
                EditorGUILayout.PropertyField(sp_UpdateRate, new GUIContent("Rate", sp_UpdateRate.tooltip), GUILayout.Width(Get.UpdateRate == 0 ? 60 : 70));

                GUI.color = new Color(1f, 1f, 1f, 0.75f);
                EditorGUILayout.LabelField(Get.UpdateRate == 0 ? "Unlimited" : "FPS", GUILayout.Width(Get.UpdateRate == 0 ? 60 : 28));
                GUI.color = c;

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0; EditorGUIUtility.fieldWidth = 0;

                GUILayout.Space(5);

                // interp, prewarm
                EditorGUILayout.BeginHorizontal();

                EditorGUIUtility.labelWidth = 135;
                EditorGUIUtility.fieldWidth = 32;
                if (Get.UpdateRate <= Mathf.Epsilon) GUI.enabled = false;
                EditorGUILayout.PropertyField(sp_Optim);
                if (Get.UpdateRate <= Mathf.Epsilon) GUI.enabled = true;

                GUILayout.FlexibleSpace();

                EditorGUIUtility.labelWidth = 68;
                if (Application.isPlaying) GUI.enabled = false;
                EditorGUILayout.PropertyField(sp_Prewarm);
                if (Application.isPlaying) GUI.enabled = true;
                GUI.color = c;

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0; EditorGUIUtility.fieldWidth = 0;


                // Animate physics etc.
                GUILayout.Space(7);

                EditorGUILayout.BeginHorizontal();

                if (animator) if (animator.updateMode == AnimatorUpdateMode.AnimatePhysics) GUI.color = new Color(0.6f, 1f, 0.6f, 1f);
                EditorGUIUtility.labelWidth = 135;
                EditorGUILayout.PropertyField(sp_AnimatePhysics);
                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth = 105;
                GUI.color = c;
                if (Application.isPlaying) GUI.enabled = false;
                EditorGUILayout.PropertyField(sp_UpdateAsLast);
                if (Application.isPlaying) GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(7);

                // Detach option
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 135;
                if (Application.isPlaying) GUI.enabled = false;
                if (Get.UpdateRate > 0 || Get.UseIK) { Get.DetachChildren = false; GUI.enabled = false; }
                EditorGUILayout.PropertyField(sp_Detach); GUI.enabled = true;
                if (Get.DetachChildren) { EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "Use it only on not animated models!"), GUILayout.Width(16)); GUILayout.Space(5); }
                //GUILayout.FlexibleSpace();
                //EditorGUIUtility.labelWidth = 65;
                //EditorGUILayout.PropertyField(sp_Boost);
                //EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();


                GUILayout.Space(7);
                El_DrawOptimizeWithMesh();

                GUILayout.Space(4f);

            }

            GUILayout.Space(5f);
            GUILayout.EndVertical();
            GUILayout.Space(-5);
        }



        bool drawWaving = true;
        void Fold_ModuleWaving()
        {
            FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UseWaving, sp_useWav, ref drawWaving, Lang("Auto Waving"), null, _TexWavingIcon, 22, sp_useWav.tooltip, LangBig());

            if (drawWaving && Get.UseWaving)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_wavType);

                if (Get.WavingType == TailAnimator2.FEWavingType.Advanced)
                    EditorGUILayout.PropertyField(sp_altWave);

                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_wavSp);
                EditorGUILayout.PropertyField(sp_wavRa);

                GUILayout.Space(5f);

                bool altWeak = false;
                if (Get.WavingType == TailAnimator2.FEWavingType.Advanced)
                {
                    int zeros = 0; if (Get.WavingAxis.x == 0) zeros++; if (Get.WavingAxis.y == 0) zeros++; if (Get.WavingAxis.z == 0) zeros++;
                    if (zeros > 1) { altWeak = true; GUI.color = new Color(1f, .95f, 0.65f, 0.9f); }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_wavAx);

                if (altWeak)
                    EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "Advanced waving should use more than one axis to get better results"), GUILayout.Width(16));

                GUI.color = c;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5f);

                if (Get.WavingType != TailAnimator2.FEWavingType.Advanced) EditorGUILayout.PropertyField(sp_cosAd);

                if (!Application.isPlaying)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (Get.FixedCycle == 0f) GUI.color = new Color(1f, 1f, 1f, 0.7f);
                    EditorGUILayout.PropertyField(sp_FixedCycle);
                    if (Get.FixedCycle == 0f)
                    { EditorGUILayout.LabelField("", GUILayout.Width(6)); EditorGUILayout.LabelField("(random)", GUILayout.Width(70)); }
                    EditorGUILayout.EndHorizontal();
                }

                GUI.color = c;

                GUILayout.Space(4f);
            }
        }

        bool drawCollisions = true;
        bool drawColSetup = true;
        void Fold_ModuleCollisions()
        {
            //FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UseCollision, ref drawCollisions, Lang("Use Collisions"), null, FGUI_Resources.Tex_Collider, 22, sp_useCollision.tooltip, LangBig());
            GUILayout.BeginHorizontal();

            if (Get.UseCollision)
            {
                if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawCollisions, 10, "►") + "   " + Lang("Collisions"), FGUI_Resources.Tex_Collider, sp_useCollision.tooltip), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle, GUILayout.Height(22))) drawCollisions = !drawCollisions;

                EditorGUILayout.PropertyField(sp_useCollision, GUIContent.none, GUILayout.Width(16));
            }
            else
            {
                if (GUILayout.Button(new GUIContent("   " + Lang("Collisions"), FGUI_Resources.Tex_Collider, sp_useCollision.tooltip), LangBig() ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle, GUILayout.Height(22))) { Get.UseCollision = true; }
                EditorGUILayout.PropertyField(sp_useCollision, GUIContent.none, GUILayout.Width(16));
            }

            GUILayout.EndHorizontal();

            if (drawCollisions && Get.UseCollision)
            {
                GUILayout.Space(4f);

                // 3D Collision Menu
                if (Get.CollisionMode == TailAnimator2.ECollisionMode.m_3DCollision)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (Application.isPlaying) GUI.enabled = false;
                    EditorGUILayout.PropertyField(sp_CollisionSpace);
                    if (Application.isPlaying) GUI.enabled = true;

                    if (Get.CollisionSpace == TailAnimator2.ECollisionSpace.World_Slow)
                    {
                        GUILayout.Space(4);
                        EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Warning, "World Space Collisions are still in experimental stage"), GUILayout.Width(16));
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(sp_collMode);

                    FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.3f), 2, 8);

                    if (Get.CollisionSpace == TailAnimator2.ECollisionSpace.World_Slow)
                    {

                        //EditorGUILayout.PropertyField(sp_PushMode);
                        EditorGUILayout.PropertyField(sp_CollisionSlippery);
                        if (Get.Slithery < 0.1f) GUI.color = new Color(1f, 1f, 1f, defaultValC.a / 2f);
                        EditorGUILayout.PropertyField(sp_ReflectCollision);
                        GUI.color = c;

                        if (Application.isPlaying) GUI.enabled = false; EditorGUI.BeginChangeCheck();
                        Get.CollidersType = EditorGUILayout.IntPopup("Detection Shape", Get.CollidersType, col_wrldColTypesNames, col_wrldColTypes); GUI.enabled = true;

                        if (EditorGUI.EndChangeCheck())
                        { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].CollidersType = Get.CollidersType; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }



                        if (Application.isPlaying) GUI.enabled = true;

                        if (Application.isPlaying) GUI.enabled = false; EditorGUI.BeginChangeCheck();
                        Get.CollideWithOtherTails = EditorGUILayout.IntPopup("Tail Collision", Get.CollideWithOtherTails ? 1 : 0, col_colObjectsNames, col_colObjects) == 1; GUI.enabled = true;

                        if (EditorGUI.EndChangeCheck())
                        { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].CollideWithOtherTails = Get.CollideWithOtherTails; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }




                        GUILayout.Space(4f);

                        if (!Application.isPlaying)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(sp_colAddRigs, new GUIContent("Add Rigidbodies", sp_colAddRigs.tooltip));
                            if (Get.CollidersAddRigidbody)
                            {
                                EditorGUILayout.LabelField(" ", GUILayout.Width(1)); EditorGUIUtility.labelWidth = 40;
                                EditorGUILayout.PropertyField(sp_RigidbodyMass, new GUIContent("Mass", sp_RigidbodyMass.tooltip)); EditorGUIUtility.labelWidth = 0;
                            }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(sp_colSameLayer, new GUIContent("Colliders Layer", sp_colSameLayer.tooltip));
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
                        EditorGUI.indentLevel++; EditorGUILayout.PropertyField(sp_colIgnored); EditorGUI.indentLevel--;

                        //if ( Physics.IgnoreCollision() )
                        //EditorGUILayout.HelpBox("With 'World Collision' tail should have assigned 'Layer' which is not collising with self in Project 'Settings/Physics'", MessageType.None);

                        GUILayout.Space(2f);

                        //FEditor_Styles.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.3f), 2, 8);
                    }
                    else // Selective collision space
                    {
                        El_DrawSelectiveCollisionBox();
                        GUILayout.Space(5f);

                        El_DrawSlippery();

                        if (Get.Slithery < 0.1f) GUI.color = new Color(1f, 1f, 1f, defaultValC.a / 2f);
                        EditorGUILayout.PropertyField(sp_ReflectCollision);
                        GUI.color = c;

                        EditorGUILayout.PropertyField(sp_DetailedCollision);

                        // Collide with disabled
                        //if (!Get.DynamicWorldCollidersInclusion)
                        {
                            //if (Application.isPlaying) GUI.enabled = false;
                            EditorGUIUtility.labelWidth = 190;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(sp_CollideWithDisabledColliders); EditorGUIUtility.labelWidth = 0;
                            if (Get.DynamicWorldCollidersInclusion) EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "[Working with 'Dynamic Inclusion'] Disabled colliders will not be detected by trigger collider but when colliders will be disabled after being caught in trigger detection then this feature will work"), GUILayout.Width(16));
                            EditorGUILayout.EndHorizontal();
                            //if (Application.isPlaying) GUI.enabled = true;
                        }

                        GUILayout.Space(7f);


                        // Dynamic inclusion with trigger colliders method

                        //EditorGUIUtility.labelWidth = 216;
                        //if (Application.isPlaying) GUI.enabled = false;
                        //EditorGUILayout.PropertyField(sp_DynamicWorldCollidersInclusion);
                        //if (Application.isPlaying) GUI.enabled = true;
                        //EditorGUIUtility.labelWidth = 0;

                        EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(Get.DynamicWorldCollidersInclusion, 10, "►") + "  " + Lang("Dynamic World Colliders Inclusion"), FGUI_Resources.Tex_MiniMotion), FGUI_Resources.HeaderStyle, GUILayout.Height(22))) Get.DynamicWorldCollidersInclusion = !Get.DynamicWorldCollidersInclusion;
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Dynamic inclusion feature is under developement stage"), GUILayout.Width(16));
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(sp_DynamicWorldCollidersInclusion, GUIContent.none, GUILayout.Width(16));
                        EditorGUILayout.EndHorizontal();

                        El_DrawCollidersDynamicInclusion();

                        EditorGUILayout.EndVertical();
                    }


                    // Colliders Setup

                    GUILayout.Space(5f);

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                    if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawColSetup, 10, "►") + "  " + Lang("Colliders Setup"), FGUI_Resources.Tex_MiniGear), FGUI_Resources.FoldStyle, GUILayout.Height(22))) drawColSetup = !drawColSetup;

                    if (drawColSetup)
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(sp_colScaleMul, new GUIContent("Scale Multiplier", sp_colScaleMul.tooltip));
                        EditorGUILayout.PropertyField(sp_colScale, new GUIContent("Scale Curve", sp_colScale.tooltip));
                        EditorGUILayout.PropertyField(sp_colDiffFact, new GUIContent("Auto Curve", sp_colDiffFact.tooltip));
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(3f);

                }
                else // 2D Collision Menu
                {
                    EditorGUILayout.PropertyField(sp_collMode);
                    if (Get.CollisionMode == TailAnimator2.ECollisionMode.m_2DCollision) Get.CollisionSpace = TailAnimator2.ECollisionSpace.Selective_Fast;

                    FGUI_Inspector.DrawUILine(new Color(0.5f, 0.5f, 0.5f, 0.3f), 2, 8);

                    El_DrawSelectiveCollisionBox2D();
                    GUILayout.Space(5f);

                    El_DrawSlippery();

                    if (Get.Slithery < 0.1f) GUI.color = new Color(1f, 1f, 1f, defaultValC.a / 2f);
                    EditorGUILayout.PropertyField(sp_ReflectCollision);
                    GUI.color = c;

                    EditorGUILayout.PropertyField(sp_DetailedCollision);

                    EditorGUIUtility.labelWidth = 190;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sp_CollideWithDisabledColliders); EditorGUIUtility.labelWidth = 0;
                    if (Get.DynamicWorldCollidersInclusion) EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "[Working with 'Dynamic Inclusion'] Disabled colliders will not be detected by trigger collider but when colliders will be disabled after being caught in trigger detection then this feature will work"), GUILayout.Width(16));
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(7f);

                    // Dynamic inclusion with trigger colliders method
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(Get.DynamicWorldCollidersInclusion, 10, "►") + "  " + Lang("Dynamic World Colliders Inclusion"), FGUI_Resources.Tex_MiniMotion), FGUI_Resources.HeaderStyle, GUILayout.Height(22))) Get.DynamicWorldCollidersInclusion = !Get.DynamicWorldCollidersInclusion;
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Dynamic inclusion feature is under developement stage"), GUILayout.Width(16));
                    GUILayout.Space(5);
                    EditorGUILayout.PropertyField(sp_DynamicWorldCollidersInclusion, GUIContent.none, GUILayout.Width(16));
                    EditorGUILayout.EndHorizontal();

                    El_DrawCollidersDynamicInclusion();

                    EditorGUILayout.EndVertical();

                    // Colliders Setup

                    GUILayout.Space(5f);

                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                    if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawColSetup, 10, "►") + "  " + Lang("Colliders Setup"), FGUI_Resources.Tex_MiniGear), FGUI_Resources.FoldStyle, GUILayout.Height(22))) drawColSetup = !drawColSetup;

                    if (drawColSetup)
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(sp_colScaleMul, new GUIContent("Scale Multiplier", sp_colScaleMul.tooltip));
                        EditorGUILayout.PropertyField(sp_colScale, new GUIContent("Scale Curve", sp_colScale.tooltip));
                        EditorGUILayout.PropertyField(sp_colDiffFact, new GUIContent("Auto Curve", sp_colDiffFact.tooltip));
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(3f);

                }


            }
        }


        #region GUI Helpers

        int[] col_wrldColTypes = new int[2] { 0, 1 };
        string[] col_wrldColTypesNames = new string[2] { "Spheres", "Capsules" };
        int[] col_colObjects = new int[2] { 0, 1 };
        string[] col_colObjectsNames = new string[2] { "Exclude other tails", "Collide with other tails" };

        #endregion


        bool drawPartialBlend = true;
        void Fold_ModulePartialBlend()
        {
            FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UsePartialBlend, sp_usePartialBlend, ref drawPartialBlend, Lang("Partial Blend"), null, _TexPartialBlendIcon, 22, sp_useCollision.tooltip, LangBig());

            if (drawPartialBlend && Get.UsePartialBlend)
            {

                float sum = 0f;

                if (Get._TransformsGhostChain.Count > 0)
                {
                    GUILayout.Space(6f);
                    EditorGUILayout.HelpBox("Rectangles identifies tail bones, white color means it will be animated just with keyframe animation", MessageType.None);
                    GUILayout.Space(5f);

                    float height = 16f;
                    Rect rect = GUILayoutUtility.GetRect(GUILayoutUtility.GetLastRect().width, height, "TextField");

                    float step = rect.width / (float)Get._TransformsGhostChain.Count;

                    for (int i = 0; i < Get._TransformsGhostChain.Count; i++)
                    {
                        float y = 1 - Mathf.InverseLerp(Get._TransformsGhostChain.Count / 2, Get._TransformsGhostChain.Count + 1, i);

                        float blendValue = Get.GetValueFromCurve(i, Get.BlendCurve);
                        sum += blendValue;

                        EditorGUI.DrawRect(new Rect(rect.x + 2 + i * step, rect.y + (1 - y) * ((height - 1) / 2), step - 2f, height * y), new Color(0.45f, 0.45f, 0.45f, (1f - blendValue) * 0.5f));
                        EditorGUI.DrawRect(new Rect(rect.x + 2 + i * step, rect.y + (1 - y) * ((height - 1) / 2), step - 2f, height * y), new Color(0.2f, 0.5f, 0.9f, (blendValue) * 0.4f));
                    }

                    GUI.color = c;
                }

                GUILayout.Space(6f);

                if (sum == (float)Get._TransformsGhostChain.Count) EditorGUILayout.LabelField("(Default tail animator motion)", FGUI_Resources.HeaderStyle);
                else if (sum == 0f) EditorGUILayout.LabelField("(No tail animator motion)", FGUI_Resources.HeaderStyle);
                else EditorGUILayout.LabelField("(White rects - animated with keyframe animation)", FGUI_Resources.HeaderStyle);
                GUILayout.Space(4f);

                EditorGUIUtility.labelWidth = 130;
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sp_BlendCurve, new GUIContent("Partial Blend Curve", sp_BlendCurve.tooltip), GUILayout.Height(26));

                if (EditorGUI.EndChangeCheck())
                {
                    Get.BlendCurve = Get.ClampCurve(Get.BlendCurve, 0f, 1f, 0f, 1f);
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.LabelField("", GUILayout.Width(4));
                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(26), GUILayout.Height(22) })) { Get.BlendCurve = AnimationCurve.EaseInOut(0f, .95f, 1f, .45f); serializedObject.ApplyModifiedProperties(); /*UnityEditorInternal.InternalEditorUtility.RepaintAllViews();*/ }
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;

                GUILayout.Space(3f);
            }

            GUILayout.Space(3f);

        }


        bool drawIK = true;
        void Fold_ModuleIK()
        {
            FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UseIK, sp_UseIK, ref drawIK, Lang("Inverse Kinematics"), null, _TexIKIcon, 22, "Using CCD IK for tail chain to controll end position of tail limb", LangBig());

            if (drawIK && Get.UseIK)
            {
                GUILayout.Space(3f);

                #region IK Target field

                Transform lastBoneKnown = null;
                if (Get._TransformsGhostChain != null)
                    if (Get._TransformsGhostChain.Count > 0)
                        lastBoneKnown = Get._TransformsGhostChain[Get._TransformsGhostChain.Count - 1];

                if (Get.IKTarget == null && lastBoneKnown)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.3f);
                    EditorGUI.BeginChangeCheck();
                    Get.IKTarget = (Transform)EditorGUILayout.ObjectField(new GUIContent(sp_IKTarget.displayName, sp_IKTarget.tooltip), lastBoneKnown, typeof(Transform), true);
                    if (EditorGUI.EndChangeCheck()) { serializedObject.ApplyModifiedProperties(); }
                    GUI.color = c;
                }
                else
                    EditorGUILayout.PropertyField(sp_IKTarget);

                #endregion

                EditorGUILayout.PropertyField(sp_IKBlend);
                EditorGUILayout.PropertyField(sp_IKAnimatorBlend);

                GUILayout.Space(7f);
                EditorGUILayout.PropertyField(sp_IKReactionQuality);
                EditorGUILayout.PropertyField(sp_IKSmoothing);
                EditorGUILayout.PropertyField(sp_IKContinous);

                if (Get._TransformsGhostChain.Count > 9)
                {
                    GUILayout.Space(7f);
                    EditorGUILayout.HelpBox("When your tail chain have many bones you need to focus on weights and rotation limits to tweak desired behaviour", MessageType.None);
                    GUILayout.Space(-3f);
                }

                GUILayout.Space(7f);
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.fieldWidth = 22; EditorGUILayout.PropertyField(sp_IKAutoWeights);
                GUILayout.Space(5f); EditorGUIUtility.fieldWidth = 0;
                if (Get.IKAutoWeights) EditorGUILayout.PropertyField(sp_IKBaseReactionWeight, new GUIContent(""));
                else
                {
                    EditorGUIUtility.labelWidth = 88;
                    EditorGUILayout.PropertyField(sp_IKweightCurve, new GUIContent("Weight Curve", "Reaction speed weight curve")); EditorGUIUtility.labelWidth = 0;
                }

                EditorGUILayout.EndHorizontal();


                GUILayout.Space(7f);

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.fieldWidth = 22; EditorGUILayout.PropertyField(sp_IKAutoAngleLimits);
                GUILayout.Space(5f); EditorGUIUtility.fieldWidth = 0;
                if (Get.IKAutoAngleLimits) EditorGUILayout.PropertyField(sp_IKAutoAngleLimit, new GUIContent(""));
                EditorGUILayout.EndHorizontal();


                if (!Get.IKAutoAngleLimits)
                {
                    if (Get.IKLimitSettings.Count != Get._TransformsGhostChain.Count)
                        Get.IK_RefreshLimitSettingsContainer();

                    FGUI_Inspector.DrawUILine(new Color(1f, 1f, 1f, 0.15f), 1, 7);

                    for (int i = 0; i < Get.IKLimitSettings.Count; i++)
                    {
                        EditorGUILayout.ObjectField(Get._TransformsGhostChain[i], typeof(Transform), true);

                        if (!Get.IKAutoAngleLimits) // Angle limit fields
                        {
                            EditorGUIUtility.labelWidth = 70;
                            if (Get.IKLimitSettings == null) Get.IK_RefreshLimitSettingsContainer();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(sp_IKLimitSettings.GetArrayElementAtIndex(i).FindPropertyRelative("AngleLimit"));
                            GUILayout.Space(5f);
                            EditorGUILayout.PropertyField(sp_IKLimitSettings.GetArrayElementAtIndex(i).FindPropertyRelative("TwistAngleLimit"));
                            EditorGUILayout.EndHorizontal();
                            FGUI_Inspector.DrawUILine(new Color(1f, 1f, 1f, 0.15f), 1, 7);
                        }

                        EditorGUIUtility.labelWidth = 0;
                    }

                }

                GUILayout.Space(5f);
            }
        }


        bool drawDefl = false;
        void Fold_ModuleDeflection()
        {
            EditorGUILayout.BeginHorizontal();
            FGUI_Inspector.FoldHeaderStart(ref drawDefl, new GUIContent(Lang("Deflection"), "Making collision (or also swing) deflection smoothed creating effect of stiffness connection over all tail segments"), FGUI_Resources.FoldStyle, null, _TexDeflIcon, 22); //"Adding deflection translation when tail have big flexion", LangBig()

            if (Get.Deflection > 0.05f)
                if (Get.MaxStretching > 0.1f)
                {
                    EditorGUILayout.LabelField("Max Stretching (Tweaking Tab) should be set to zero when using deflection and 'Slithery' about zero", FGUI_Resources.BGInBoxStyle);
                    //EditorGUILayout.HelpBox("Max Stretching (Tweaking Tab) should be set to zero when using deflection and 'Slithery' about zero", MessageType.None);
                    GUILayout.Space(3f);
                }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Deflection feature is under developement stage"), GUILayout.Width(16));
            EditorGUILayout.EndHorizontal();

            if (drawDefl)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(sp_Deflection);
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(sp_DeflectOnlyCollisions);
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_DeflectionStartAngle);
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(sp_DeflectionSmooth);
                GUILayout.Space(4f);
                EditorGUILayout.PropertyField(sp_DeflectionFalloff);
                GUILayout.Space(5f);
            }
        }

        bool drawPhysEffectors = false;
        bool drawWindSettings = true;
        void Fold_ModulePhysEffectors()
        {
            FGUI_Inspector.FoldHeaderStart(ref drawPhysEffectors, new GUIContent(Lang("Physical Effectors"), "Simulating physical effects like gravity, wind"), FGUI_Resources.FoldStyle, null, _TexWindIcon, 22); //"Adding deflection translation when tail have big flexion", LangBig()


            if (drawPhysEffectors)
            {

                // Gravity
                GUILayout.Space(5f);
                if (FEngineering.VIsZero(Get.Gravity) && !Get.UseGravityCurve) GUI.color = defaultValC; else GUI.color = c;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_gravity);

                if (Get.UseGravityCurve)
                {
                    EditorGUILayout.LabelField(new GUIContent("*", "Gravity value weight for tail segments multiplied by curve"), GUILayout.Width(9));
                    EditorGUILayout.PropertyField(sp_GravityCurve, new GUIContent("", sp_GravityCurve.tooltip), GUILayout.MaxWidth(32));
                }
                else
                    GUILayout.Space(4f);

                EditorGUI.BeginChangeCheck();
                SwitchButton(ref Get.UseGravityCurve, "Spread gravity weight over tail segments", curveIcon);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].UseGravityCurve = Get.UseGravityCurve; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }
                EditorGUILayout.EndHorizontal();


                // Wind Effector
                GUILayout.Space(5f);

                if (Get.UseWind) // When wind enabled foldable
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(drawWindSettings, 10, "►") + "  " + Lang("Wind"), _windIcon), FGUI_Resources.HeaderStyle, GUILayout.Height(22))) drawWindSettings = !drawWindSettings;
                    GUILayout.Space(8);
                    EditorGUILayout.LabelField("Use TailAnimator Wind component for more settings", FGUI_Resources.BGInBoxStyle);

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Wind feature is under developement stage"), GUILayout.Width(16));
                    GUILayout.Space(5);
                    EditorGUILayout.PropertyField(sp_UseWind, GUIContent.none, GUILayout.Width(16));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("   " + Lang("Wind"), _windIcon), FGUI_Resources.HeaderStyle, GUILayout.Height(22))) { Get.UseWind = true; }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(sp_UseWind, GUIContent.none, GUILayout.Width(16));
                    EditorGUILayout.EndHorizontal();
                }

                if (Get.UseWind && drawWindSettings)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(sp_WindEffectPower, new GUIContent("Effect Power", sp_WindEffectPower.tooltip));
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(sp_WindTurbulencePower, new GUIContent("Turbulence Power", sp_WindTurbulencePower.tooltip));
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(sp_WindWorldNoisePower, new GUIContent("World Noise Power", sp_WindWorldNoisePower.tooltip));
                }

                GUILayout.Space(1f);
                //EditorGUILayout.EndVertical();

            }
        }



        bool drawMaxDistance = true;
        void Fold_ModuleMaxDistance()
        {
            FGUI_Inspector.FoldSwitchableHeaderStart(ref Get.UseMaxDistance, sp_UseMaxDistance, ref drawMaxDistance, Lang("Disable when Far"), null, FGUI_Resources.Tex_Distance, 22, "Measuring distance to camera or other object and smoothly disabling tail animator if tail object is far", LangBig());
            if (drawMaxDistance && Get.UseMaxDistance)
            {
                GUILayout.Space(6f);

                if (Get.DistanceFrom == null)
                {
                    Transform t = null;
                    if (Camera.main) t = Camera.main.transform;
                    else
                    {
                        Camera c = FindObjectOfType<Camera>();
                        if (c) t = c.transform;
                    }

                    if (!Application.isPlaying) Get._distanceFrom_Auto = t;

                    EditorGUI.BeginChangeCheck();
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    Transform tt = (Transform)EditorGUILayout.ObjectField(new GUIContent(sp_DistanceFrom.displayName + " (Auto)", sp_DistanceFrom.tooltip), t, typeof(Transform), true);
                    GUI.color = c;
                    if (EditorGUI.EndChangeCheck()) { Get.DistanceFrom = tt; serializedObject.ApplyModifiedProperties(); }
                }
                else // Distance from defined
                {
                    EditorGUILayout.PropertyField(sp_DistanceFrom);
                }

                GUILayout.Space(4f);
                EditorGUILayout.PropertyField(sp_DistanceMeasurePoint);
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(sp_MaximumDistance);
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_MaxOutDistanceFactor);
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(sp_DistanceWithoutY);
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(sp_FadeDuration);

                GUILayout.Space(5f);
            }
        }

        bool drawSmoothing = true;
        void Fold_TweakingSmoothing()
        {
            if (Get.Slithery < 0.5f)
                if (Get.ReactionSpeed < 0.95f)
                    if (Get.Springiness > 0.2f)
                        drawSmoothing = true;

            FGUI_Inspector.FoldHeaderStart(ref drawSmoothing, Lang("Smoothing Motion"), FGUI_Resources.BGInBoxStyle, FGUI_Resources.TexSmallOptimizeIcon, 21);

            if (drawSmoothing)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.7f);
                EditorGUIUtility.labelWidth = 125;
                EditorGUILayout.BeginHorizontal();
                if (Get.UsePosSpeedCurve)
                    GUILayout.Label("                                      " + Lang("Tail Start"), smallStyle);
                else
                    GUILayout.Label("                                          " + Lang("Slow"), smallStyle);

                GUILayout.FlexibleSpace();

                if (Get.UsePosSpeedCurve)
                    GUILayout.Label(Lang("Tail End") + "         ", smallStyle);
                else
                    GUILayout.Label(Lang("Rapid") + "                      ", smallStyle);

                EditorGUILayout.EndHorizontal();
                GUI.color = c;
                if (Get.UsePosSpeedCurve) GUILayout.Space(-0f); else GUILayout.Space(-2f);


                // Position Speed
                if (Get.UsePosSpeedCurve)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sp_PosCurve, new GUIContent(sp_ReactSpeed.displayName, sp_ReactSpeed.tooltip), GUILayout.MaxHeight(18)); GUILayout.Space(3f);
                    SwitchButton(ref Get.UsePosSpeedCurve, "Spread position speed parameter weight over tail segments", curveIcon);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (Get.ReactionSpeed >= 1f) GUI.color = defaultValC; else GUI.color = c;

                    EditorGUILayout.BeginHorizontal();
                    if (Get.Slithery < 0.5f)
                        if (Get.ReactionSpeed < 0.95f)
                            if (Get.Springiness > 0.2f)
                            {
                                GUI.color = new Color(1f, 1f, 0.6f, 1f);
                                EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "To make springiness noticeable then reaction speed should be higher!"), GUILayout.Width(16));
                            }

                    EditorGUIUtility.fieldWidth = 38;
                    EditorGUILayout.PropertyField(sp_ReactSpeed); EditorGUIUtility.fieldWidth = 0;
                    SwitchButton(ref Get.UsePosSpeedCurve, "Spread position speed parameter weight over tail segments", curveIcon);
                    EditorGUILayout.EndHorizontal();
                }

                GUI.color = c;

                if (Get.Slithery < 0.4f)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.HelpBox("Rotation relevancy works only with Slithery blend", MessageType.None);
                }

                // Rotation Speed
                if (Get.UseRotSpeedCurve)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(sp_RotCurve, new GUIContent(sp_RotRelev.displayName, sp_RotRelev.tooltip), GUILayout.MaxHeight(18)); GUILayout.Space(3f);
                    SwitchButton(ref Get.UseRotSpeedCurve, "Spread rotation speed parameter weight over tail segments", curveIcon);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (Get.RotationRelevancy >= 1f) GUI.color = defaultValC; else GUI.color = c;
                    EditorGUILayout.BeginHorizontal(); EditorGUIUtility.fieldWidth = 38;
                    EditorGUILayout.PropertyField(sp_RotRelev); EditorGUIUtility.fieldWidth = 0;
                    SwitchButton(ref Get.UseRotSpeedCurve, "Spread rotation speed parameter weight over tail segments", curveIcon);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUIUtility.labelWidth = 0;
                GUI.color = c;
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(sp_SmoothingStyle);


                //GUILayout.Space(4f);
                //if (Get.ClassicSlithery == 0f) GUI.color = defaultValC;
                //EditorGUILayout.PropertyField(sp_ClassicSlithery); GUI.color = c;

                GUILayout.Space(4f);
            }

            GUILayout.Space(2f);
            GUILayout.EndVertical();

        }


        #region GUI Helpers

        GUIContent[] axis2D_Names = new GUIContent[4] { new GUIContent("3D Movement (no limit)"), new GUIContent("X is Depth"), new GUIContent("Y is Depth"), new GUIContent("Z is Depth") };
        int[] axis2D_int = new int[4] { 0, 1, 2, 3 };

        #endregion


        bool drawTweakAdditional = false;
        void Fold_TweakingAdditional()
        {
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxLightStyle);

            EditorGUILayout.BeginHorizontal();
            FGUI_Inspector.FoldHeaderStart(ref drawTweakAdditional, Lang("Additional Parameters"), null, FGUI_Resources.TexMotionIcon, 21);
            GUILayout.FlexibleSpace();

            if ((Get._TransformsGhostChain.Count > 0 && Get._TransformsGhostChain.Count < 22))
                EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "If your chain is not bending enough you can try increasing 'Unify Bendiness' down below in this tab and then lowering 'Curling' to adjust it more"), GUILayout.Width(16));
            EditorGUILayout.EndHorizontal();

            if (drawTweakAdditional)
            {
                GUI.color = c;
                GUILayout.Space(5f);

                // Sustain
                if (Get.Sustain == 0f) GUI.color = defaultValC;
                else
                {
                    if (Get.Sustain > 0.1f)
                        if (Get.Springiness > 0.45f)
                            EditorGUILayout.HelpBox("When you set 'Spriginess' high then 'Sustain' can react too much!", MessageType.None);
                        else
                            EditorGUILayout.HelpBox("Set 'Spriginess' around 0.25 and 0.45 to notice sustain effect", MessageType.None);
                }

                EditorGUILayout.PropertyField(sp_Sustain); GUI.color = c;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(sp_Unify);
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(sp_Boost, new GUIContent("Tangle (Experimental)", sp_Boost.tooltip));
                GUILayout.Space(3f);
                GUIContent pname = new GUIContent("Limit Axis for 2D", "If your object moves very fast making tail influenced by speed too much then you can controll it with this parameter");
                EditorGUI.BeginChangeCheck();
                Get.Axis2D = EditorGUILayout.IntPopup(pname, Get.Axis2D, axis2D_Names, axis2D_int);
                if (EditorGUI.EndChangeCheck()) { GetSelectedTailAnimators(); for (int i = 0; i < lastSelected.Count; i++) { lastSelected[i].Axis2D = Get.Axis2D; new SerializedObject(lastSelected[i]).ApplyModifiedProperties(); } }

                GUILayout.Space(3);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_AnimateRoll);

                if (Get.AnimateRoll)
                {
                    float val = EditorGUILayout.Slider(GUIContent.none, 1f - Get.RotationRelevancy, 0f, 1f);
                    Get.RotationRelevancy = 1f - val;
                }

                EditorGUILayout.EndHorizontal();

                if (Get.AnimateRoll)
                    if (Get.RotationRelevancy > 0.5f) EditorGUILayout.HelpBox("Set Roll higher than 0.6 to see results more clearly", MessageType.None);


                GUILayout.Space(2f);
            }

            //EditorGUILayout.BeginVertical();
            GUILayout.Space(2f);
            GUILayout.EndVertical();

        }


        void Fold_TweakingBending()
        {
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            //if (drawBending)
            {
                EditorGUIUtility.labelWidth = 115;
                El_DrawSlithery();
                GUILayout.Space(2f);
                El_DrawCurling();
                GUILayout.Space(2f);
                El_DrawSpringiness();
                GUILayout.Space(5f);
                //EditorGUILayout.PropertyField(sp_Deflection);
                //GUILayout.Space(5f);
                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(2f);
            GUILayout.EndVertical();
        }


        bool drawLimiting = true;
        void Fold_TweakingLimiting()
        {
            FGUI_Inspector.FoldHeaderStart(ref drawLimiting, Lang("Limiting Motion"), FGUI_Resources.BGInBoxLightStyle, FGUI_Resources.Tex_Knob, 21);

            if (drawLimiting)
            {
                GUILayout.Space(4f);
                if (Get.MaxStretching >= 1f) GUI.color = defaultValC;
                EditorGUILayout.PropertyField(sp_MaxStretching); GUI.color = c;
                GUILayout.Space(5f);
                El_DrawLimitingAngle();
                GUILayout.Space(6f);
                if (Get.MotionInfluence == 1f) GUI.color = defaultValC;
                EditorGUILayout.PropertyField(sp_MotionInfluence); GUI.color = c;
                GUILayout.Space(1f);
            }

            GUILayout.Space(4f);
            GUILayout.EndVertical();
        }
    }
}
