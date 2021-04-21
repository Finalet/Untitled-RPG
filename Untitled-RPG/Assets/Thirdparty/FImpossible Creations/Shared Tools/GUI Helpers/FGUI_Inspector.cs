﻿#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    public static class FGUI_Inspector
    {
        public static readonly RectOffset ZeroOffset = new RectOffset(0, 0, 0, 0);

        public static void HeaderBox(ref bool foldout, string title, bool frame, Texture icon = null, int height = 20, int iconsSize = 19, bool big = false)
        {
            if (frame) EditorGUILayout.BeginHorizontal(FGUI_Resources.HeaderBoxStyle); else EditorGUILayout.BeginHorizontal();
            string f = FGUI_Resources.GetFoldSimbol(foldout);

            GUILayout.Label(new GUIContent(" "), GUILayout.Width(1));
            if (icon != null) if (GUILayout.Button(new GUIContent(icon), EditorStyles.label, new GUILayoutOption[2] { GUILayout.Width(iconsSize), GUILayout.Height(iconsSize) })) { foldout = !foldout; }
            if (GUILayout.Button(f + "     " + title + "     " + f, big ? FGUI_Resources.HeaderStyleBig : FGUI_Resources.HeaderStyle, GUILayout.Height(height))) foldout = !foldout;
            if (icon != null) if (GUILayout.Button(new GUIContent(icon), EditorStyles.label, new GUILayoutOption[2] { GUILayout.Width(iconsSize), GUILayout.Height(iconsSize) })) { foldout = !foldout; }
            GUILayout.Label(new GUIContent(" "), GUILayout.Width(1));

            EditorGUILayout.EndHorizontal();
        }

        public static void HeaderBox(string title, bool frame, Texture icon = null, int height = 20, int iconsSize = 19, bool big = false)
        {
            if (frame) EditorGUILayout.BeginHorizontal(FGUI_Resources.HeaderBoxStyle); else EditorGUILayout.BeginHorizontal();

            GUILayout.Label(new GUIContent(" "), GUILayout.Width(1));
            if (icon != null) if (GUILayout.Button(new GUIContent(icon), EditorStyles.label, new GUILayoutOption[2] { GUILayout.Width(iconsSize), GUILayout.Height(iconsSize) })) { }
            if (GUILayout.Button(title, big ? FGUI_Resources.HeaderStyleBig : FGUI_Resources.HeaderStyle, GUILayout.Height(height))) { }
            if (icon != null) if (GUILayout.Button(new GUIContent(icon), EditorStyles.label, new GUILayoutOption[2] { GUILayout.Width(iconsSize), GUILayout.Height(iconsSize) })) { }
            GUILayout.Label(new GUIContent(" "), GUILayout.Width(1));

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// GUILayout.EndVertical(); after foldout
        /// </summary>
        public static void FoldHeaderStart(ref bool foldout, string title, GUIStyle style = null, Texture icon = null, int height = 22)
        {
            FoldHeaderStart(ref foldout, new GUIContent(title), FGUI_Resources.FoldStyle, style, icon, height);
        }

        public static void FoldHeaderStart(ref bool foldout, GUIContent title, GUIStyle textStyle, GUIStyle vertStyle, Texture icon = null, int height = 22)
        {
            if (vertStyle != null) GUILayout.BeginVertical(vertStyle);
            if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(foldout, 10, "►") + "  " + title.text, icon, title.tooltip), textStyle, GUILayout.Height(height))) foldout = !foldout;
        }

        /// <summary>
        /// Header for modules switch / fold
        /// </summary>
        public static void FoldSwitchableHeaderStart(ref bool enable, ref bool foldout, string title, GUIStyle style = null, Texture icon = null, int height = 22, string tooltip = "", bool big = false)
        {
            if (style != null) GUILayout.BeginVertical(style);
            GUILayout.BeginHorizontal();

            if (enable)
            {
                if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(foldout, 10, "►") + "  " + title, icon, tooltip), big ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle, GUILayout.Height(height))) foldout = !foldout;
                enable = EditorGUILayout.Toggle(enable, GUILayout.Width(16));
            }
            else
            {
                if (GUILayout.Button(new GUIContent("  " + title, icon, tooltip), big ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle, GUILayout.Height(height))) { enable = true; }
                enable = EditorGUILayout.Toggle(enable, GUILayout.Width(16));
            }

            GUILayout.EndHorizontal();
        }

        public static void FoldSwitchableHeaderStart(ref bool enable, SerializedProperty toggle, ref bool foldout, string title, GUIStyle style = null, Texture icon = null, int height = 22, string tooltip = "", bool big = false)
        {
            if (style != null) GUILayout.BeginVertical(style);
            GUILayout.BeginHorizontal();

            if (enable)
            {
                if (GUILayout.Button(new GUIContent("  " + FGUI_Resources.GetFoldSimbol(foldout, 10, "►") + "  " + title, icon, tooltip), big ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle, GUILayout.Height(height))) foldout = !foldout;
                EditorGUILayout.PropertyField(toggle, GUIContent.none, GUILayout.Width(16));
            }
            else
            {
                if (GUILayout.Button(new GUIContent("  " + title, icon, tooltip), big ? FGUI_Resources.FoldStyleBig : FGUI_Resources.FoldStyle, GUILayout.Height(height))) { enable = true; }
                EditorGUILayout.PropertyField(toggle, GUIContent.none, GUILayout.Width(16));
            }

            GUILayout.EndHorizontal();
        }


        public static void DrawSwitchButton(ref bool switcher, Texture icon, Texture pressedIcon = null, string tooltip = "", int width = 20, int height = 20, bool reversePress = false)
        {
            bool pressed = switcher;
            if (reversePress) pressed = !pressed;
            Color c = GUI.color;

            if (pressed) GUI.color = new Color(.7f, .7f, .7f, 1f);

            if (pressedIcon != null && ((pressed && !reversePress) || (!pressed && reversePress)))
            {
                if (GUILayout.Button(new GUIContent(pressedIcon, tooltip), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(width), GUILayout.Height(height) })) switcher = !switcher;
            }
            else
                if (GUILayout.Button(new GUIContent(icon, tooltip), FGUI_Resources.ButtonStyle, new GUILayoutOption[2] { GUILayout.Width(width), GUILayout.Height(height) })) switcher = !switcher;

            GUI.color = c;
        }

        public static void DrawSwitchButton(ref bool enable, string tooltip, Texture icon)
        {
            Color c = GUI.color;
            GUI.color = enable ? new Color(0.9f, 0.9f, 0.9f, 1f) : c;
            if (GUILayout.Button(new GUIContent(icon, tooltip), EditorStyles.miniButtonRight, new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(16) })) enable = !enable;
            GUI.color = c;
        }


        /// <returns> Returns true if warning was clicked </returns>
        public static bool DrawWarning(string title)
        {
            bool clicked = false;
            EditorGUILayout.BeginVertical(Style(new Color(.6f, .6f, .3f, .075f), 0));
            if ( GUILayout.Button(new GUIContent(title, EditorGUIUtility.IconContent("console.warnicon.sml").image), EditorStyles.boldLabel)) clicked = true;
            //EditorGUILayout.LabelField(new GUIContent(title, EditorGUIUtility.IconContent("console.warnicon.sml").image), EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            return clicked;
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10, float width = 1f)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            float w = rect.width; float off = rect.width - rect.width * width;
            rect.height = thickness; rect.y += padding / 2; rect.x -= 2; rect.x += off / 2f; rect.width += 2; rect.width *= width;
            EditorGUI.DrawRect(rect, color);
        }

        public static void DrawUILine(float alpha, float brightness = 0.25f, int thickness = 2, int padding = 10, float width = 1f)
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            float w = rect.width; float off = rect.width - rect.width * width;
            rect.height = thickness; rect.y += padding / 2; rect.x -= 2; rect.x += off / 2f; rect.width += 2; rect.width *= width;
            EditorGUI.DrawRect(rect, new Color(brightness, brightness, brightness, alpha));
        }

        public static void VSpace(int space2019, int spacePre2019)
        {
#if UNITY_2019_3_OR_NEWER
            GUILayout.Space(space2019);
#else
            GUILayout.Space(spacePre2019);
#endif
        }


        private static bool displayedDPIWarning = false;
        public static GUIStyle Style(Color bgColor, int off = -1)
        {
            GUIStyle newStyle = new GUIStyle(GUI.skin.box);
            if (off < 0) { if (Screen.dpi != 120) newStyle.border = new RectOffset(off, off, off, off); else if (!displayedDPIWarning) { Debug.Log("<b>[HEY! UNITY DEVELOPER!]</b> It seems you have setted up incorrect DPI settings for unity editor. Check <b>Unity.exe -> Properties -> Compatibility -> Change DPI Settings -> Replace Scaling -> System / System (Upgraded)</b> And restart Unity Editor."); displayedDPIWarning = true; } }
            else newStyle.border = new RectOffset(off, off, off, off);

            Color[] solidColor = new Color[1] { bgColor };

            Texture2D bg = new Texture2D(1, 1);
            bg.SetPixels(solidColor); bg.Apply();
            newStyle.normal.background = bg;

            return newStyle;
        }

        public static GUIStyle Style(RectOffset padding)
        {
            GUIStyle newStyle = new GUIStyle();
            newStyle.padding = padding;
            return newStyle;
        }

        public static GUIStyle Style(RectOffset padding, RectOffset margin, Color bgColor, Vector4 off, int zeroBorder = 0)
        {
            GUIStyle newStyle = new GUIStyle(GUI.skin.box);
            bool g = false;
            if (off.x < 0) { if (Screen.dpi == 120) { if (!displayedDPIWarning) { Debug.Log("<b>[HEY! UNITY DEVELOPER!]</b> It seems you have setted up incorrect DPI settings for unity editor. Check <b>Unity.exe -> Properties -> Compatibility -> Change DPI Settings -> Replace Scaling -> System / System (Upgraded)</b> And restart Unity Editor."); displayedDPIWarning = true; } } else g = true; } else g = true;
            if (g) newStyle.border = new RectOffset((int)off.x, (int)off.y, (int)off.z, (int)off.w);
            newStyle.margin = margin;
            newStyle.padding = padding;

            Texture2D bg;

            if (zeroBorder < 1)
            {
                bg = new Texture2D(1, 1);
                bg.SetPixels(new Color[1] { bgColor }); bg.Apply();
            }
            else
            {
                int s = 16;
                bg = new Texture2D(s, s);
                bg.filterMode = FilterMode.Point;
                Color[] c = new Color[s * s];
                for (int x = 0; x < s; x++)
                {
                    for (int y = 0; y < s; y++)
                    {
                        if (x < zeroBorder || x >= (s - zeroBorder) || y < zeroBorder || y >= (s - zeroBorder))
                            c[x + y * s] = Color.clear;
                        else
                            c[x + y * s] = bgColor;
                    }
                }

                bg.SetPixels(c); bg.Apply();
            }

            newStyle.normal.background = bg;

            return newStyle;
        }

    }
}

#endif
