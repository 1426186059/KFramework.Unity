using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CommonResSerialization2)), CanEditMultipleObjects]
public class CommonResSerialization2Editor : Editor
{
    private CommonResSerialization2 mTarget;

    private void OnEnable()
    {
        mTarget = target as CommonResSerialization2;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }

    protected void DrawInspectorGUI()
    {
        base.DrawDefaultInspector();
        DrawMyInspector();
    }

    private void DrawMyInspector()
    {
        if (string.IsNullOrWhiteSpace(mTarget.mResFolder))
        {
            mTarget.mResFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mTarget));
        }

        if (string.IsNullOrWhiteSpace(mTarget.mResSuffix))
        {
            mTarget.mResSuffix = ".prefab;.txt;.json";
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset Folder: ");
        mTarget.mResFolder = EditorGUILayout.TextField(mTarget.mResFolder);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset Suffix: ");
        mTarget.mResSuffix = EditorGUILayout.TextField(mTarget.mResSuffix);
        EditorGUILayout.EndHorizontal();

        foreach (var v in mTarget.mAssetList)
        {
            if (v.mObj != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(v.mObj);
                if (assetPath != null)
                {
                    v.assetPath = assetPath.ToLower();
                }
                else
                {
                    v.assetPath = null;
                }
            }
        }

        if (GUILayout.Button("Add Asset From Folder Path"))
        {
            DoAddAssetFromFolder(mTarget);
        }
    }

    public static void DoAddAssetFromFolder(CommonResSerialization2 mTarget)
    {
        var suffixArray = mTarget.mResSuffix.Split(";");
        foreach (var v in Directory.GetFiles(mTarget.mResFolder, "*", SearchOption.AllDirectories))
        {
            string extention = Path.GetExtension(v);
            if (!v.EndsWith(".meta") && (string.IsNullOrWhiteSpace(mTarget.mResSuffix) || Array.IndexOf(suffixArray, extention) >= 0))
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(v);
                if (obj != null && obj != mTarget.gameObject)
                {
                    if (mTarget.mAssetList.Find((x) => x.mObj == obj) == null)
                    {
                        var mItem = new CommonResSerialization2.Item();
                        mItem.assetPath = v.ToLower();
                        mItem.mObj = obj;
                        mTarget.mAssetList.Add(mItem);
                    }
                }
            }
        }

        EditorUtility.SetDirty(mTarget);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}