using UnityEngine;
using UniRx;

public abstract class AttackHitTriggerAffix : ConditionTriggerAffix
{
    [Range(0, 100)]
    public float chance = 100;
    private bool attackHitTrigger = false;

    protected override void InitAffix()
    {
        if (!eventSubscribedTrigger)
        {
            eventSubscribedTrigger = true;
            owner.combatController.AttackHitAsObservable
                .ObserveOnMainThread()
                .Subscribe(target => {
                    attackHitTrigger = true;
                });
        }
        base.InitAffix();
    }

    protected override bool GetAffectCondition()
    {
        return TriggerWithChance();
    }

    private bool TriggerWithChance()
    {
        // 命中後擲骰
        if (attackHitTrigger)
        {
            attackHitTrigger = false;
            if (Random.Range(0, 100) <= chance)
            {
                return true;
            }    
        }
        return false;
    }
}
