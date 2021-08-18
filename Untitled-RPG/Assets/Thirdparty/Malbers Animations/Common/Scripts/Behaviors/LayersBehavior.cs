using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif


namespace MalbersAnimations
{
    [System.Serializable]
    public class LayersActivation
    {
        public string layer;
        public bool activate;
        public StateTransition transA;
        public bool deactivate;
        public StateTransition transD;

    }
    public class LayersBehavior : StateMachineBehaviour
    {
        public LayersActivation[] layers;
        AnimatorTransitionInfo transition;


        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (LayersActivation layer in layers)
            {
                int layer_index = animator.GetLayerIndex(layer.layer);

                transition = animator.GetAnimatorTransitionInfo(layerIndex);

                if (animator.IsInTransition(layerIndex))
                {
                    if (layer.activate)
                    {
                        if (layer.transA == StateTransition.First && stateInfo.normalizedTime <= 0.5f)
                        {
                            animator.SetLayerWeight(layer_index, transition.normalizedTime);
                        }
                        if (layer.transA == StateTransition.Last && stateInfo.normalizedTime >= 0.5f)
                        {
                            animator.SetLayerWeight(layer_index, transition.normalizedTime);
                        }
                    }

                    if (layer.deactivate)
                    {
                        if (layer.transD == StateTransition.First && stateInfo.normalizedTime <= 0.5f)
                        {
                            animator.SetLayerWeight(layer_index, 1 - transition.normalizedTime);
                        }
                        if (layer.transD == StateTransition.Last && stateInfo.normalizedTime >= 0.5f)
                        {
                            animator.SetLayerWeight(layer_index, 1 - transition.normalizedTime);
                        }
                    }
                }

                else
                {
                    //Clean LayerWeight(1) when finish the first Transition
                    if (layer.activate && layer.transA == StateTransition.First)
                        animator.SetLayerWeight(layer_index, 1);

                    //Clean LayerWeight(0) when finish the first Transition
                    if (layer.deactivate && layer.transD == StateTransition.First)
                        animator.SetLayerWeight(layer_index, 0);
                }
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (LayersActivation layer in layers)
            {
                int layer_index = animator.GetLayerIndex(layer.layer);

                //Clean LayerWeight(1) when finish the Last Transition
                if (layer.activate && layer.transA == StateTransition.Last)
                    animator.SetLayerWeight(layer_index, 1);

                //Clean LayerWeight(0) when finish the Last Transition
                if (layer.deactivate && layer.transD == StateTransition.Last)
                    animator.SetLayerWeight(layer_index, 0);
            }
        }
    }





    //INSPECTOR

#if UNITY_EDITOR
    [CustomEditor(typeof(LayersBehavior))]
    public class LayersBehaviorEd : Editor
    {
        private ReorderableList list;
        private LayersBehavior MlayerB;

        private void OnEnable()
        {
            MlayerB = ((LayersBehavior)target);

            list = new ReorderableList(serializedObject, serializedObject.FindProperty("layers"), false, true, true, true);
            list.drawElementCallback = drawElementCallback;
            list.drawHeaderCallback = HeaderCallbackDelegate;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("This Script Enable/Disable Layers in the Transition times");

            EditorGUI.BeginChangeCheck();
            list.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Layers Inspector");
            }
            serializedObject.ApplyModifiedProperties();
        }


        /// <summary> Reordable List Header</summary>
        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_1 = new Rect(rect.x, rect.y, (rect.width / 3), EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_1, "Layer");

            Rect R_2 = new Rect(rect.x + (((rect.width) / 3) + 2), rect.y, 25, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_2, "On");

            Rect R_3 = new Rect(rect.x + ((rect.width) / 3) + 25, rect.y, ((rect.width) / 3) - 25, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_3, "Transition");

            Rect R_4 = new Rect(rect.x + ((rect.width) / 3) * 2 + 2, rect.y, 25, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_4, "Off");

            Rect R_5 = new Rect(rect.x + ((rect.width) / 3) * 2 + 25, rect.y, ((rect.width) / 3) - 25, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(R_5, "Transition");
        }

        void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = MlayerB.layers[index];
            rect.y += 2;
            // element.active = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight), element.active);

            Rect R_1 = new Rect(rect.x, rect.y, (rect.width / 3), EditorGUIUtility.singleLineHeight);
            element.layer = EditorGUI.TextField(R_1, element.layer);

            Rect R_2 = new Rect(rect.x + (((rect.width) / 3) + 5), rect.y, 15, EditorGUIUtility.singleLineHeight);
            element.activate = EditorGUI.Toggle(R_2, element.activate);

            if (element.activate)
            {
                Rect R_3 = new Rect(rect.x + ((rect.width) / 3) + 25, rect.y, ((rect.width) / 3) - 25, EditorGUIUtility.singleLineHeight);
                element.transA = (StateTransition)EditorGUI.EnumPopup(R_3, element.transA);
            }
            Rect R_4 = new Rect(rect.x + ((rect.width) / 3) * 2 + 5, rect.y, 15, EditorGUIUtility.singleLineHeight);
            element.deactivate = EditorGUI.Toggle(R_4, element.deactivate);

            if (element.deactivate)
            {
                Rect R_5 = new Rect(rect.x + ((rect.width) / 3) * 2 + 25, rect.y, ((rect.width) / 3) - 25, EditorGUIUtility.singleLineHeight);
                element.transD = (StateTransition)EditorGUI.EnumPopup(R_5, element.transD);
            }
        }
    }
#endif
}
