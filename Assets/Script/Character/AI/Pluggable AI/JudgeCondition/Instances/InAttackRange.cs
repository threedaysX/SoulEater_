using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/InAttackRange")]
public class InAttackRange : JudgeCondition
{
    private Character character;

    public override bool CheckActConditionHaviour()
    {
        return CheckCloseDistance();
    }

    private bool CheckCloseDistance()
    {
        if (character == null)
            character = AI<Character>();

        if (Ai.ChaseTarget != null)
        {
            // 若在攻擊距離內(含)
            if ((Ai.ChaseTarget.transform.position - Ai.transform.position).sqrMagnitude <=  character.data.attackRange.Value * character.data.attackRange.Value)
            {
                return true;
            }
        }
        return false;
    }
}
