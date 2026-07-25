using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WWWTools : SingleTonMonoBehaviour<WWWTools>
{
    /// <summary>
    /// post请求 
    /// </summary>
    /// <param name="url"> 请求的url </param>   
    /// <param name="json"> 传输的数据 </param>
    /// <returns></returns>
    public void PostJsonData(string url, string json, Action<bool> result = null)
    {
        Debug.Log("PostJsonData json: " + json);
        StartCoroutine(PostJsonData1(url, json, result));
    }

    private IEnumerator PostJsonData1(string url, string json, Action<bool> result = null)
    {
        UnityWebRequest www = UnityWebRequest.Post(url, json, "text");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();
        bool bSuccess = string.IsNullOrWhiteSpace(www.error);
        result(bSuccess);

        if (bSuccess)
        {
            Debug.Log("WWWHelper PostJsonData Success:" + www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Error: " + www.error);
        }
    }
}
