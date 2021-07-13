using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio {
	public abstract class StringTableListGUI
	{
		public static float RowHeight = 40.0f;

	  private static Texture2D _deleteRowIcon;
	  private static Texture2D _upRowIcon;
	  private static Texture2D _downRowIcon;

		// Returns the index
		public static bool RenderTableList(
      List<string> list, out int deleteIndex, out bool didSwapRows, out int swapIndex1, out int swapIndex2)
		{
		  bool didModifyList = false;

      deleteIndex = -1;

		  didSwapRows = false;
		  swapIndex1 = -1;
		  swapIndex2 = -1;


		  EditorGUILayout.BeginVertical(GUI.skin.box);
      GUIStyle rowStyle = new GUIStyle(GUI.skin.label);
		  const float buttonHeight = 15.0f;

      for (int i = 0; i < list.Count; i++)
      {
        string item = list[i];

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal(rowStyle);

        EditorGUILayout.LabelField(item, GUI.skin.label);

        GUILayout.FlexibleSpace();

        if (_deleteRowIcon == null)
        {
          _deleteRowIcon = SkyEditorUtility.LoadEditorResourceTexture("RowDelete");
        }

        if (_upRowIcon == null)
        {
          _upRowIcon = SkyEditorUtility.LoadEditorResourceTexture("RowUp");
        }

        if (_downRowIcon == null)
        {
          _downRowIcon = SkyEditorUtility.LoadEditorResourceTexture("RowDown");
        }

        // Tint our images to match the editor skin text.
        Color originalColor = GUI.contentColor;
        GUI.contentColor = GUI.skin.label.normal.textColor;

        // Move row up.
        if (i - 1 >= 0)
        {
          EditorGUI.BeginChangeCheck();
          GUILayout.Button(new GUIContent(_upRowIcon), GUILayout.Height(buttonHeight));
          if (EditorGUI.EndChangeCheck()) {
            // Swap with row above.
            swapIndex1 = i;
            swapIndex2 = i - 1;
            didSwapRows = true;
            didModifyList = true;
          }
        }

        // Move row down.
        if (i + 1 < list.Count)
        {
          EditorGUI.BeginChangeCheck();
          GUILayout.Button(new GUIContent(_downRowIcon), GUILayout.Height(buttonHeight));
          if (EditorGUI.EndChangeCheck()) {
            swapIndex1 = i;
            swapIndex2 = i + 1;
            didSwapRows = true;
            didModifyList = true;
          }
        }
        

        // Delete this row.
        EditorGUI.BeginChangeCheck();
        GUILayout.Button(new GUIContent(_deleteRowIcon), GUILayout.Height(buttonHeight));
        if (EditorGUI.EndChangeCheck())
				{
          didModifyList = true;
          deleteIndex = i;
				}

        GUI.contentColor = originalColor;

				EditorGUILayout.EndHorizontal();

        // Draw a divider between rows.
        if (i != list.Count - 1)
        {
          Rect dividerRect = EditorGUILayout.GetControlRect(false, 1.0f);
          EditorGUI.DrawRect(dividerRect, Color.gray);
        }

        EditorGUILayout.EndVertical();
			}

      GUILayout.EndVertical();

      GUI.changed = didModifyList;
      return didModifyList;
		}

	  public Texture2D TextureFromColor(Color c)
	  {
	    Texture2D tex = new Texture2D(1, 1);
      tex.SetPixel(0, 0, c);
      tex.Apply();

	    return tex;
	  }

	}

}
