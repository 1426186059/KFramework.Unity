using System;
using System.IO;
using UnityEngine;

public static class FileIOTool
{
    public static bool ExistsDirectory(string path)
	{
        path = Path.Combine(Application.persistentDataPath, path);
        return System.IO.Directory.Exists(path);
	}

    public static bool ExistsFile(string path)
    {
        path = Path.Combine(Application.persistentDataPath, path);
        return System.IO.File.Exists(path);
    }

    public static void CreateDirectory(string path)
	{
        path = Path.Combine(Application.persistentDataPath, path);
        System.IO.Directory.CreateDirectory(path);
	}

    public static void DeleteDirectory(string path)
    {
        path = Path.Combine(Application.persistentDataPath, path);
        System.IO.Directory.Delete(path);
    }

    public static void DeleteFile(string path)
    {
        path = Path.Combine(Application.persistentDataPath, path);
        System.IO.File.Delete(path);
    }

    public static void WriteAllText(string path, string content)
    {
        path = Path.Combine(Application.persistentDataPath, path);
        System.IO.File.WriteAllText(path, content);
    }

    public static string ReadAllText(string path)
    {
        path = Path.Combine(Application.persistentDataPath, path);
        return System.IO.File.ReadAllText(path);
    }
    
    public static string GetFilePath(string path)
    {
        path = Path.Combine(Application.persistentDataPath, path);
        return path;
    }
}

