using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Animal Controller/Combo Manager")]
    public class ComboManager : MonoBehaviour
    {
        public MAnimal animal;

        public int Branch = 0;
        public List<Combo> combos;

        public IntReference ActiveComboIndex = new IntReference(0);

        public Combo ActiveCombo { get; internal set; }

        public int ActiveComboSequenceIndex { get; internal set; }

        public ComboSequence ActiveComboSequence => ActiveCombo.CurrentSequence;

        /// <summary> Is the manager playing a combo? </summary>
        public bool PlayingCombo { get; internal set; }

        public bool debug;

        private void OnEnable()
        {
            animal = this.FindComponent<MAnimal>();

            ActiveCombo = combos[ActiveComboIndex];

            animal.OnModeEnd.AddListener(OnModeEnd);
            Restart();
        }
        private void OnDisable()  { animal.OnModeEnd.RemoveListener(OnModeEnd); }

        private void OnModeEnd(int modeID, int CurrentExitAbility)
        {
            if (PlayingCombo)
            {
                if (ActiveComboSequence.Finisher)
                {
                    ActiveCombo.OnComboFinished.Invoke(ActiveComboSequenceIndex);
                    MDebug($"Combo Finished. <b>[{ActiveComboSequenceIndex}]</b> Branch:<b>[{Branch}]</b>. [Restarting]");
                    Restart();
                }
                else if (CurrentExitAbility == ActiveComboSequence.Ability ) //Are we exiting the Current Secuence or just the Old one??? A new Secuence is playing
                {
                    StartCoroutine(ComboInterrupted());
                }
            }
        }

        protected IEnumerator ComboInterrupted()
        {
            yield return null;
            yield return null; //Wait 2 frames

            if (!animal.IsPlayingMode) // if is no longer playing a Mode then means it was interruptedd
            {
                MDebug($"Incomplete <b>[{ActiveComboSequenceIndex}]</b> Branch: <b>[{Branch}]</b>. [Restarting]");
                ActiveCombo.OnComboInterrupted.Invoke(ActiveComboSequenceIndex);
                Restart();//meaning it got to the end of the combo
            }
            yield return null;
        }


        public virtual void SetActiveCombo(int index)
        {
            ActiveComboIndex = index;
            ActiveCombo = combos[ActiveComboIndex % combos.Count];
        }

        public virtual void SetActiveCombo(IntVar index) => SetActiveCombo(index.Value);
        
        public virtual void SetActiveCombo(string ComboName)
        {
            int index = combos.FindIndex(x => x.Name == ComboName);
            if (index != -1) SetActiveCombo(index);

        }

        public virtual void Play(int branch)
        {
            if (animal.Sleep) return;
            if (!enabled) return;
            if (!animal.IsPlayingMode && !animal.IsPreparingMode) Restart();   //Means is not Playing any mode so Restart

            Branch = branch;
            if (ActiveCombo != null) ActiveCombo.Play(this);
        }

        public virtual void PlayCombo(int branch) => Play(branch);
       

        public virtual void Restart()
        {
            ActiveComboSequenceIndex = 0;
            PlayingCombo = false;

            if (ActiveCombo != null)
            {
                ActiveCombo.CurrentSequence = null;  //Clean the current combo secuence
                ActiveCombo.ActiveSequenceIndex = -1;  //Clean the current combo secuence
                foreach (var seq in ActiveCombo.Sequence) seq.Used = false; //Set that the secuenced is used to 
            }
        }



        internal void MDebug(string value)
        {
#if UNITY_EDITOR
            if (debug) Debug.Log($"<b><color=orange>[{animal.name}] - [Combo]</color></b> - {value}");
#endif
        }

        [HideInInspector] public int selectedCombo = -1;
    }

    [System.Serializable]
    public class Combo
    {
        public ModeID Mode;
        public string Name = "Combo1";

        public List<ComboSequence> Sequence = new List<ComboSequence>();
        public ComboSequence CurrentSequence { get; internal set; }

        /// <summary> Current Index on the list to search combos. This is used to avoid searching already used Sequences on the list</summary>
        public int ActiveSequenceIndex{ get; internal set; }

        public IntEvent OnComboFinished = new IntEvent();
        public IntEvent OnComboInterrupted = new IntEvent();

        public void Play(ComboManager M)
        {
            var animal = M.animal;

            if (!animal.IsPlayingMode) //Means is starting the combo
            {
                for (int i = 0; i < Sequence.Count; i++)
                {
                    var Starter = Sequence[i];

                    if (!Starter.Used && Starter.Branch == M.Branch && Starter.PreviewsAbility == 0) //Only Start with Started Abilities
                    {
                        if (animal.Mode_TryActivate(Mode, Starter.Ability))
                        {
                            M.PlayingCombo = true;
                            PlaySequence(M, Starter);
                            ActiveSequenceIndex = i; //Finding which is the active secuence index;
                            break;
                        }
                    }
                }
            }
            else
            {
                var aMode = animal.ActiveMode;      //Get the Animal Active Mode 

                for (int i = ActiveSequenceIndex + 1; i < Sequence.Count; i++) //Search from the next one
                {
                    var s = Sequence[i];

                    if (!s.Used && s.Branch == M.Branch && s.PreviewsAbility == aMode.AbilityIndex && s.Activation.IsInRange(animal.ModeTime))
                    {
                        if (animal.Mode_ForceActivate(Mode, s.Ability)) //Play the nex animation on the sequence
                        {
                            PlaySequence(M, s);
                            ActiveSequenceIndex = i; //Finding which is the active secuence index;
                            break;
                        }
                    }
                }
            }
        }

        private void PlaySequence(ComboManager M, ComboSequence sequence)
        {
            M.ActiveComboSequenceIndex = Mode.ID * 1000 + sequence.Ability;
            CurrentSequence = sequence; //Store the current sequence
            CurrentSequence.Used = true;
            CurrentSequence.OnSequencePlay.Invoke(M.ActiveComboSequenceIndex);

            M.MDebug($"Sequence: <b>[{M.ActiveComboSequenceIndex}]</b> - Branch:<b>[{M.Branch}]</b>. Time: {Time.time:F2}");
        }
    }


    [System.Serializable]
    public class ComboSequence
    {
        [MinMaxRange(0, 1)]
        public RangedFloat Activation = new RangedFloat(0.3f, 0.6f);
        public int PreviewsAbility = 0;
        public int Ability = 0;
        public int Branch = 0;
        public bool Used;
        /// <summary> Is this Secuence a Finisher Combo? </summary>
        public bool Finisher;
        public IntEvent OnSequencePlay = new IntEvent(); 
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(ComboManager))]

    public class ComboEditor : Editor
    {
        public static GUIStyle StyleGray => MTools.Style(new Color(0.5f, 0.5f, 0.5f, 0.3f));
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));

        private int branch, prev, current;

        SerializedProperty Branch, combos, selectedCombo, debug, ActiveComboIndex;
        private Dictionary<string, ReorderableList> SequenceReordable = new Dictionary<string, ReorderableList>();
        private ReorderableList CombosReor;

        private ComboManager M;
        private int abiliIndex;

        private void OnEnable()
        {
            M= (ComboManager )target;

            combos = serializedObject.FindProperty("combos");
            Branch = serializedObject.FindProperty("Branch");
            selectedCombo = serializedObject.FindProperty("selectedCombo");
            debug = serializedObject.FindProperty("debug");
            ActiveComboIndex = serializedObject.FindProperty("ActiveComboIndex");

            CombosReor = new ReorderableList(serializedObject, combos, true, true, true, true)
            {
                drawElementCallback = Draw_Element_Combo,
                drawHeaderCallback = Draw_Header_Combo,
                onSelectCallback = Selected_ComboCB,
                onRemoveCallback = OnRemoveCallback_Mode
            };
        }

        private void Selected_ComboCB(ReorderableList list)
        {
            selectedCombo.intValue = list.index;
        }

        private void OnRemoveCallback_Mode(ReorderableList list)
        {
            // The reference value must be null in order for the element to be removed from the SerializedProperty array.
            combos.DeleteArrayElementAtIndex(list.index);
            list.index -= 1;

            if (list.index == -1 && combos.arraySize > 0) list.index = 0;   //In Case you remove the first one

            selectedCombo.intValue--;

            list.index = Mathf.Clamp(list.index, 0, list.index - 1);

            EditorUtility.SetDirty(target);
        }

        private void Draw_Header_Combo(Rect rect)
        {
            float half = rect.width / 2;
            var IDIndex = new Rect(rect.x, rect.y, 45, EditorGUIUtility.singleLineHeight);
            var IDName = new Rect(rect.x + 45, rect.y, half - 15 - 45, EditorGUIUtility.singleLineHeight);
            var IDRect = new Rect(rect.x + half + 10, rect.y, half - 10, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(IDIndex, "Index");
            EditorGUI.LabelField(IDName, " Name");
            EditorGUI.LabelField(IDRect, "  Mode");
        }

        private void Draw_Element_Combo(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = combos.GetArrayElementAtIndex(index);
            var Mode = element.FindPropertyRelative("Mode");
            var Name = element.FindPropertyRelative("Name");
            rect.y += 2;

            float half = rect.width / 2;

            var IDIndex = new Rect(rect.x, rect.y, 25, EditorGUIUtility.singleLineHeight);
            var IDName = new Rect(rect.x + 25, rect.y, half - 15 - 25, EditorGUIUtility.singleLineHeight);
            var IDRect = new Rect(rect.x + half + 10, rect.y, half - 10, EditorGUIUtility.singleLineHeight);

            var oldColor = GUI.contentColor;
           
            if (index == M.ActiveComboIndex)
            {
                GUI.contentColor = Color.yellow;
            }


            EditorGUI.LabelField(IDIndex, "(" + index.ToString() + ")");
            EditorGUI.PropertyField(IDName, Name, GUIContent.none);
            EditorGUI.PropertyField(IDRect, Mode, GUIContent.none);
           
            GUI.contentColor = oldColor;

        }

        private void DrawSequence(int ModeIndex, SerializedProperty combo, SerializedProperty sequence)
        {
            ReorderableList Reo_AbilityList;
            string listKey = combo.propertyPath;

            if (SequenceReordable.ContainsKey(listKey))
            {
                Reo_AbilityList = SequenceReordable[listKey]; // fetch the reorderable list in dict
            }
            else
            {
                Reo_AbilityList = new ReorderableList(combo.serializedObject, sequence, true, true, true, true)
                {
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;

                        var Height = EditorGUIUtility.singleLineHeight;
                        var element = sequence.GetArrayElementAtIndex(index);

                        //var Activation = element.FindPropertyRelative("Activation");
                        var PreviewsAbility = element.FindPropertyRelative("PreviewsAbility");
                        var Ability = element.FindPropertyRelative("Ability");
                        var Branch = element.FindPropertyRelative("Branch");
                        var useD = element.FindPropertyRelative("Used");
                        var finisher = element.FindPropertyRelative("Finisher");

                        var IDRect = new Rect(rect) { height = Height };

                        float wid = rect.width / 3;

                        var IRWidth = 30f;
                        var Sep = -10f;
                        var Offset = 40f;

                        float xx = IRWidth + Offset;

                        var IndexRect = new Rect(IDRect) { width = IRWidth };
                        var BranchRect = new Rect(IDRect) { x = xx, width = wid - 15};
                        var PrevARect = new Rect(IDRect) { x = wid + xx+ Sep, width = wid - 15 - Sep };
                        var AbilityRect = new Rect(IDRect) { x = wid * 2 + xx+ Sep, width = wid - 15 - 20 };
                        var FinisherRect = new Rect(IDRect) { x = IDRect.width +30, width =  20 };

                        var style = new GUIStyle(EditorStyles.label);

                        if (!useD.boolValue && Application.isPlaying)style.normal.textColor = Color.green; //If the Combo is not used turn the combos to Green
                      

                        EditorGUI.LabelField(IndexRect, "(" + index.ToString() + ")", style);
                        var oldCColor = GUI.contentColor;
                        var oldColor = GUI.color;

                        if (PreviewsAbility.intValue <= 0)
                        {
                            GUI.contentColor = Color.green;
                        }

                        if (Application.isPlaying)
                        {
                            if (M.ActiveCombo != null)
                            {
                                var Index = M.ActiveCombo.ActiveSequenceIndex;

                                if (Index == index) //Paint Active Index
                                {
                                    GUI.contentColor =  
                                    GUI.color = Color.yellow;

                                    if (M.ActiveComboSequence.Finisher) //Paint finisher
                                    {
                                        GUI.contentColor =
                                        GUI.color = (Color.red + Color.yellow) / 2;
                                    }
                                }
                                else if (Index > index)  //Paint Used Index
                                {
                                    GUI.contentColor =  
                                    GUI.color = Color.gray;
                                }
                               
                            }
                        }

                        EditorGUI.PropertyField(BranchRect, Branch, GUIContent.none);
                        EditorGUI.PropertyField(PrevARect, PreviewsAbility, GUIContent.none);
                        EditorGUI.PropertyField(AbilityRect, Ability, GUIContent.none);
                        EditorGUI.PropertyField(FinisherRect, finisher, GUIContent.none);
                        GUI.contentColor = oldCColor;
                        GUI.color = oldColor;

                        if (index == abiliIndex)
                        {
                            branch = Branch.intValue;
                            prev = PreviewsAbility.intValue;
                            current = Ability.intValue;
                        } 
                    },

                    drawHeaderCallback = rect =>
                    {
                        var Height = EditorGUIUtility.singleLineHeight;
                        var IDRect = new Rect(rect) { height = Height };

                        float wid = rect.width / 3;
                        var IRWidth = 30f;
                        var Sep = -10f;
                        var Offset = 40f;

                        float xx = IRWidth + Offset;

                        var IndexRect = new Rect(IDRect) { width = IRWidth +5};
                        var BranchRect = new Rect(IDRect) { x = xx, width = wid - 15 };
                        var PrevARect = new Rect(IDRect) { x = wid + xx + Sep, width = wid - 15 };
                        var AbilityRect = new Rect(IDRect) { x = wid * 2 + xx + Sep - 10, width = wid - 80};
                        var FinisherRect = new Rect(IDRect) { x = IDRect.width-15, width = 45 };

                        EditorGUI.LabelField(IndexRect, "Index");
                        EditorGUI.LabelField(BranchRect, " Branch");
                        EditorGUI.LabelField(PrevARect, new GUIContent("Activation Ability" ,"Current Mode Ability [Index] Playing on the Animal needed to activate a sequence"));
                        EditorGUI.LabelField(AbilityRect, new GUIContent("Next Ability", "Next Mode Ability [Index] to Play on the Animal if the Active Mode Animation is withing the Activation Range limit "));
                        EditorGUI.LabelField(FinisherRect, new GUIContent("Finisher", "Combo Finisher"));
                    },

                    //elementHeightCallback = (index) =>
                    //{
                    //    Repaint();

                    //    if (index == abiliIndex)

                    //        return EditorGUIUtility.singleLineHeight * 3;
                    //    else
                    //        return EditorGUIUtility.singleLineHeight + 5;
                    //}
                };

                SequenceReordable.Add(listKey, Reo_AbilityList);  //Store it on the Editor
            }

            Reo_AbilityList.DoLayoutList();

            abiliIndex = Reo_AbilityList.index;

            if (abiliIndex != -1)
            {
                var element = sequence.GetArrayElementAtIndex(abiliIndex);

                var Activation = element.FindPropertyRelative("Activation");
                var OnSequencePlay = element.FindPropertyRelative("OnSequencePlay");

                var lbl = "B[" + branch + "] AA[" + prev + "] NA[" + current + "]";

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {

                    EditorGUILayout.LabelField("Sequence Properties - " + lbl);
                    EditorGUILayout.PropertyField(Activation, new GUIContent("Activation", "Range of the Preview Animation the Sequence can be activate"));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.PropertyField(OnSequencePlay, new GUIContent("Sequence Play - " + lbl));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Use Modes to create combo sequences.\nPlay the Active Combo using Play(int Branch); where the Branch is the input difference)");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(ActiveComboIndex, new GUIContent("Active Combo Index", "Active Combo"));

                MalbersEditor.DrawDebugIcon(debug);
              //  debug.boolValue = GUILayout.Toggle(debug.boolValue,new GUIContent("D","Debug"), EditorStyles.miniButton, GUILayout.Width(23));
            }

            EditorGUILayout.EndHorizontal();

               EditorGUILayout.PropertyField(Branch, new GUIContent("Branch", 
                   "Current Branch ID for the Combo Sequence, if this value change then the combo will play different sequences"));

            EditorGUILayout.EndVertical();

            CombosReor.DoLayoutList();

            CombosReor.index = selectedCombo.intValue;
            int IndexCombo = CombosReor.index;

            if (IndexCombo != -1)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    var combo = combos.GetArrayElementAtIndex(IndexCombo);

                    if (combo != null)
                    {
                        var name = combo.FindPropertyRelative("Name");
                        EditorGUILayout.LabelField(name.stringValue, EditorStyles.boldLabel);
                     //   var active = combo.FindPropertyRelative("m_Active");
                        var OnComboFinished = combo.FindPropertyRelative("OnComboFinished");
                        var OnComboInterrupted = combo.FindPropertyRelative("OnComboInterrupted");
                      //  EditorGUILayout.PropertyField(active, new GUIContent("Active", "is the Combo Active?"));
                        EditorGUILayout.HelpBox("Green Sequences are starters combos",  MessageType.None);
                        EditorGUILayout.LabelField("Combo Sequence List", EditorStyles.boldLabel);
                        var sequence = combo.FindPropertyRelative("Sequence");
                        DrawSequence(IndexCombo, combo, sequence);
                        EditorGUILayout.PropertyField(OnComboFinished);
                        EditorGUILayout.PropertyField(OnComboInterrupted);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}