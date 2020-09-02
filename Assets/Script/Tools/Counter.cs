using System;
using System.Collections;
using UnityEngine;

public class Counter : Singleton<Counter>
{
    /// <summary>
    /// CountDown and get coroutine data, and set float with setter.
    /// </summary>
    public Coroutine CountDown(float timer, bool ignoreTimeScale, Action<float> setter)
    {
        return StartCoroutine(StartCountDown(timer, ignoreTimeScale, setter));
    }

    /// <summary>
    /// CountDown and get coroutine data, and set float with setter.
    /// </summary>
    public Coroutine CountDown(float timer, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
    {
        return StartCoroutine(StartCountDown(timer, ignoreTimeScale, setter, callback));
    }

    /// <summary>
    /// CountDown a number within duration, and set float with setter.
    /// And get coroutine data.
    /// </summary>
    public Coroutine CountDownInTimes(float originNumber, float endNumber, float duration, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
    {
        if (duration <= 0)
        {
            setter.Invoke(endNumber);
            return null;
        }
        return StartCoroutine(StartCountInTimes(originNumber, endNumber, duration, ignoreTimeScale, setter, callback));
    }

    /// <summary>
    /// CountDown a number within duration, and callback action at the end.
    /// And get coroutine data.
    /// </summary>
    public Coroutine CountDownInTimes(float originNumber, float endNumber, float duration, bool ignoreTimeScale, params Action[] callback)
    {
        return StartCoroutine(StartCountInTimes(originNumber, endNumber, duration, ignoreTimeScale, callback));
    }

    #region IEnumerator
    private IEnumerator StartCountDown(float timer, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
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

    private IEnumerator StartCountInTimes(float originNumber, float endNumber, float duration, bool ignoreTimeScale, Action<float> setter, params Action[] callback)
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

    private IEnumerator StartCountInTimes(float originNumber, float endNumber, float duration, bool ignoreTimeScale, params Action[] callback)
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

            timeleft -= Time.deltaTime;
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
