
using UnityEditor;

namespace AwesomeTechnologies.TouchReact
{
    [InitializeOnLoad]
    public class CompilerDefineTouchReact : Editor
    {
        private static readonly string _EditorSymbol = "TOUCH_REACT";

        static CompilerDefineTouchReact()
        {
            var symbols =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!symbols.Contains(_EditorSymbol))
            {
                symbols += ";" + _EditorSymbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    symbols);
            }
        }
    }
}
