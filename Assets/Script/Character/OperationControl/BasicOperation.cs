using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BasicOperation
{
    public bool CanDo { get; private set; }
    public BasicOperationType operationType;
    public Dictionary<LockType, LockData> locks;
    private Coroutine last = null;

    public void Lock(LockType lockType, MonoBehaviour where = null, bool ignoreTimeScale = false, float duration = -1)
    {
        if (locks == null)
            locks = new Dictionary<LockType, LockData>();

        if (locks.ContainsKey(lockType))
        {
            locks[lockType].duration = duration;
        }
        else
        {
            locks.Add(lockType, new LockData(1, duration));
        }

        CanDo = false;

        if (duration >= 0)
        {
            CountDown(where, lockType, duration, ignoreTimeScale);
        }
    }

    public void UnLock(LockType lockType = LockType.All)
    {
        if (locks == null)
            locks = new Dictionary<LockType, LockData>();

        if (lockType == LockType.All)
        {
            CanDo = true;
            locks.Clear();
            return;
        }

        if (locks.ContainsKey(lockType))
        {
            locks[lockType].lockNum--;
            if (locks[lockType].lockNum <= 0)
            {
                locks.Remove(lockType);
            }
        }
        if (locks.Count == 0)
            CanDo = true;
    }

    private void CountDown(MonoBehaviour where, LockType lockType, float duration, bool ignoreTimeScale)
    {
        if (last != null)
        {
            Counter.Instance.StopCountDown(where, last);
        }
        last = Counter.Instance.StartCountDown(where, duration, ignoreTimeScale, null, delegate { UnLock(lockType); });
    }
}

public enum BasicOperationType
{ 
    Move,
    Jump,
    Evade,
    Attack,
    UseSkill,
    LockDirection
}

public enum LockType 
{
    All,
    OperationAction,
    SkillAction,
    Stun,
    Freeze,
    Silence,
    Afraid,
    Lame, // 無法移動
    TypeChange,  // 型態改變(變身or變化...)
    Die
}

public class LockData
{
    public int lockNum;
    public float duration;

    public LockData(int lockNum, float nextUnlockTime)
    {
        this.lockNum = lockNum;
        this.duration = nextUnlockTime;
    }
}