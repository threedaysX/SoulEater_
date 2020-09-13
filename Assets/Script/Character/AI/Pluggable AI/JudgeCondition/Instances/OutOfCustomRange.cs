using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/OutOfCustomRange")]
public class OutOfCustomRange : JudgeCondition
{
    public float customRange;
    public override bool CheckActConditionHaviour()
    {
        return CheckCustomFarDistance();
    }

    private bool CheckCustomFarDistance()
    {
        if (Ai.ChaseTarget != null)
        {
            // 若在自定距離外(含)
            if ((Ai.ChaseTarget.transform.position - Ai.transform.position).sqrMagnitude >= customRange * customRange)
            {
                return true;
            }
        }
        return false;
    }
}
