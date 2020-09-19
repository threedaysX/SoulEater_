using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimParameterKeyWord
{
    public const string idle = "Idle";
    public const string moveSpeed = "Speed";
    public const string jump = "IsJumping";
    public const string startJump = "StartJump";
    public const string fallJump = "FallJump";
    public const string endJump = "EndJump";
    public const string preAttack = "IsPreAttacking";
    public const string attack = "IsAttacking";
    public const string attackCount = "AttackCount";
    public const string evade = "IsEvading";
    public const string useSkill = "IsSkillUsing";
    public const string castSkill = "IsSkillCasting";
    public const string knockStun = "IsKnockStun";
}

public class AnimNameKeyWord
{
    public const string useSkill = "UseSkill";
    public const string castSkill = "CastSkill";
}

public class Operation
{
    public string name;        // 執行當前行動的來源
    public OperationStateType stateType;    
    public IEnumerator action;              // 當前用來執行此動作的方法
    public float delay;     // 當前動作結束後的時間間隔延遲 (延遲多久進行下一個行動)
    public float nextOperationTime;     // 下一個行動的時間點
    public bool finished;               // 代表動作完成了
    public bool paused;
    public bool running;
    public bool stop;
    public Action endEvent;   // If operation stoped, roll back some action.

    /// <summary>
    /// 代表動作剛完成
    /// </summary>
    public bool FinishedTrigger
    {
        set
        {
            _finishedTrigger = value;
        }
        get
        {
            if (_finishedTrigger)
            {
                _finishedTrigger = false;
                return true;
            }
            return false;
        }
    }
    private bool _finishedTrigger;
    private bool forceActToEnd;  // 當前動作一定會做完，不會被其他動作中斷(依舊會被外力影響中斷)

    public Operation(string name, IEnumerator action = null, float delay = 0)
    {
        this.name = name;
        this.action = action;
        this.delay = delay;
        stateType = OperationStateType.None;
    }

    public IEnumerator CallOperation()
    {
        if (!stop)
            Start();

        IEnumerator e = action;
        while (running || forceActToEnd)
        {
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
                    forceActToEnd = false;
                    break;
                }
            }
        }
        finished = true;
        FinishedTrigger = true;
        if (endEvent != null || endEvent != default)
        {
            endEvent.Invoke();
        }
    }

    private void Start()
    {
        running = true;
        finished = false;
        stop = false;
    }

    public void Stop(float delayOffset = -999)
    {
        stop = true;
        running = false;
        if (delayOffset != -999)
        {
            delay = delayOffset;
        }
    }

    public void Pause()
    {
        paused = true;
    }
    public void Unpause()
    {
        paused = false;
    }

    public void ForceActToEnd()
    {
        forceActToEnd = true;
    }

    public void SetAction(IEnumerator action)
    {
        this.action = action;
        
    }
    public void SetDelay(float delay)
    {
        this.delay = delay;
        this.nextOperationTime = Time.time + this.delay;
    }
    public void SetEndEvent(Action endEvent)
    {
        this.endEvent = endEvent;
    }

    /// <summary>
    /// 設置動作狀態 (以對應下一個動作)
    /// </summary>
    public void SetOperationState(OperationStateType stateType)
    {
        this.stateType = stateType;
    }

    public bool CheckOperationSame(string newOperationName)
    {
        if (newOperationName == name)
        {
            return true;
        }
        return false;
    }

    public bool CheckOperationSame(Operation newOperation)
    {
        if (newOperation == null)
            return false;
        if (newOperation.name == name)
        {
            return true;
        }
        return false;
    }

    public bool CheckOperationState(string newOperationName)
    {
        bool canStoreNextOperation = false;
        bool isSameOperation = CheckOperationSame(newOperationName);

        switch (stateType)
        {
            case OperationStateType.None:
                break;
            case OperationStateType.Continuous:
                if (isSameOperation)
                {
                    canStoreNextOperation = true;
                }
                break;
            case OperationStateType.Interrupt:
                if (!isSameOperation)
                {
                    canStoreNextOperation = true;
                    Stop(0);
                }
                break;
            case OperationStateType.Trigger:
                canStoreNextOperation = true;
                if (!isSameOperation)
                {
                    Stop(0);
                }
                break;
            case OperationStateType.Link:
                canStoreNextOperation = true;
                if (isSameOperation)
                {
                    Stop();
                }
                else
                {
                    Stop(0);
                }
                break;
        }

        return canStoreNextOperation;
    }
}

