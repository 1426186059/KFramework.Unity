using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DbSaveByOnApplicationPauseMgr : SingleTonMonoBehaviour<DbSaveByOnApplicationPauseMgr>
{
	private readonly List<Action> mPauseEventList = new List<Action>();
	
	void OnApplicationPause(bool pauseStatus)
	{
		if(pauseStatus)
		{
			this.Fire();
		}
	}

	void OnDisable()
	{
		this.Fire();
	}

	private void Fire()
	{
		for(int i = 0; i < mPauseEventList.Count; i++)
        {
            Action mEvent = mPauseEventList[i];
            mEvent();
        }
	}

	public void AddListener(Action mFunc)
	{
        mPauseEventList.Add(mFunc);
	}

    public void RemoveListener(Action mFunc)
	{
        mPauseEventList.Remove(mFunc);
	}
}
