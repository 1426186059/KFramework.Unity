//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
//using UnityEngine;

//public class SceneCopyEditor
//{
//	[MenuItem("KFramework/Copy Scene")]
//	public static void Do()
//	{
//        foreach (string v in Directory.GetFiles("Assets/Scenes/", "*", SearchOption.AllDirectories))
//        {
//            string fileName = Path.GetFileName(v);
//            if (!fileName.EndsWith(".meta") && !fileName.Contains("GameLauncher"))
//            {
//                File.Copy(v, GameConst.ResRootDir + "Scenes/" + fileName, true);
//            }
//        }

//		AssetDatabase.SaveAssets();
//		AssetDatabase.Refresh();
//		Debug.Log("Copy Scene Success !");
//	}
//}

