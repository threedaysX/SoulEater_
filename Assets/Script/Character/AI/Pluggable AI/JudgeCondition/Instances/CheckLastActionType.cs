using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/CheckLastActionType")]
public class CheckLastActionType : JudgeCondition
{
    [Header("非此Type")]
    public bool isReverse;
    public AiActionType isLastActionType;

    public override bool CheckActConditionHaviour()
    {
        return LastActionType();
    }

    private bool LastActionType()
    {
        bool result = false;
        if(Ai.lastAction != null)
        {
            if (Ai.lastAction.actionType == isLastActionType)
            {
                result = true;
            }
        }

        if (isReverse)
        {
            result = !result;
        }

        return result;
    }
}
