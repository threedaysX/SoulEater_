using UnityEngine;

[CreateAssetMenu(menuName = "Endowment/Affix/AttackIgniteDebuff")]
public class AttackIgniteAffix : AttackHitTriggerAffix
{
    public float duration;

    protected override void SetAffect()
    {
        Character target = GetAffectTarget();
        if (target == null)
            return;

        Debuff.Instance.Ignite(owner, target, duration);
    }

    protected override void RemoveAffixAffect()
    {
        
    }
}
