using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class FindAssetDependenciesEditor
{
    private static readonly Dictionary<string, List<string>> referenceCacheDic = new Dictionary<string, List<string>>();
    [MenuItem("Assets/KFramework/查找 选中的这个资源 被引用（被依赖）的所有其他资源")]
    static void Do1()
    {
        CollectAllDepend();
        
        // 获取所有选中 文件、文件夹的 GUID
        string[] guids = Selection.assetGUIDs;
        foreach (var guid in guids)
        {
            // 将 GUID 转换为 路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (Directory.Exists(assetPath))
            {
                PrintDirDependInfo(assetPath);
            }
            else
            {
                PrintDependInfo(assetPath);
            }
        }
    }

    [MenuItem("Assets/KFramework/查找 选中的这个资源 所有依赖")]
    static void Do2()
    {
        // 获取所有选中 文件、文件夹的 GUID
        string[] guids = Selection.assetGUIDs;
        foreach (var guid in guids)
        {
            // 将 GUID 转换为 路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (Directory.Exists(assetPath))
            {
                Dictionary<string, List<string>> mDic = new Dictionary<string, List<string>>();
                foreach (var v in Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories))
                {
                    foreach (var v2 in AssetDatabase.GetDependencies(v, false))
                    {
                        if (!v2.Contains(assetPath) && !v2.EndsWith(".cs"))
                        {
                            List<string> mList2 = null;
                            if (!mDic.TryGetValue(v, out mList2))
                            {
                                mList2 = new List<string>();
                                mDic[v] = mList2;
                            }
                            mList2.Add(v2);
                        }
                    }
                }

                int nCount = 0;
                foreach (var v in mDic)
                {
                    foreach (var v2 in v.Value)
                    {
                        Debug.Log($"{v} 依赖: {v2}");
                        nCount++;
                    }
                }
                Debug.Log($"目录 {assetPath} 依赖资源数量  {nCount}");
            }
            else
            {
                string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);
                foreach (var file in dependencies)
                {
                    Debug.Log("依赖: " + file);
                }
                Debug.Log($"文件 {assetPath} 依赖资源数量  {dependencies.Length}");
            }
        }
    }

    public static Dictionary<string, List<string>> GetAssetDirDependList(string dirPath)
    {
        Dictionary<string, List<string>> mDic = new Dictionary<string, List<string>>();
        foreach (var v in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
        {
            foreach (var v2 in GetAssetDependList(v))
            {
                if (!v2.Contains(dirPath))
                {
                    List<string> mList2 = null;
                    if (!mDic.TryGetValue(v, out mList2))
                    {
                        mList2 = new List<string>();
                        mDic[v] = mList2;
                    }
                    mList2.Add(v2);
                }
            }
        }
        return mDic;
    }

    public static List<string> GetAssetDependList(string filePath)
    {
        if(referenceCacheDic.Count == 0)
        {
            CollectAllDepend();
        }
        
        List<string> list = null;
        if (!referenceCacheDic.TryGetValue(filePath, out list))
        {
            list = new List<string>();
        }

        return list;
    }

    // 收集项目中所有依赖关系
    public static void CollectAllDepend()
    {
        referenceCacheDic.Clear();
        int count = 0;
        // 获取 Assets 文件夹下所有资源
        string[] guids = AssetDatabase.FindAssets("");
        foreach (string guid in guids)
        {
            // 将 GUID 转换为路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 获取文件所有直接依赖的资源
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

            foreach (var filePath in dependencies)
            {
                // dependency 被 assetPath 依赖了
                // 将所有依赖关系存储到字典中
                List<string> list = null;
                if (!referenceCacheDic.TryGetValue(filePath, out list))
                {
                    list = new List<string>();
                    referenceCacheDic[filePath] = list;
                }
                list.Add(assetPath);
            }

            count++;
            EditorUtility.DisplayProgressBar("Search Dependencies", "Dependencies", (float)(count * 1.0f / guids.Length));
        }

        EditorUtility.ClearProgressBar();
    }

    // 判断文件是否被依赖
    static void PrintDependInfo(string filePath)
    {
        Debug.Log($"收集被引用的信息数量: " + referenceCacheDic.Count);
        List<string> list = GetAssetDependList(filePath);
        foreach (var file in list)
        {
            Debug.Log(filePath + "   被: " + file + "   引用");
        }
        Debug.Log($"文件 {filePath} 被引用数量  {list.Count}");
    }

    static void PrintDirDependInfo(string dirPath)
    {
        Debug.Log($"收集被引用的信息数量: " + referenceCacheDic.Count);
        int nCount = 0;
        var mDic = GetAssetDirDependList(dirPath);
        foreach (var v in mDic)
        {
            foreach (var v2 in v.Value)
            {
                Debug.Log($"{v} 被: {v2} 引用");
                nCount++;
            }
        }
        Debug.Log($"目录 {dirPath} 被引用数量  {nCount}");
    }

}
