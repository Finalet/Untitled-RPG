///////////////////////////////////////////////////////////////////////////
//  Sheath Script 1.1 - Custom Inspector for SheathScript                //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
//  Documentation: 														 //
//  https://www.keviniglesias.com/assets/SheathScript/Documentation.pdf  //
///////////////////////////////////////////////////////////////////////////

/*
 This script makes a custom inspector for the MonoBehaviour SheathComponentScript
 for easier adding and removing sheaths.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This 'if' makes this script to compile only if using the Unity Editor,
//avoids errors when Building the game project
#if UNITY_EDITOR

using UnityEditor;

namespace KevinIglesias {

	//Custom Inspector
	[CustomEditor(typeof(SheathComponentScript))]
	public class SheathScriptCustomInspector : Editor
	{
		string version = "1.1";
		static bool about = false;
		
		public override void OnInspectorGUI()
		{
			var coreScript = target as SheathComponentScript;
		 
			GUILayout.Space(10);

			//HEADER
				if(about)
				{
					GUILayout.BeginHorizontal();
				
					GUI.skin.label.fontSize = 13;
					GUILayout.Label("SHEATH SCRIPT "+version);
					GUI.skin.label.fontSize = 11;
					
					if(GUILayout.Button("▼ About"))
					{
						about = !about;
					}
					
					GUILayout.EndHorizontal();	
					
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("By Kevin Iglesias (support@keviniglesias.com)");
					GUILayout.EndHorizontal();
					
					GUILayout.Space(3);
					
					GUILayout.BeginHorizontal();

					if(GUILayout.Button("DOCUMENTATION"))
					{
						Application.OpenURL("https://www.keviniglesias.com/assets/SheathScript/Documentation.pdf");
					}
					
					GUILayout.EndHorizontal();	
					
					GUILayout.BeginHorizontal();
					if(GUILayout.Button("TUTORIALS"))
					{
						Application.OpenURL("https://www.youtube.com/playlist?list=PLEVwmRZp8_iHVSr4nuoPlDBv5nn6j452k");
					}
					
					if(GUILayout.Button("FORUM"))
					{
						Application.OpenURL("https://forum.unity.com/threads/624106/");
					}
					
					GUILayout.EndHorizontal();	

				}else{
					
					GUILayout.BeginHorizontal();
				
					GUI.skin.label.fontSize = 13;
				
					GUILayout.Label("SHEATH SCRIPT "+version);
					
					GUI.skin.label.fontSize = 11;
					
					if(GUILayout.Button("► About"))
					{
						about = !about;
					}
					
					GUILayout.EndHorizontal();
				}	
				
			//SEPARATOR	
			DrawUILine(Color.black);
			
			
			//SHEATHS
			EditorGUI.BeginChangeCheck();
			
			if(coreScript.sheaths != null)
			{
				for(int i = 0; i < coreScript.sheaths.Count; i++)
				{
					GUILayout.BeginHorizontal();
					GUIContent removeSheath = new GUIContent("[X]", "Remove this Sheath");
					if(GUILayout.Button(removeSheath, GUILayout.Width(33)))
					{
						coreScript.sheaths.RemoveAt(i);
						break;
					}
					GUILayout.Label("Sheath ID "+i.ToString("00")+" | "+coreScript.sheaths[i].sheathName);
					GUILayout.EndHorizontal();
					
					EditorGUI.BeginChangeCheck();
					
					GUILayout.BeginHorizontal();
					GUILayout.Space(10);
					GUIContent sheathContent = new GUIContent("Sheath Name: ", "Write a name for identifying the sheath");
					string iSheathName = EditorGUILayout.TextField(sheathContent, coreScript.sheaths[i].sheathName);
					GUILayout.EndHorizontal();

					if(EditorGUI.EndChangeCheck()) {

						Undo.RegisterUndo(target, "Change Sheath Name "+(i));
						coreScript.sheaths[i].sheathName = iSheathName;
					}
					
					EditorGUI.BeginChangeCheck();
					
					GUILayout.BeginHorizontal();
					GUILayout.Space(10);
					GUIContent weaponContent = new GUIContent("Weapon: ", "Transform of the Weapon to sheath/unsheath");
					Transform iWeapon = EditorGUILayout.ObjectField(weaponContent, coreScript.sheaths[i].weapon, typeof(Transform)) as Transform;
					GUILayout.EndHorizontal();
					
					if(EditorGUI.EndChangeCheck()) {
						Undo.RegisterUndo(target, "Change Weapon"+(i));
						coreScript.sheaths[i].weapon = iWeapon;
					}
					
					if(coreScript.sheaths[i].weapon != null)
					{
						if(coreScript.sheaths[i].weapon.localEulerAngles != Vector3.zero || coreScript.sheaths[i].weapon.localPosition != Vector3.zero)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Space(20);
							GUILayout.Label("!!! WARNING: Weapon Transform not in 0 position or rotation");
							GUILayout.EndHorizontal();
						}
					}
					
					EditorGUI.BeginChangeCheck();
					
					GUILayout.BeginHorizontal();
					GUILayout.Space(10);
					GUIContent handContent = new GUIContent("Weapon Root in Hand: ", "Transform of the Weapon root when unsheathed");
					Transform iHand = EditorGUILayout.ObjectField(handContent, coreScript.sheaths[i].hand, typeof(Transform)) as Transform;	
					GUILayout.EndHorizontal();
					
					if(EditorGUI.EndChangeCheck()) {
						Undo.RegisterUndo(target, "Change Hand"+(i));
						coreScript.sheaths[i].hand = iHand;
					}
					
					EditorGUI.BeginChangeCheck();
					
					GUILayout.BeginHorizontal();
					GUILayout.Space(10);
					GUIContent wSheathContent = new GUIContent("Weapon Root in Sheath: ", "Transform of the Weapon when sheathed");
					Transform iSheath = EditorGUILayout.ObjectField(wSheathContent, coreScript.sheaths[i].sheath, typeof(Transform)) as Transform;
					GUILayout.EndHorizontal();

					if(EditorGUI.EndChangeCheck()) {
						Undo.RegisterUndo(target, "Change Sheath"+(i));
						coreScript.sheaths[i].sheath = iSheath;
					}
					
					GUILayout.Space(3);
				}
			}
			
			GUILayout.BeginHorizontal();
			GUIContent addButton = new GUIContent("[+] Add Sheath", "Add a new Sheath");
			if(GUILayout.Button(addButton))
			{
				if(coreScript.sheaths == null)
				{
					coreScript.sheaths = new List<Sheaths>();
				}
				coreScript.sheaths.Add(null);
			}
			
			GUIContent removeAll = new GUIContent("[X] Remove All", "Clear ALL Sheaths");
			if(GUILayout.Button(removeAll))
			{
				coreScript.sheaths = new List<Sheaths>();
			}
			
			List<Sheaths> iSheaths = coreScript.sheaths;
			GUILayout.EndHorizontal();
			
			if(EditorGUI.EndChangeCheck()) {
				Undo.RegisterUndo(target, "Change Sheath Size");
				coreScript.sheaths = iSheaths;
			}
			
			if (GUI.changed)
			{
				if(!EditorApplication.isPlaying)
				{
					EditorUtility.SetDirty(target);
					EditorApplication.MarkSceneDirty();
				}
			}
		}
		
		//FUNCTION FOR DRAWING A SEPARATOR
		public static void DrawUILine(Color color, int thickness = 1, int padding = 15)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
			r.height = thickness;
			r.y+=padding/2;
			EditorGUI.DrawRect(r, color);
		}
	}
}

#endif
