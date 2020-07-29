
using UnityEditor;

namespace AwesomeTechnologies.Common
{
    [InitializeOnLoad]
    public class CompilerDefine : Editor
    {
        private static readonly string _EditorSymbol = "VEGETATION_STUDIO_PRO";

        static CompilerDefine()
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
