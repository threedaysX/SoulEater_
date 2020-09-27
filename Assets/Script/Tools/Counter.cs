using System;
using System.Collections;
using UnityEngine;

public class Counter : Singleton<Counter>
{
    /// <summary>
    /// CountDown and get coroutine data, and set float with setter.
    /// </summary>
    public Coroutine StartCountDown(float timer, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
    {
        return StartCoroutine(CountDownCoroutine(timer, ignoreTimeScale, setter, callback));
    }

    public Coroutine StartCountDown(MonoBehaviour where, float timer, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
    {
        return where.StartCoroutine(CountDownCoroutine(timer, ignoreTimeScale, setter, callback));
    }

    /// <summary>
    /// CountDown a number within duration, and set float with setter.
    /// And get coroutine data.
    /// </summary>
    public Coroutine StartCountDownInTimes(float originNumber, float endNumber, float duration, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
    {
        if (duration <= 0)
        {
            setter.Invoke(endNumber);
            return null;
        }
        return StartCoroutine(CountInTimesCoroutine(originNumber, endNumber, duration, ignoreTimeScale, setter, callback));
    }

    public Coroutine StartCountDownInTimes(MonoBehaviour where, float originNumber, float endNumber, float duration, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
    {
        if (duration <= 0)
        {
            setter.Invoke(endNumber);
            return null;
        }
        return where.StartCoroutine(CountInTimesCoroutine(originNumber, endNumber, duration, ignoreTimeScale, setter, callback));
    }

    public void StopCountDown(Coroutine which)
    {
        this.StopCoroutine(which);
    }

    public void StopCountDown(MonoBehaviour where, Coroutine which)
    {
        where.StopCoroutine(which);
    }

    public void StopAllCountDown()
    {
        this.StopAllCoroutines();
    }
    
    public void OnDisableCall()
    {
        StopAllCountDown();
    }

    #region IEnumerator
    private IEnumerator CountDownCoroutine(float timer, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
    {
        while (timer > 0)
        {
            timer -= GetDeltaTime(ignoreTimeScale);
            if (setter != null)
            {
                setter.Invoke(timer);
            }
            yield return timer;
        }

        foreach (Action item in callback)
        {
            item.Invoke();
        }
    }

    private IEnumerator CountInTimesCoroutine(float originNumber, float endNumber, float duration, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
    {
        float timeleft = duration;
        float step = originNumber - endNumber;
        float resultCount = originNumber;
        while (timeleft > 0)
        {
            float trueDeltaTime = GetDeltaTime(ignoreTimeScale);
            if (timeleft > trueDeltaTime)
                resultCount -= (step * trueDeltaTime / duration);
            else
                resultCount -= (step * timeleft / duration);

            timeleft -= trueDeltaTime;
            if (setter != null)
            {
                setter.Invoke(resultCount);
            }
            yield return resultCount;
        }

        foreach (Action item in callback)
        {
            item.Invoke();
        }
    }

    private float GetDeltaTime(bool isUnScaled)
    {
        if (isUnScaled)
        {
            return Time.unscaledDeltaTime;
        }
        else
        {
            return Time.deltaTime;
        }
    }
    #endregion
}
