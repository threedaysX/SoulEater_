using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/InCustomRange")]
public class InCustomRange : JudgeCondition
{
    public float customRange;
    public override bool CheckActConditionHaviour()
    {
        return CheckCustomCloseDistance();
    }

    private bool CheckCustomCloseDistance()
    {
        if (Ai.ChaseTarget != null)
        {
            // 若在自定距離內(含)
            if ((Ai.ChaseTarget.transform.position - Ai.transform.position).sqrMagnitude <= customRange * customRange)
            {
                return true;
            }
        }
        return false;
    }
}
