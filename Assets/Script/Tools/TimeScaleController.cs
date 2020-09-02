﻿using System.Collections;
using UnityEngine;

public class TimeScaleController : Singleton<TimeScaleController>
{
    private void Start()
    {
        TimeScale.global = new TimeScaleData();
    }

    private void Update()
    {
        Time.timeScale = TimeScale.global.currentTimeScale;
    }

    /// <summary>
    /// Custom for UI use.
    /// (Flexable in any project).
    /// </summary>
    public void OpenUI(bool open)
    {
        if (open)
        {
            TimeScale.global.PauseMotion(true);
            TimeScale.global.originTimeScale = TimeScale.global.currentTimeScale;
            TimeScale.global.currentTimeScale = 0f;
        }
        else
        {
            TimeScale.global.PauseMotion(false);
            TimeScale.global.currentTimeScale = TimeScale.global.originTimeScale;
        }
    }

    public void ResetTimeScale(bool forceReset = false)
    {
        if (!forceReset && TimeScale.global.forced)
        {
            return;
        }
        TimeScale.global.currentTimeScale = 1f;
    }

    /// <summary>
    /// Set time scale to factor, 
    /// after delay, time scale will recover within slowdownLength(duration)
    /// </summary>
    /// <param name="slowdownFactor">Set to time scale</param>
    /// <param name="recoverDelay">Delay duration with recovering</param>
    /// <param name="slowdownLength">Recover time scale duration</param>
    /// <param name="recoverFactor">How much will time scale recover to</param>
    /// <param name="forced">If true, this slow motion can not be overwritted by others【DoSlowMotion】, it will run to end.</param>
    public void DoSlowMotion(float slowdownFactor, float recoverDelay, float slowdownLength, float recoverFactor = 1f, string motionName = "", bool forced = false, bool overwrite = true)
    {
        if (SlowSetup(slowdownFactor, motionName, forced, overwrite))
        {
            RecoverSlowMotion(recoverDelay, slowdownLength, recoverFactor);
        }
    }

    /// <param name="forced">If 【True】
    /// , this slow motion can not be overwritted by others【DoSlowMotion】
    /// , it need to cancel manually by【ResetTimeScale】and【RecoverSlowMotion】method.</param>
    public void DoSlowMotion(float slowdownFactor, string motionName = "", bool forced = false, bool overwrite = true)
    {
        TimeScale.global.originTimeScale = TimeScale.global.currentTimeScale;
        SlowSetup(slowdownFactor, motionName, forced, overwrite);
    }

    private bool SlowSetup(float slowdownFactor, string motionName, bool forced, bool overwrite)
    {
        bool result = false;
        if (overwrite)
        {
            if (TimeScale.global.forced)
            {
                if (motionName == TimeScale.global.name)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
        }
        else
        {
            if (!TimeScale.global.running)
            {
                result = true;
            }
        }
        if (result)
        {
            TimeScale.global.currentTimeScale = slowdownFactor;
            TimeScale.global.forced = forced;
            TimeScale.global.name = motionName;
        }
        return result;
    }

    /// <summary>
    /// After delay, time scale will recover within slowdownLength(duration)
    /// </summary>
    /// <param name="recoverDelay">Delay duration with recovering</param>
    /// <param name="slowdownLength">Recover time scale duration</param>
    /// <param name="recoverFactor">How much will time scale recover to</param>
    public void RecoverSlowMotion(float recoverDelay, float slowdownLength, float recoverFactor = 1f, string motionName = "")
    {
        if (!RecoverCheck(motionName))
            return;
        StartCoroutine(TimeScale.global.CallMotion(EaseOut(recoverDelay, slowdownLength, recoverFactor)));
    }

    private bool RecoverCheck(string motionName)
    {
        // If motion forced, need to check motionName same, then can cancel(start recover) this motion.
        if (TimeScale.global.forced)
        {
            if (motionName == TimeScale.global.name)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    private IEnumerator EaseOut(float recoverDelay, float slowdownLength, float recoverFactor)
    {
        yield return new WaitForSecondsRealtime(recoverDelay);
        Time.fixedDeltaTime = TimeScale.global.currentTimeScale * 0.02f;
        float timeleft = slowdownLength;
        while (timeleft > 0)
        {
            TimeScale.global.currentTimeScale += (recoverFactor / slowdownLength) * Time.unscaledDeltaTime;
            TimeScale.global.currentTimeScale = Mathf.Clamp(TimeScale.global.currentTimeScale, 0f, recoverFactor);
            timeleft -= Time.deltaTime;
            yield return null;
        }
    }

    // 調整整體動畫速度
    public void ModifyAllAnimSpeed(Animator anim, float newSpeed)
    {
        anim.speed = newSpeed;
    }
}

public enum SlowMotionTargetType
{
    Player,
    Enemy,
    Global
}

public class TimeScale
{
    // Default Timescales
    public static TimeScaleData global;
}

public class TimeScaleData
{
    public float originTimeScale = 1;
    public float currentTimeScale = 1;
    public float slowdownDuration = 1;

    public string name;
    public bool stop;
    public bool running;
    public bool paused;
    public bool finished;
    public bool forced;
    private bool @lock;

    public IEnumerator CallMotion(IEnumerator action)
    {
        // To block another motion, because there is a motion running now and it forced to run till it end.
        if (@lock)
        {
            yield break;
        }
        if (forced)
        {
            this.@lock = true;
        }

        if (running)
        {
            StopMotion();
            yield return new WaitForEndOfFrame();
        }
        if (!running)
        {
            StartMotion();
        }

        IEnumerator e = action;
        while (running)
        {
            if (stop)
                break;

            if (paused)
                yield return null;
            else
            {
                if (e != null && e.MoveNext())
                {
                    yield return e.Current;
                }
                else
                {
                    running = false;
                    break;
                }
            }
        }
        finished = true;
        if (forced)
        {
            this.@lock = false;
            forced = false;
        }
    }

    public void StartMotion()
    {
        stop = false;
        finished = false;
        paused = false;
        running = true;
    }

    public void StopMotion()
    {
        stop = true;
        running = false;
    }

    public void PauseMotion(bool paused)
    {
        this.paused = paused;
    }
}