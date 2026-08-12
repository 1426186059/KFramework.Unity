using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

[CustomEditor(typeof(CommonResSerialization)), CanEditMultipleObjects]
public class CommonResSerializationEditor : Editor
{
	private CommonResSerialization mTarget;
	private static int tab=0;

	private void OnEnable()
	{
		mTarget = target as CommonResSerialization;
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
        var mNowPath = AssetDatabase.GetAssetPath(mTarget);
        if(string.IsNullOrWhiteSpace(mNowPath))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(mTarget.mResFolder))
        {
            mTarget.mResFolder = Path.GetDirectoryName(mNowPath);
        }

        if (string.IsNullOrWhiteSpace(mTarget.mResSuffix))
        {
            mTarget.mResSuffix = ".prefab;";
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset Folder: ");
        mTarget.mResFolder = EditorGUILayout.TextField(mTarget.mResFolder);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset Suffix: ");
        mTarget.mResSuffix = EditorGUILayout.TextField(mTarget.mResSuffix);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add Asset From Folder Path"))
        {
            var suffixArray = mTarget.mResSuffix.Split(";");
            foreach (var v in Directory.GetFiles(mTarget.mResFolder, "*", SearchOption.AllDirectories))
			{
                string extention = Path.GetExtension(v);
                if (!v.EndsWith(".meta") && (string.IsNullOrWhiteSpace(mTarget.mResSuffix) || Array.IndexOf(suffixArray, extention) >= 0))
				{
					var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(v);
					if (obj is GameObject)
					{
						var obj1 = obj as GameObject;
						if (obj1 != mTarget.gameObject)
						{
							if (!mTarget.m_PrefabList.Contains(obj1))
							{
								mTarget.m_PrefabList.Add(obj1);
							}
						}
					}
                    else if (obj is SpriteAtlas)
                    {
                        var obj1 = obj as SpriteAtlas;
                        if (!mTarget.m_AtlasList.Contains(obj1))
                        {
                            mTarget.m_AtlasList.Add(obj1);
                        }
                    }
                    else if (obj is Texture)
                    {
                        var obj1 = obj as Texture;

						var obj2 = AssetDatabase.LoadAllAssetsAtPath(v);
						if (obj2.Length > 1)
						{
							foreach (var v2 in obj2)
							{
								if (v2 is Sprite)
								{
									var obj3 = v2 as Sprite;
									if (!mTarget.m_SpriteList.Contains(obj3))
									{
										mTarget.m_SpriteList.Add(obj3);
									}
								}
                            }
						}
						else
						{
							if (!mTarget.m_TextureList.Contains(obj1))
							{
								mTarget.m_TextureList.Add(obj1);
							}
						}
                    }
                    else if (obj is AudioClip)
                    {
                        var obj1 = obj as AudioClip;
                        if (!mTarget.m_AudoClipList.Contains(obj1))
                        {
                            mTarget.m_AudoClipList.Add(obj1);
                        }
                    }
                    else if (obj is Shader)
                    {
                        var obj1 = obj as Shader;
                        if (!mTarget.m_ShaderList.Contains(obj1))
                        {
                            mTarget.m_ShaderList.Add(obj1);
                        }
                    }
                    else if (obj is Material)
                    {
                        var obj1 = obj as Material;
                        if (!mTarget.m_MaterialList.Contains(obj1))
                        {
                            mTarget.m_MaterialList.Add(obj1);
                        }
                    }
                    else if (obj is TextAsset)
                    {
                        var obj1 = obj as TextAsset;
                        if (!mTarget.m_TextAssetList.Contains(obj1))
                        {
                            mTarget.m_TextAssetList.Add(obj1);
                        }
                    }
                }
            }

            EditorUtility.SetDirty(mTarget);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("一键绑定 GameObject"))
		{
			foreach (var v in Directory.GetFiles(mTarget.mResFolder, "*", SearchOption.AllDirectories))
			{
				if (v.EndsWith(".prefab"))
				{
					var mResObj = AssetDatabase.LoadAssetAtPath<GameObject>(v);
					if (mTarget.m_PrefabList.IndexOf(mResObj) == -1)
					{
						mTarget.m_PrefabList.Add(mResObj);
					}
				}
            }

			EditorUtility.SetDirty(mTarget);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

        if (GUILayout.Button("一键绑定 SpriteAtlas"))
        {
            foreach (var v in Directory.GetFiles(mTarget.mResFolder, "*", SearchOption.AllDirectories))
            {
                if (v.EndsWith(".spriteatlasv2"))
                {
                    var mResObj = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(v);
					if (mTarget.m_AtlasList.IndexOf(mResObj) == -1)
					{
						mTarget.m_AtlasList.Add(mResObj);
					}
                }
            }

            EditorUtility.SetDirty(mTarget);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("一键绑定 Mp3"))
        {
            foreach (var v in Directory.GetFiles(mTarget.mResFolder, "*", SearchOption.AllDirectories))
            {
                if (v.EndsWith(".mp3"))
                {
                    var resObj = AssetDatabase.LoadAssetAtPath<AudioClip>(v);
                    if (mTarget.m_AudoClipList.IndexOf(resObj) == -1)
                    {
                        mTarget.m_AudoClipList.Add(resObj);
                    }
                }
            }

            EditorUtility.SetDirty(mTarget);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("一键绑定 Sprite"))
        {
            foreach (var v in Directory.GetFiles(mTarget.mResFolder, "*", SearchOption.AllDirectories))
            {
                if (v.EndsWith(".png") || v.EndsWith(".jpg"))
                {
                    var resObj = AssetDatabase.LoadAssetAtPath<Sprite>(v);
					if (mTarget.m_SpriteList.IndexOf(resObj) == -1)
					{
						mTarget.m_SpriteList.Add(resObj);
					}
                }
            }

            EditorUtility.SetDirty(mTarget);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        
        if (GUILayout.Button("一键绑定 Text/Json"))
        {
            foreach (var v in Directory.GetFiles(mTarget.mResFolder, "*", SearchOption.AllDirectories))
            {
                if (v.EndsWith(".txt") || v.EndsWith(".json"))
                {
                    var resObj = AssetDatabase.LoadAssetAtPath<TextAsset>(v);
                    if (mTarget.m_TextAssetList.IndexOf(resObj) == -1)
                    {
                        mTarget.m_TextAssetList.Add(resObj);
                    }
                }
            }

            EditorUtility.SetDirty(mTarget);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}