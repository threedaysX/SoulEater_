using UnityEngine;

[CreateAssetMenu(menuName = "Endowment/Affix/KnockBack")]
public class AttackKnockBackAffix : AttackHitTriggerAffix
{
    public float knockBackForce;

    protected override void SetAffect()
    {
        Character target = GetAffectTarget();
        if (target == null)
            return;

        target.knockBackSystem.KnockStun(target,  owner.combatController.damageDirectionX, knockBackForce);
    }

    protected override void RemoveAffixAffect()
    {
        
    }
}
