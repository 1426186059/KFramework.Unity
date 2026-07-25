using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 经测试，如果是纯C# 调用，这种方法不可取。
public class FindCacheMgr
{
    private static Dictionary<Transform, List<Component>> mComponentDic = new Dictionary<Transform, List<Component>>();
    private static Dictionary<string, Transform> mObjDic = new Dictionary<string, Transform>();

    public static void Clear()
    {
        mObjDic = new Dictionary<string, Transform>();
        mComponentDic = new Dictionary<Transform, List<Component>>();
    }

    public static Transform FindDeepChild(Transform obj, string path)
    {
        Transform mResult = null;
        if (!mObjDic.TryGetValue(path, out mResult))
        {
            mResult = obj.transform.FindDeepChild(path);
            if (mResult)
            {
                mObjDic[path] = mResult;
            }
        }

        return mResult;
    }

    public static Component GetComponent(Transform obj, Type type)
    {
        List<Component> mResult = null;
        if (!mComponentDic.TryGetValue(obj, out mResult))
        {
            var mCom = obj.GetComponent(type);
            if (mCom != null)
            {
                mResult = new List<Component>();
                mResult.Add(mCom);
            }
        }

        return mResult.Find((x)=>x.GetType() == type);
    }

    public static Component GetComponent(Transform obj, string path, Type type)
    {
        Transform subObj = FindDeepChild(obj, path);
        if (subObj == null) return null;
        return GetComponent(subObj, type);
    }
}
