using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventClickTriggerAllListener : MonoBehaviour, IPointerClickHandler
{
    public GameObject[] mClickObjList;
    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerClickHandler);
    }

    //把事件透下去
    private void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.GetComponent<EventClickTriggerAllListener>() == null)
            {
                if (mClickObjList == null || mClickObjList.Length == 0)
                {
                    ExecuteEvents.Execute(results[i].gameObject, data, function);
                }
                else
                {
                    for (int j = 0; j < mClickObjList.Length; j++)
                    {
                        if (results[i].gameObject == mClickObjList[j])
                        {
                            ExecuteEvents.Execute(results[i].gameObject, data, function);
                        }
                    }
                }
            }
        }
    }
}