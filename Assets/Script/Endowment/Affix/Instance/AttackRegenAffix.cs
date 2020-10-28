using UnityEngine;

[CreateAssetMenu(menuName = "Endowment/Affix/AttackRegen")]
public class AttackRegenAffix : AttackHitTriggerAffix
{
    public float regenValue;

    protected override void SetAffect()
    {
        owner.CurrentHealth += regenValue;
    }

    protected override void RemoveAffixAffect()
    {

    }
}
