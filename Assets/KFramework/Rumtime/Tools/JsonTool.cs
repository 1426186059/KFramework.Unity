using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class JsonTool
{
    public static T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static string ToJson(object t)
    {
        return JsonConvert.SerializeObject(t);
    }
}
