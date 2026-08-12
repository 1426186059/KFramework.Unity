using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildApkEditor
{
    static string LOCATION_PATH_ANDROID = $"APK/{Application.productName}.apk";
    
    [MenuItem("KFramework/打包Apk/打包")]
    public static void ProjectBuild()
    {
        ProjectBuildExecute(BuildTarget.Android);
    }

    public static void ProjectBuildExecute(BuildTarget target)
    {
        SwitchPlatform(target);

        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;

        List<string> scenes = new List<string>();
        foreach (UnityEditor.EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                if (System.IO.File.Exists(scene.path))
                {
                    Debug.Log("Add Scene (" + scene.path + ")");
                    scenes.Add(scene.path);
                }
            }
        }
        
        BuildReport report = BuildPipeline.BuildPlayer(scenes.ToArray(), LOCATION_PATH_ANDROID, target, BuildOptions.None);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("打包失败。(" + report.summary.ToString() + ")");
        }
    }

    private static void SwitchPlatform(BuildTarget target)
    {
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            if (target == BuildTarget.iOS)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }
            if (target == BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
            else if (target == BuildTarget.WebGL)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }
        }
    }
}