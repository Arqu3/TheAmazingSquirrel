using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    float timer = 0.0f;

    bool timeOut = false;

    public void ResetData()
    {
        timer = 0.0f;
        timeOut = false;
    }

    public float GetRemainingTime()
    {
        return 600f - timer;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (GetRemainingTime() <= 0f && !timeOut)
        {
            timeOut = true;
            HUD.Current.onGameFinished.Invoke();
        }

        HUD.Current?.UpdateTime(GetRemainingTime());
    }
}
