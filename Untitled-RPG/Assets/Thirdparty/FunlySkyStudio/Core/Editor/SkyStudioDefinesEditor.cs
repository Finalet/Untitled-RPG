using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio {
	[InitializeOnLoad]
	public class SkyStudioDefinesEditor : Editor {
			static SkyStudioDefinesEditor()
			{
				var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
				if (!symbols.Contains("SKY_STUDIO_PRESENT"))
				{
					symbols += ";" + "SKY_STUDIO_PRESENT";
					PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
				}
			}
	}
}
