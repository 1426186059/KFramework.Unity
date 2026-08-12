#if UNITY_IOS

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildApkEditor
{
    static string LOCATION_PATH_ANDROID = $"APK/{Application.productName}.apk";
    string SymbolDefinition = "APP_DEBUG";
    static bool bReleaseApp = false;
    static bool bGoogleApp = false;

    private static void InitParam()
    {
        bReleaseApp = false;
        bGoogleApp = false;
    }


    [MenuItem("打包Apk/Debug 打包")]
    public static void ProjectBuild_Debug()
    {
        InitParam();
        ProjectBuildExecute();
    }

    [MenuItem("打包Apk/Release 打包")]
    public static void ProjectBuild_Release()
    {
        InitParam();
        bReleaseApp = true;
        ProjectBuildExecute();
    }

    [MenuItem("打包Apk/Release 打包-Google")]
    public static void ProjectBuild_Release_Google()
    {
        InitParam();
        bReleaseApp = true;
        bGoogleApp = true;
        ProjectBuildExecute();
    }

    public static void ProjectBuildExecute()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        SwitchPlatform(target);
        
        bool buildAppBundle = EditorUserBuildSettings.buildAppBundle;
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
        }
    }

    private static void DoPlatform(BuildTarget target)
    {
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            if (target == BuildTarget.iOS)
            {
                
            }
            if (target == BuildTarget.Android)
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystorePass = "xu825126369ke";
                PlayerSettings.Android.keyaliasName = "game";
                PlayerSettings.Android.keyaliasPass = "xu825126369ke";
                PlayerSettings.Android.renderOutsideSafeArea = false;
            }
        }

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        NamedBuildTarget mTarget = new NamedBuildTarget();
        mTarget.TargetName = "Hello";
        PlayerSettings.SetScriptingDefineSymbols(mTarget, "");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
    }

    public static bool EnableBuildDefine(string define)
    {
        bool wasDefineAdded = false;
        Debug.LogWarning("Please ignore errors \"PlayerSettings Validation: Requested build target group doesn't exist\" below");
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (IsInvalidGroup(group))
                continue;

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (!defines.Contains(define))
            {
                wasDefineAdded = true;
                if (defines.EndsWith(";", System.StringComparison.Ordinal))
                    defines += define;
                else
                    defines += ";" + define;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
            }
        }
        Debug.LogWarning("Please ignore errors \"PlayerSettings Validation: Requested build target group doesn't exist\" above");

        if (wasDefineAdded)
        {
            Debug.LogWarning("Setting Scripting Define Symbol " + define);
        }
        else
        {
            Debug.LogWarning("Already Set Scripting Define Symbol " + define);
        }
        return wasDefineAdded;
    }

    public static bool DisableBuildDefine(string define)
    {

        bool wasDefineRemoved = false;
        foreach (BuildTargetGroup group in System.Enum.GetValues(typeof(BuildTargetGroup)))
        {
            if (IsInvalidGroup(group))
                continue;

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            if (defines.Contains(define))
            {
                wasDefineRemoved = true;
                if (defines.Contains(define + ";"))
                    defines = defines.Replace(define + ";", "");
                else
                    defines = defines.Replace(define, "");

                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
            }
        }

        if (wasDefineRemoved)
        {
            Debug.LogWarning("Removing Scripting Define Symbol " + define);
        }
        else
        {
            Debug.LogWarning("Already Removed Scripting Define Symbol " + define);
        }
        return wasDefineRemoved;
    }
}

#endif