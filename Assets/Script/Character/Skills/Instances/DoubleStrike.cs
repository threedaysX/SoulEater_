using UnityEngine;

public class DoubleStrike : DisposableSkill
{
    protected override void AddAffectEvent()
    {
        hitAffect.AddListener(delegate
        {
            DamageTarget();
            DamageTarget();
        });
    }
}
