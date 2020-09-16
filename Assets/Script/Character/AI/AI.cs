using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DistanceDetect))]
public class AI : MonoBehaviour, IAiBase, IAiActionBase
{
    [Header("開始運作AI")]
    [SerializeField] private bool _switchOn = true;
    public bool canDetect = true;
    public bool canAction = true;
    public bool SwitchOn
    {
        get
        {
            return _switchOn;
        }
        protected set
        {
            _switchOn = value;
            if (_switchOn)
                switchOnTrigger = true;
        }
    }
    protected bool switchOnTrigger;

    [Header("偵測動作")]
    [SerializeField] protected Detect[] detects;
    [Header("行為模式")]
    [SerializeField] protected AiAction[] actions;
    [Header("預設行為模式")]
    public AiAction defaultAction;
    [Header("上一個行為模式")]
    public AiAction lastAction;
    public LinkedAction linkedAction;
    private bool lastActionSuccess;
    private List<AiAction> actionToDoList = new List<AiAction>();

    [Header("偵測距離")]
    public DistanceDetect detectControl;
    public float detectDistance;
    [Header("下次可行動時間")]
    private float nextActTimes = 0f;

    [Header("最大權重容許區間")]
    public float maxWeightOffset = 2;    // 將符合【最大權重】與【最大權重-Offset】挑出並執行。
    [Header("動作權重恢復")]
    public float actionWeightRegen = 1;       // 每次恢復量
    public int actionWeightRegenCount = 10;  // 每做N個動作，就恢復權重一次
    private int cumulativeActionCount = 0;

    private bool inCombatStateTrigger = false; // 是否進入戰鬥狀態
    private bool outOfCombatTrigger = false;

    public IFacement _facement;

    [HideInInspector] public Transform ChaseTarget { get; protected set; }
    [HideInInspector] public Transform LastChaseTarget { get; protected set; }
    [HideInInspector] public LayerMask PlayerLayer { get; protected set; }

    /// <summary>
    /// Call this when start.
    /// </summary>
    public virtual void OnStart()
    {
        PlayerLayer = LayerMask.GetMask("Player");
        
        ReturnDefaultAction(true);

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
        if (switchOnTrigger)
        {
            ResetAiSwitchOn();
        }
        if (SwitchOn)
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
            if (canAction && Time.time >= nextActTimes)
            {
                // 每次開始執行動作之前，回到Idle狀態
                ReturnDefaultAction();
                // 在執行Action時，會持續面對目標
                _facement.FaceTarget(this, ChaseTarget);
                // Do action or linked action.
                if (linkedAction.action == null)
                {
                    DoActions();
                }
                else
                {
                    DoLinkedAction();
                }
            }
        }
        else
        {
            ReturnDefaultAction();
        }
    }

    private void DoLinkedAction()
    {
        linkedAction.action.GetCurrentAIHavior(this);
        // Check linked action condition.(If not ignore)
        if (linkedAction.ignoreJudgement || linkedAction.action.CheckActionThatCanDo())
        {
            DoAction(linkedAction.action, linkedAction.influenceWeight);
        }
        // Reset linked action.
        linkedAction.action = null;
    }

    private void DoActions()
    {
        actionToDoList.Clear();
        // 判斷哪些動作符合條件，代表可以做
        // 若上一個相同的動作執行失敗，則權重降低N一次
        foreach (AiAction action in actions)
        {
            if (!action.on)
                continue;
            action.GetCurrentAIHavior(this);
            if (action.CheckActionThatCanDo())
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

        // 如果沒動作可以做，就Idle
        if (actionToDoList.Count == 0)
        {
            ReturnDefaultAction();
            return;
        }

        // 決定最後的動作
        DoHightestWeightAction(actionToDoList);
    }

    private void DoHightestWeightAction(List<AiAction> actions)
    {
        AiAction action;
        if (actions.Count == 1)
        {
            action = actions[0];
            DoAction(action, true);
            return;
        }

        // 執行任一動作
        action = actions[Random.Range(0, actions.Count - 1)];
        DoAction(action, true);
    }

    int count = 0;
    /// <summary>
    /// 動作執行
    /// </summary>
    /// <param name="action">要執行的動作</param>
    public void DoAction(AiAction action, bool influenceWeight)
    {
        if (action.beforeActionEvent != null && action.beforeActionEvent.GetPersistentEventCount() > 0)
        {
            action.beforeActionEvent.Invoke();
        }
        if (action.beforeActionAddEvent != null && action.beforeActionAddEvent.GetPersistentEventCount() > 0)
        {
            action.beforeActionAddEvent.Invoke();
            action.beforeActionAddEvent.RemoveAllListeners();
        }

        // Start Action
        lastActionSuccess = action.StartActHaviour();

        if (lastActionSuccess)
        {
            lastAction = action;
            linkedAction = action.linkedAction;
            // Reset action delay.
            ResetNextActTimes(action);
        }
        Debug.Log("AAAA    " + (count++) + " CC             " + nextActTimes + "   BB    " + action);

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
        float delay = currentAction.offsetActionDelay + currentAction.additionActionDelay;
        if (currentAction.linkedAction.action != null)
        {
            delay += currentAction.linkedAction.delay;
        }
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
        ReturnDefaultAction();
        switchOnTrigger = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, detectDistance);
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

