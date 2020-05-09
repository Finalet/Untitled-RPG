using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace MalbersAnimations
{
    public class MalbersEditor
    {
        public static GUIStyle StyleGray
        {
            get
            {
                return Style(new Color(0.5f, 0.5f, 0.5f, 0.3f));
            }
        }

        public static GUIStyle BoldFoldout
        {
            get
            {
                var boldFoldout = new GUIStyle(EditorStyles.foldout);
                boldFoldout.fontStyle = FontStyle.Bold;
                return boldFoldout;
            }
        }

        public static GUIStyle StyleBlue
        {
            get
            {
                return Style(new Color(0, 0.5f, 1f, 0.3f));
            }
        }

        public static GUIStyle StyleRed
        {
            get
            {
                return Style(new Color(1, 0.3f, 0f, 0.3f));
            }
        }

        public static GUIStyle StyleGreen
        {
            get
            {
                return Style(new Color(0f, 1f, 0.5f, 0.3f));
            }
        }
        public static GUIStyle StyleOrange
        {
            get
            {
                return Style(new Color(1f, 0.5f, 0.0f, 0.3f));
            }
        }


        public static GUIStyle Border
        {
            get
            {
                return Style(new Color(0, 0.5f, 1f, 0.0f));
            }
        }

       

        public static GUIStyle FlatBox
        {
            get
            {
                return Style(new Color(0.35f, 0.35f, 0.35f, 0.1f));
            }
        }

        public static GUIStyle Style(Color color)
        {
            GUIStyle currentStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(-1, -1, -1, -1)
            };

            Color[] pix = new Color[1];
            pix[0] = color;
            Texture2D bg = new Texture2D(1, 1);
            bg.SetPixels(pix);
            bg.Apply();


            currentStyle.normal.background = bg;
            return currentStyle;
        }

        public static string GetPropertyType(SerializedProperty property)
        {
            var type = property.type;
            var match = Regex.Match(type, @"PPtr<\$(.*?)>");
            if (match.Success)
                type = match.Groups[1].Value;
            return type;
        }

        public static System.Type[] GetTypesByName(string className)
        {
            List<System.Type> returnVal = new List<System.Type>();

            foreach (Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Type[] assemblyTypes = a.GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].Name == className)
                    {
                        returnVal.Add(assemblyTypes[j]);
                    }
                }
            }

            return returnVal.ToArray();
        }

        public static System.Type GetTypeByName(string className)
        {
            return System.AppDomain.CurrentDomain.GetAssemblies() .SelectMany(x => x.GetTypes())  .FirstOrDefault(t => t.Name == className);
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

        public static void DrawHeader(string title)
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
            EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

            // Title
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }
 
        public static void DrawSplitter()
        {
            EditorGUILayout.Space();
            var rect = GUILayoutUtility.GetRect(1f, 1f);

            // Splitter rect should be full-width
            rect.xMin = 20f;
            rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, !EditorGUIUtility.isProSkin
                ? new Color(0.6f, 0.6f, 0.6f, 1.333f)
                : new Color(0.12f, 0.12f, 0.12f, 1.333f));
        }
 
        public static bool DrawHeaderFoldout(string title, bool state)
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
            EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

            // Title
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

            // Active checkbox
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

            var e = Event.current;
            if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
            {
                state = !state;
                e.Use();
            }

            return state;
        } 
        public static void DrawCross(Transform m_transform)
        {
            var gizmoSize = 0.25f;
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.forward * gizmoSize / m_transform.localScale.z));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.forward * -gizmoSize / m_transform.localScale.z));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.up * gizmoSize / m_transform.localScale.y));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.up * -gizmoSize / m_transform.localScale.y));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.right * gizmoSize / m_transform.localScale.x));
            Gizmos.DrawLine(m_transform.position, m_transform.position + m_transform.TransformVector(m_transform.root.right * -gizmoSize / m_transform.localScale.x));
        }

        internal static void DrawLineHelpBox()
        {
            EditorGUILayout.BeginVertical(EditorStyles.textField);
            EditorGUILayout.EndVertical();
        }

        internal static void DrawScript(MonoScript script)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
        }

        internal static void DrawDescription(string v)
        {
            EditorGUILayout.BeginVertical(StyleBlue);
            EditorGUILayout.HelpBox(v, MessageType.None);
            EditorGUILayout.EndVertical();
        }

        internal static void BoolButton(SerializedProperty prop, GUIContent content)
        {
            prop.boolValue = GUILayout.Toggle(prop.boolValue, content , EditorStyles.miniButton);
        }

        internal static void Arrays(SerializedProperty prop, GUIContent content = null)
        {
            EditorGUI.indentLevel++;
            if (content != null)
                EditorGUILayout.PropertyField(prop, content, true);
            else
                EditorGUILayout.PropertyField(prop, true);
            EditorGUI.indentLevel--;
        }


        internal static bool Foldout(SerializedProperty prop, string name, bool bold = false)
        {
            EditorGUI.indentLevel++;
            if (bold)
                prop.boolValue = EditorGUILayout.Foldout(prop.boolValue, name, BoldFoldout);
            else
                prop.boolValue = EditorGUILayout.Foldout(prop.boolValue, name);
            EditorGUI.indentLevel--;

            return prop.boolValue;
        }
    }
}