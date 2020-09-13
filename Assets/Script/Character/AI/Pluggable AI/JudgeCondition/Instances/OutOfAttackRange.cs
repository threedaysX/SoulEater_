using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/OutOfAttackRange")]
public class OutOfAttackRange : JudgeCondition
{
    private Character character;

    public override bool CheckActConditionHaviour()
    {
        return CheckFarDistance();
    }

    private bool CheckFarDistance()
    {
        if (character == null)
            character = AI<Character>();
        if (Ai.ChaseTarget != null)
        {
            // 若在攻擊距離外
            if ((Ai.ChaseTarget.position - Ai.transform.position).sqrMagnitude >= character.data.attackRange.Value * character.data.attackRange.Value)
            {
                return true;
            }
        }
        return false;
    }
}
