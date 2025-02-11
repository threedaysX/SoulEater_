﻿using UnityEngine;

// 複製用: 
// [CreateAssetMenu(menuName = "Character/AI/Action/繼承的新ClassName")]
public abstract class AiAction : AiHaviourBase
{
    [Header("基本設定")]
    public bool on = true;
    public bool switchOnActionOnStart = true;
    public bool ignoreJudgement = false;    // 是否無視動作的條件判斷
    public bool influenceWeight = true;    // 是否影響動作的權重

    /// <summary>
    /// 此動作的權重，基本以每2點為一個階段。
    /// [0~1] 不太想
    /// [2~3] 較少
    /// [4~5] 普通
    /// [6~7] 常採取的動作
    /// [8~9] 擅長而且喜歡選擇的動作
    /// [10] 超想這麼做
    /// </summary>
    [HideInInspector] public float currentActionWeight;

    /// <summary>
    /// 這個行動已經被降低過了N點權重
    /// </summary>
    public float DiffCount
    {
        get
        {
            return _diffCount;
        }
        set
        {
            _diffCount = value;
            if (_diffCount < 0)
                _diffCount = 0;
        }
    }

    [Header("基本權重")]
    public float originalActionWeight;

    [Header("行為後延遲")]
    public float offsetActionDelay; // 校正行為延遲(可調整)
    public float AdditionActionDelay { get; private set; }   // 行為本身延遲(至行為結束的時長)

    [Header("權重減值")]
    public float minusWeightAmountWhenNotSuccess = 2;
    public float minusWeightAmountAfterAction = 1;
    private float _diffCount;

    [Header("行為")]
    public AiActionType actionType;
    public JudgeSet[] judgeSets;

    [Header("接續行為")]
    public AiAction[] linkedActions;

    public bool CheckActionThatCanDo()
    {
        if (judgeSets == null)
            return false;

        // 若權重已經小於0，則重置權重。
        if (currentActionWeight < 0)
        {
            ResetWeight();
        }
        // 沒有任何條件，直接觸發
        if (judgeSets.Length == 0)
        {
            return true;
        }
        foreach (var set in judgeSets)
        {
            // 開始檢查該動作的各個觸發條件
            if (set.judgement.StartCheckActCondition(Ai))
            {
                // 將判斷後的權重設在動作權重上
                if (set.actionWeightAfterJudge != 0)
                    currentActionWeight = set.actionWeightAfterJudge - DiffCount;
                return true;
            }
        }
        return false;
    }

    public void AddDiffCount(float diff)
    {
        DiffCount += diff;
    }

    public void ResetActionOn()
    {
        if (switchOnActionOnStart)
        {
            on = true;
        }
    }
    /// <summary>
    /// 重置權重與權重減值
    /// </summary>
    /// <param name="reduceDiffCount">可決定權重減值要恢復多少，而不是歸0</param>
    public void ResetWeight(float reduceDiffCount = -999)
    {
        currentActionWeight = originalActionWeight;
        if (reduceDiffCount == -999)
            DiffCount = 0;
        else
            DiffCount -= reduceDiffCount;
    }
    public void ApplyActionDelay(float value)
    {
        AdditionActionDelay = value;
    }
    public void SetAiActionSwitchOn(bool on)
    {
        this.on = on;
    }

    /// <summary>
    /// 做什麼動作
    /// </summary>
    public abstract bool StartActHaviour();
}

/// <summary>
/// AI行為判斷基礎。
/// (每個)判斷含有單一 or 複合式條件。
/// </summary>
[System.Serializable]
public struct JudgeSet
{
    public int actionWeightAfterJudge;
    public Judgement judgement;
}

[System.Serializable]
public struct Judgement
{
    private int conditionTrueCount;

    public JudgeCondition[] conditions;

    public bool StartCheckActCondition(AI Ai)
    {
        conditionTrueCount = 0;
        foreach (JudgeCondition condition in conditions)
        {
            if (condition == null)
                continue;
            condition.GetCurrentAIHavior(Ai);
            if (condition.CheckActConditionHaviour())
            {
                conditionTrueCount++;
            }
            else
            {
                return false;
            }
        }
        return CheckTrueConditionCount();
    }

    private bool CheckTrueConditionCount()
    {
        if (conditionTrueCount == conditions.Length)
        {
            return true;
        }
        return false;
    }
}
