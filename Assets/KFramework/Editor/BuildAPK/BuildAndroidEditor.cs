#if UNITY_ANDROID

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildApkEditor
{
    const BuildTargetGroup mBuildTargetGroup = BuildTargetGroup.Android;
    static string LOCATION_PATH_ANDROID = $"APK/{Application.productName}";
    const string SymbolDefinition = "APP_DEBUG";
    static bool bReleaseApp = false;
    static bool bGoogleApp = false;
    
    private static void InitParam()
    {
        bReleaseApp = false;
        bGoogleApp = false;
    }
    
    [MenuItem("安卓打包/Debug 打包")]
    public static void ProjectBuild_Debug()
    {
        InitParam();
        DoBuild();
    }

    [MenuItem("安卓打包/Release 打包")]
    public static void ProjectBuild_Release()
    {
        InitParam();
        bReleaseApp = true;
        DoBuild();
    }

    [MenuItem("安卓打包/Release 打包-Google")]
    public static void ProjectBuild_Release_Google()
    {
        InitParam();
        bReleaseApp = true;
        bGoogleApp = true;
        DoBuild();
    }

    public static void DoBuild()
    {
        DoPlayerSettings();
        DoBuildSettings();

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

        string appPath = LOCATION_PATH_ANDROID;
        if(bGoogleApp)
        {
            appPath += ".aab";
        }
        else
        {
            appPath += ".apk";
        }
        
        BuildReport report = BuildPipeline.BuildPlayer(scenes.ToArray(), appPath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.AutoRunPlayer);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("打包成功: " + report.summary.outputPath);
        }
        else
        {
            Debug.LogError("打包失败: " + report.summary.ToString());
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
        }
    }

    private static void DoBuildSettings()
    {
        EditorUserBuildSettings.buildAppBundle = bGoogleApp;
        EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Disabled;
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        Debug.Log("EditorUserBuildSettings.remoteDeviceUsername: " + EditorUserBuildSettings.remoteDeviceUsername);
        Debug.Log("EditorUserBuildSettings.remoteDeviceAddress: " + EditorUserBuildSettings.remoteDeviceAddress);
        Debug.Log("EditorUserBuildSettings.remoteDeviceInfo: " + EditorUserBuildSettings.remoteDeviceInfo);
        Debug.Log("EditorUserBuildSettings.remoteDeviceExports: " + EditorUserBuildSettings.remoteDeviceExports);

        EditorUserBuildSettings.remoteDeviceInfo = true;
    }

    private static void DoPlayerSettings()
    {
        PlayerSettings.applicationIdentifier = "com.yugong.ooxxsolitaire";
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystorePass = "xu825126369ke";
        PlayerSettings.Android.keyaliasName = "game";
        PlayerSettings.Android.keyaliasPass = "xu825126369ke";
        PlayerSettings.Android.renderOutsideSafeArea = false;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        DoDefineSymbol();
    }

    private static void DoDefineSymbol()
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
#endif