public class Operations
{
    public Operation[] operations;
    public byte currentIndex;   // 當前執行的動作Index
    public byte lastIndex;      // 最後的動作Index
    private byte size;

    public Operations(byte size)
    {
        this.size = size;
        operations = new Operation[size];
        currentIndex = 0;
        lastIndex = 0;
    }

    public Operation GetCurrentOperation()
    {
        return operations[currentIndex];
    }

    public Operation GetLastOperation()
    {
        if (lastIndex < 1)
            return operations[size - 1];
        return operations[lastIndex - 1];
    }

    public void AddOperation(Operation operation)
    {
        if (lastIndex >= size || lastIndex == byte.MaxValue)
            lastIndex = byte.MinValue;

        operations[lastIndex++] = operation;
    }

    public void MoveToNextOperation()
    {
        if (currentIndex == size - 1)
        {
            currentIndex = byte.MinValue;
            return;
        }

        currentIndex++;
    }

    public void ReverseLastOperation()
    {
        lastIndex--;
    }
}

public enum OperationStateType
{
    /// <summary>
    /// 用來判斷此動作【可不可以被預存】(同樣動作or不同動作- 不預存)
    /// </summary>
    None,

    /// <summary>
    /// 用來判斷此動作【可不可以被接續】(同樣動作- 預存不打斷)
    /// </summary>
    Continuous,

    /// <summary>
    /// 用來判斷此動作【可不可以被打斷】(不同動作- 預存打斷)
    /// </summary>
    Interrupt,

    /// <summary>
    /// 用來判斷此動作【可不可以被觸發】(同樣動作- 預存不打斷 or 不同動作- 預存打斷)
    /// </summary>
    Trigger,

    /// <summary>
    /// 用來判斷此動作【可不可以被連段】(同樣動作or不同動作- 預存打斷)
    /// </summary>
    Link
}

/// <summary>
/// 每個動作會有獨立的延遲
/// </summary>
public class OperationDelayDictionary : Dictionary<string, Operation>
{
    public void AddOperation(string name, Operation operation)
    {
        if (this.ContainsKey(name))
        {
            this[name] = operation;
        }
        else
        {
            this.Add(name, operation);
        }
    }
}

public class OperationController : MonoBehaviour
{
    private Character character;
    private Animator anim;

    [Header("操作狀態判定")]
    public bool isIdle = false;
    public bool isSkillUsing = false;
    public bool isSkillCasting = false;
    public bool isJumping = false;
    public bool startJump = false;
    public bool fallJump = false;
    public bool endJump = false;
    public bool isAttacking = false;
    public bool isEvading = false;
    public bool isPreAttacking = false;
    public bool isKnockStun = false;

    [Header("碰撞判定")]
    public bool isGrounding = false;
    public bool groundTouch = false;
    public float collisionRadius = 0.3f;
    public Vector2 bottomOffset = new Vector2(0, -1);
    private LayerMask groundLayer;

    private Operations operations;   // 行動列，將按下的動作儲存
    private OperationDelayDictionary delayOperationDict;      // 記錄任何有延遲時間的動作 (延遲時間將以動作個別計算)

    private void Start()
    {
        character = GetComponent<Character>();
        anim = character.anim;
        operations = new Operations(10);
        delayOperationDict = new OperationDelayDictionary();       
        groundLayer = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        StartOperation();

        if (anim != null)
        {
            anim.SetBool(AnimParameterKeyWord.jump, isJumping);
            anim.SetBool(AnimParameterKeyWord.startJump, startJump);
            anim.SetBool(AnimParameterKeyWord.fallJump, fallJump);
            anim.SetBool(AnimParameterKeyWord.endJump, endJump);

            anim.SetBool(AnimParameterKeyWord.evade, isEvading);

            anim.SetBool(AnimParameterKeyWord.useSkill, isSkillUsing);
            anim.SetBool(AnimParameterKeyWord.castSkill, isSkillCasting);

            anim.SetBool(AnimParameterKeyWord.preAttack, isPreAttacking);
            anim.SetBool(AnimParameterKeyWord.attack, isAttacking);
            anim.SetInteger(AnimParameterKeyWord.attackCount, character.AttackAnimNumber);

            anim.SetBool(AnimParameterKeyWord.knockStun, isKnockStun);
        }
    }

