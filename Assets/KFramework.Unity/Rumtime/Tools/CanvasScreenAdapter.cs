using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScreenAdapter : MonoBehaviour
{
    void Start()
    {
        CanvasScaler mCanvasScaler = GetComponent<CanvasScaler>();
        float fRatio = Screen.height / (float)Screen.width;
        //if(fRatio <= 4 / 3f + 0.01f)
        //{
        //    mCanvasScaler.matchWidthOrHeight = 1.0f;
        //}
        //else
        //{
        //    mCanvasScaler.matchWidthOrHeight = 0f;
        //}
    }
}
