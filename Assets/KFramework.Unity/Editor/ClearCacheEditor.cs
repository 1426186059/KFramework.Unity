using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ClearCacheEditor : MonoBehaviour
{
    [MenuItem("KFramework/清理Cache工具/Clear All Cache")]
    private static void Do1()
    {
        PlayerPrefs.DeleteAll();
        Caching.ClearCache();
		Directory.Delete(Application.persistentDataPath, true);

		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();
    }

    [MenuItem("KFramework/清理Cache工具/Clear PlayerPrefs数据库")]
	private static void Do2()
	{
		PlayerPrefs.DeleteAll();
	}

	[MenuItem("KFramework/清理Cache工具/Clear WWW Cache")]
	private static void Do3()
	{
		Caching.ClearCache();
    }

    [MenuItem("KFramework/清理Cache工具/Clear persistentDataPath本地目录")]
    private static void Do4()
    {
        Directory.Delete(Application.persistentDataPath, true);
    }

    

    [MenuItem("KFramework/打开文件夹/Open AppDomain.CurrentDomain.BaseDirectory")]
    private static void Open_AppDomainCurrentDomainBaseDirectory()
    {
        Process.Start(AppDomain.CurrentDomain.BaseDirectory);
    }

    [MenuItem("KFramework/打开文件夹/Open Environment.CurrentDirectory")]
    private static void Open_EnvironmentCurrentDirectory()
    {
        Process.Start(Environment.CurrentDirectory);
    }

    [MenuItem("KFramework/打开文件夹/Open Application.persistentDataPath")]
    private static void OpenPersistentDataPath()
    {
        Process.Start(Application.persistentDataPath);
    }

    [MenuItem("KFramework/打开文件夹/Open Application.streamingAssetsPath")]
    private static void Open_StreamingAssetsPath()
    {
        if(!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }

        Process.Start(Application.streamingAssetsPath);
    }
}