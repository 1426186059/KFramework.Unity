using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CommonEditor
{
    [MenuItem("KFramework/Tools/清理 空 文件夹")]
    private static void ClearEmptyFolder()
    {
        int i = 0;
        while (i++ < 10)
        {
            FileToolEditor.ClearEmptyFolder("Assets/");
            Debug.Log("清理中...");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static BuildTarget GetBuildTarget()
    {
        BuildTarget target = BuildTarget.StandaloneWindows64;

#if UNITY_IPHONE
		target = BuildTarget.iOS;
#elif UNITY_ANDROID
        target = BuildTarget.Android;
#endif

        return target;
    }
}
