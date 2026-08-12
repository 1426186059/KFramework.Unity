using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonResSerialization2 : MonoBehaviour
{
    [System.Serializable]
    public class Item
    {
        public string assetPath;
        public UnityEngine.Object mObj;
    }

    public List<Item> mAssetList = new List<Item>();
    private readonly Dictionary<string, UnityEngine.Object> mAssetDic = new Dictionary<string, UnityEngine.Object>();
    public UnityEngine.Object GetAsset(string assetPath)
    {
        assetPath = assetPath.ToLower();
        UnityEngine.Object mResult = null;
        if (!mAssetDic.TryGetValue(assetPath, out mResult))
        {
            var Result = mAssetList.Find((x) =>
            {
                return x.assetPath.EndsWith(assetPath);
            });
            
            if (Result != null && Result.mObj != null)
            {
                mResult = Result.mObj;
                mAssetDic[assetPath] = mResult;
            }
        }
        
        return mResult;
    }

    public T GetAsset<T>(string assetPath) where T:UnityEngine.Object
    {
        return GetAsset(assetPath) as T;
    }

    //----------------------------------Editor 相关-----------------------------------------
    [HideInInspector] public string mResFolder = string.Empty;
    [HideInInspector] public string mResSuffix = string.Empty;
}
