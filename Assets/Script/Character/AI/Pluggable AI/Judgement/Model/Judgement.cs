﻿using UnityEngine;

/// <summary>
/// AI行為判斷基礎。
/// (每個)判斷含有單一、複合式條件。
/// </summary>
[CreateAssetMenu(menuName = "Character/AI/Judgement")]
public class Judgement : AiHaviourBase
{
    public int actionWeightAfterJudge;
    public int conditionTrueCount;
    public JudgeCondition[] conditions;

    public void StartCheckActCondition()
    {
        conditionTrueCount = 0;
        foreach (JudgeCondition condition in conditions)
        {
            if (condition == null)
                continue;
            condition.GetCurrentAIHavior(ai);
            if (condition.CheckActConditionHaviour())
            {
                conditionTrueCount++;
            }
        }
    }

    public bool CheckTrueConditionCount()
    {
        if (conditionTrueCount == conditions.Length)
        {
            return true;
        }
        return false;
    }
}