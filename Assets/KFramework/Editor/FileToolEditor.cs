using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FileToolEditor
{
    public static void OpenFolder(string path)
    {
        if (Directory.Exists(path))
        {
            System.Diagnostics.Process.Start(path);
        }

    }

    public static void CopyFolder(Dictionary<string, string> copyDic)
    {
        foreach (KeyValuePair<string, string> path in copyDic)
        {
            if (Directory.Exists(path.Key))
            {

                CopyDir(path.Key, path.Value);
                Debug.Log("Copy Success : \n\tFrom:" + path.Key + " \n\tTo:" + path.Value);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    public static void CopyFolder(string fromPath, string toPath)
    {
        CopyDir(fromPath, toPath);
        Debug.Log("Copy Success : From: " + fromPath + " To: " + toPath);
        EditorUtility.ClearProgressBar();
    }

    public static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            File.Delete(path + ".meta");
        }
    }

    public static string GetDirName(string path)
    {
        if (Directory.Exists(path))
        {
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            int nIndex = path.LastIndexOf("/");
            return path.Substring(nIndex + 1);
        }

        return null;
    }

    public static string GetDirParentDir(string path)
    {
        if (Directory.Exists(path))
        {
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            int nIndex = path.LastIndexOf("/");
            return path.Substring(0, nIndex);
        }

        return null;
    }

    private static void CopyDir(string origin, string target)
    {
        if (!Directory.Exists(target))
        {
            Directory.CreateDirectory(target);
        }

        DirectoryInfo info = new DirectoryInfo(origin);
        FileInfo[] fileList = info.GetFiles();
        DirectoryInfo[] dirList = info.GetDirectories();
        float index = 0;
        foreach (FileInfo fi in fileList)
        {
            if (fi.Extension == ".meta")
            {
                continue;
            }

            float progress = (index / (float)fileList.Length);
            EditorUtility.DisplayProgressBar("Copy ", "Copying: " + Path.GetFileName(fi.FullName), progress);
            File.Copy(fi.FullName, target + fi.Name, true);
            index++;
        }

        foreach (DirectoryInfo di in dirList)
        {
            CopyDir(di.FullName, target + "\\" + di.Name);
        }
    }

    public static void ClearEmptyFolder(string Dir)
    {
        foreach (var v in Directory.GetDirectories(Dir, "*", SearchOption.TopDirectoryOnly))
        {
            bool isEmptyFolder = true;
            foreach (string v1 in Directory.GetFiles(v, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetExtension(v1) != ".meta")
                {
                    isEmptyFolder = false;
                }
            }

            if(isEmptyFolder)
            {
                isEmptyFolder = Directory.GetDirectories(v, "*", SearchOption.TopDirectoryOnly).Length == 0;
            }
            
            if (isEmptyFolder)
            {
                DeleteFolder(v);
                Debug.Log("Empty Folder: " + v);
            }
            else
            {
                ClearEmptyFolder(v);
            }
        }
    }



}