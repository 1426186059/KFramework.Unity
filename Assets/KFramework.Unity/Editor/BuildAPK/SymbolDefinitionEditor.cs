using UnityEditor;

public class SymbolDefinitionEditor
{
    const string SymbolDefinition = "APP_DEBUG";
    [MenuItem("KFramework/预定义符号/Debug")]
    public static void DoDefineSymbol1()
    {
        DoDefineSymbol(false);
    }

    [MenuItem("KFramework/预定义符号/Release")]
    public static void DoDefineSymbol2()
    {
        DoDefineSymbol(true);
    }

    public static void DoDefineSymbol(bool bReleaseApp)
    {
        string define = SymbolDefinition;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        if (bReleaseApp)
        {
            if (defines.Contains(define))
            {
                int nRemoveBeginIndex = defines.IndexOf(define);
                int nRemoveCount = define.Length;
                defines = defines.Remove(nRemoveBeginIndex, nRemoveCount);
            }
        }
        else
        {
            if (!defines.Contains(define))
            {
                if (defines.EndsWith(";"))
                {
                    defines += define;
                }
                else
                {
                    defines += ";" + define;
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
            }
        }
        
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
    }
}