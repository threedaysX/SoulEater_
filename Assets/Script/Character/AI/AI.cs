﻿using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DistanceDetect))]
public class AI : MonoBehaviour, IAiBase, IAiActionBase
{
    [Header("開始運作AI")]
    public bool canDetect = true;
    public bool canAction = true;
    public bool switchOn = true;

    #region On Update Settings
    [Header("偵測動作")]
    [SerializeField] protected Detect[] detects;
    [Header("行為模式")]
    [SerializeField] protected AiAction[] actions;
    [Header("預設行為模式")]
    public AiAction defaultAction;
    [Header("上一個行為模式")]
    public AiAction lastAction;
    private bool lastActionSuccess;
    private bool toDoLinkedActions;

    private bool inCombatStateTrigger = false; // 是否進入戰鬥狀態
    private bool outOfCombatTrigger = false;

    [Header("偵測距離")]
    public DistanceDetect detectControl;
    public float detectDistance;
    [Header("下次可行動時間")]
    private float nextActTimes = 0f;
    #endregion

    #region Weight Settings
    [Header("最大權重容許區間")]
    public float maxWeightOffset = 2;    // 將符合【最大權重】與【最大權重-Offset】挑出並執行。
    [Header("動作權重恢復")]
    public float actionWeightRegen = 1;       // 每次恢復量
    public int actionWeightRegenCount = 10;  // 每做N個動作，就恢復權重一次
    private int cumulativeActionCount = 0;
    #endregion

    #region Target Settings
    public Transform ChaseTarget { get; protected set; }
    public Transform LastChaseTarget { get; protected set; }
    public LayerMask PlayerLayer { get; protected set; }
    #endregion

    #region Interface
    public IFacement _facement;
    #endregion

    #region EventHanlder
    public delegate void OnDisableHandler();
    public delegate void OnEnableHandler();
    public OnDisableHandler OnDisableCall { get; set; }
    public OnEnableHandler OnEnableCall { get; set; }
    #endregion

    /// <summary>
    /// Call this when start.
    /// </summary>
    public virtual void OnStart()
    {
        PlayerLayer = LayerMask.GetMask("Player");
        ResetAiSwitchOn();

        foreach (AiAction action in actions)
        {
            action.ResetActionOn();
        }
    }

    /// <summary>
    /// Call this update.
    /// </summary>
    public virtual void OnUpdate()
    {
        if (switchOn)
        {
            DoDetects();
            Combat();
            CheckOutOfCombatState();
        }
    }

    private void CheckOutOfCombatState()
    {
        if (inCombatStateTrigger && ChaseTarget == null)
        {
            inCombatStateTrigger = false;
            outOfCombatTrigger = true;
        }

        // 脫離戰鬥後，重置所有行動權重
        if (!inCombatStateTrigger && outOfCombatTrigger)
        {
            outOfCombatTrigger = false;
            foreach (var action in actions)
            {
                action.ResetWeight();
            }
        }

        // 每執行N次動作後，恢復N點的行動權重
        if (cumulativeActionCount != 0 && cumulativeActionCount % actionWeightRegenCount == 0)
        {
            foreach (var action in actions)
            {
                action.ResetWeight(actionWeightRegen);
            }
        }
    }

    private void DoDetects()
    {
        if (!canDetect)
            return;

        // 偵測
        foreach (Detect detect in detects)
        {
            detect.GetCurrentAIHavior(this);
            if (detect.StartDetectHaviour())
            {
                if (!inCombatStateTrigger)
                    inCombatStateTrigger = true;
            }
            else
            {
                if (ChaseTarget != null)
                    ChaseTarget = null;
            }
        }
    }

    private void Combat()
    {
        // 進入戰鬥狀態
        if (inCombatStateTrigger && ChaseTarget != null)
        {
            // 在戰鬥狀態時，會持續面對目標
            _facement.FaceTarget(this, ChaseTarget);

            if (canAction && Time.time >= nextActTimes)
            {
                // 每次開始執行動作之前，回到Idle狀態
                ReturnDefaultAction();
                // Do action or linked action.
                if (toDoLinkedActions)
                {
                    DoActions(lastAction.linkedActions);
                    toDoLinkedActions = false;
                }
                else
                {
                    DoActions(actions);
                }
            }
        }
        else
        {
            ReturnDefaultAction();
        }
    }

