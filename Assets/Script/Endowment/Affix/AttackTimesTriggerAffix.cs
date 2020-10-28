using UnityEngine;

public abstract class AttackTimesTriggerAffix : ConditionTriggerAffix
{
    [Range(0, 100)]
    public float chance = 100;
    public int needAttackTimesToTrigger = 1;

    protected override bool GetAffectCondition()
    {
        return TriggerWithChance();
    }

    private bool TriggerWithChance()
    {
        // 每N次攻擊觸發
        if (TriggerAttackTimes())
        {
            // 觸發後擲骰
            if (Random.Range(0, 100) <= chance)
            {
                return true;
            }
        }
        return false;
    }

    private const string attackTimes = "AttackTimes";
    private CumulativeDataType cdHitType = CumulativeDataType.HitTimes;
    private bool TriggerAttackTimes()
    {
        var cdc = owner.cumulativeDataController;
        if (cdc.GetData(attackTimes, cdHitType) >= needAttackTimesToTrigger)
        {
            cdc.ModifyData(attackTimes, cdHitType, 0);
            return true;
        }
        return false;
    }
}