    private void LateUpdate()
    {
        CheckStun();
        CheckIdle();
        CheckGrounded();
        CheckResetAttack();
    }

    #region Check
    private void CheckIdle()
    {
        isIdle = anim.GetCurrentAnimatorStateInfo(0).IsName(AnimParameterKeyWord.idle);
    }

    private void CheckStun()
    {
        if (character.isKnockStun && !isKnockStun)
        {
            isKnockStun = true;
            InterruptAnimOperation();
            character.LockOperation(LockType.Stun, true);
        }
        if (!character.isKnockStun && isKnockStun)
        {
            isKnockStun = false;
            character.LockOperation(LockType.Stun, false);
        }
    }

    private void CheckGrounded()
    {
        character.opc.isGrounding = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);

        if (isGrounding && !groundTouch)
        {
            // Play groundTouch particle
            groundTouch = true;
        }

        if (!isGrounding && groundTouch)
        {
            groundTouch = false;
        }
    }

    private void CheckResetAttack()
    {
        // 當不在攻擊中，且確認玩家在攻擊間隔內是否有再次攻擊，否則攻擊次數歸0
        if (!isAttacking && !isPreAttacking && Time.time >= character.nextAttackResetTime && character.attackComboCount > 0)
        {
            character.AttackAnimNumber = 0;
            character.attackComboCount = 0;
            if (anim != null)
            {
                anim.SetInteger(AnimParameterKeyWord.attackCount, character.attackComboCount);
            }
        }
    }
    #endregion

    #region Operation Control
    private void StartOperation()
    {
        // 本次動作尚未開始，並等待每個動作的獨立延遲，可開始動作。
        Operation operation = operations.GetCurrentOperation();
        if (operation == null)
            return;

        if (!operation.running && !operation.stop && !operation.finished)
        {
            StartCoroutine(operation.CallOperation());
            delayOperationDict.AddOperation(operation.name, operation);
        }

        if (operation.FinishedTrigger)
        {
            operations.MoveToNextOperation();
            return;
        }
    }

    /// <summary>
    /// 新增代辦動作
    /// </summary>
    public void AddOperation(Operation operation, bool forceActToEnd = false)
    {
        if (forceActToEnd)
        {
            operation.ForceActToEnd();
        }
        operations.AddOperation(operation);
    }

    public bool CheckOperation(string operationName)
    {
        if (!CheckOperationDelay(operationName))
            return false;

        Operation currentOperation = operations.GetCurrentOperation();
        if (currentOperation == null || currentOperation.finished)
        {
            return true;
        }

        Operation lastOperation = operations.GetLastOperation();
        // 若為正在執行的動作，則檢查狀態，決定是否要打斷或是等待
        // 若否，則先檢查是否為相同動作，不是則會被其他動作覆蓋
        if (lastOperation != null && lastOperation.running)
        {
            return lastOperation.CheckOperationState(operationName);
        }
        else
        {
            if (lastOperation != null && !lastOperation.CheckOperationSame(operationName))
            {
                operations.ReverseLastOperation();
                return true;
            }
        }

        return false;
    }

    private bool CheckOperationDelay(string operationName)
    {
        // 如果沒有動作紀錄，代表不需等待延遲。
        if (!delayOperationDict.ContainsKey(operationName))
            return true;

        Operation previousOperation = delayOperationDict[operationName];
        if (Time.time >= previousOperation.nextOperationTime)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 動作動畫中止
    /// </summary>
    public void InterruptAnimOperation()
    {
        isSkillUsing = false;
        isSkillCasting = false;

        isJumping = false;
        startJump = false;
        fallJump = false;
        endJump = false;

        isEvading = false;

        isPreAttacking = false;
        isAttacking = false;
    }
    #endregion

    #region Others  
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // 地板偵測
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);
    }
    #endregion
}