    private void DoActions(AiAction[] actions)
    {
        List<AiAction> actionToDoList = CheckActions(actions);

        // 決定最後的動作
        DoHightestWeightAction(actionToDoList);
    }

    private List<AiAction> CheckActions(AiAction[] actions)
    {
        List<AiAction> actionToDoList = new List<AiAction>();
        foreach (AiAction action in actions)
        {
            if (!action.on)
                continue;
            action.GetCurrentAIHavior(this);
            // 判斷哪些動作符合條件，代表可以做
            if (action.ignoreJudgement || action.CheckActionThatCanDo())
            {
                // 只留下【最大】與【最大-offset】之間權重的動作(相同權重也會留下)。
                if (actionToDoList.Count != 0)
                {
                    if (action.currentActionWeight < actionToDoList[0].currentActionWeight - maxWeightOffset)
                        continue;
                    if (action.currentActionWeight > actionToDoList[0].currentActionWeight)
                    {
                        actionToDoList.Clear();
                    }
                }
                actionToDoList.Add(action);
            }
        }

        return actionToDoList;
    }

    private void DoHightestWeightAction(List<AiAction> actions)
    {
        // 如果沒動作可以做，就Idle
        if (actions.Count == 0)
        {
            // Reset linked action trigger.
            toDoLinkedActions = false;
            // Reset action.
            ReturnDefaultAction(true);
            return;
        }

        AiAction action;
        if (actions.Count == 1)
        {
            action = actions[0];
            DoAction(action, action.influenceWeight);
            return;
        }

        // 執行任一動作
        action = actions[UnityEngine.Random.Range(0, actions.Count - 1)];
        DoAction(action, action.influenceWeight);
    }

    /// <summary>
    /// 動作執行
    /// </summary>
    /// <param name="action">要執行的動作</param>
    public void DoAction(AiAction action, bool influenceWeight)
    {
        // Start Action
        lastActionSuccess = action.StartActHaviour();

        if (lastActionSuccess)
        {
            lastAction = action;

            // To do linked actions bool trigger.
            if (lastAction.linkedActions.Length > 0)
            {
                toDoLinkedActions = true;
            }

            // Reset action delay.
            ResetNextActTimes(action);
        }
        
        if (influenceWeight)
        {
            float amount;
            if (lastActionSuccess)
            {
                amount = action.minusWeightAmountAfterAction;
            }
            else
            {
                amount = action.minusWeightAmountWhenNotSuccess;
            }
            if (amount > 0)
            {
                // 動作結束後，權重將下降N點，降低這個對於動作的慾望
                action.currentActionWeight -= amount;
                action.AddDiffCount(amount);
            }
        }
    }

    private void ResetNextActTimes(AiAction currentAction)
    {
        // 只有移動不需要等待延遲(且移動不算是實際的行動數)
        if (lastAction.actionType == AiActionType.Move)
            return;
        float delay = currentAction.offsetActionDelay + currentAction.AdditionActionDelay;
        if (delay <= 0)
        {
            delay = 0.02f;  // 行動下限延遲
        }
        nextActTimes = Time.time + delay;
        cumulativeActionCount++;    // 行動次數+1
    }

    public void ReturnDefaultAction(bool setToLastAction = false)
    {       
        if (defaultAction == null)
            return;
        if (lastAction != null && lastAction.actionType == AiActionType.Idle)
            return;

        defaultAction.GetCurrentAIHavior(this);
        defaultAction.StartActHaviour();

        if (setToLastAction)
            lastAction = defaultAction;
    }

    public void SetChaseTarget(Transform target)
    {
        if (LastChaseTarget == null)
        {
            LastChaseTarget = target;
        }
        else
        {
            LastChaseTarget = ChaseTarget;
        }
        ChaseTarget = target;
    }

    public virtual void ResetAiSwitchOn()
    {
        this.StopAllCoroutines();
        this.ReturnDefaultAction();
        switchOn = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }

    public void OnDisable()
    {
        OnDisableCall?.Invoke();
    }

    public void OnEnable()
    {
        OnEnableCall?.Invoke();
    }
}

public interface IAiActionBase
{
    void DoAction(AiAction action, bool influenceWeight);
}

public interface IAiBase
{
    void OnStart();
    void OnUpdate();
}

public interface IFacement
{
    void FaceTarget(MonoBehaviour self, Transform target, bool force = false);
}

