using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/CheckCustomDamageInTime")]
public class CheckCustomDamageInTime : JudgeCondition
{
    [Header("受最大生命幾%傷害"), Range(0, 100)]
    public float custumDamage;
    [Header("幾秒內")]
    public float damageInTime;

    private Character character;

    public override bool CheckActConditionHaviour()
    {
        return HasDowned();
    }

    private bool HasDowned()
    {
        if (character == null)
            character = AI<Character>();

        character.combatController.hasHitInTime = damageInTime;
        if ((character.combatController.takeHowMuchDamage / character.data.maxHealth.Value) * 100 >= custumDamage)
        {
            return true;
        }

        return false;
    }
}
