using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public static class UnityEngineObjectExtention
{
    public static RectTransform rectTransform(this GameObject go)
    {
        return go.transform as RectTransform;
    }

    public static RectTransform rectTransform(this Transform go)
    {
        return go as RectTransform;
    }

    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
        {
            return result;
        }

        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public static void SetAlpha(this Image obj, float alpha)
    {
        Color oriColor = obj.color;
        obj.color = new Color(oriColor.r, oriColor.g, oriColor.b, alpha);
    }

    public static T AddMissComponent<T>(this GameObject obj) where T : Component
    {
        T t = obj.GetComponent<T>();
        if (t == null)
        {
            t = obj.AddComponent<T>();
        }
        return t;
    }

    public static Component AddMissComponent(this GameObject go, Type t)
    {
        if (null != go)
        {
            var component = go.GetComponent(t);
            if (null == component)
            {
                component = go.AddComponent(t);
            }
            return component;
        }
        Debug.LogWarning("要添加组件的物体为空！");
        return null;
    }
    
    public static void removeAllChildren(this GameObject obj)
    {
        foreach(Transform v in obj.transform)
        {
            UnityEngine.Object.Destroy(v.gameObject);
        }
    }

    public static string GetTreeName(this GameObject go)
    {
        string treeName = go.name;
        Transform tempTran = go.transform;
        while(tempTran.parent != null)
        {
            tempTran = tempTran.parent;
            treeName = tempTran.name + "/" + treeName;
        }
        return treeName;
    }
}
