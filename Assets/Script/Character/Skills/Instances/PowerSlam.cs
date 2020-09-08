using UnityEngine;

public class PowerSlam : DisposableSkill
{
    protected override void AddAffectEvent()
    {
        hitAffect.AddListener(delegate
        {
            DebuffArmorBreak();
            DamageTarget();
        });
    }

    public string armorBreak = "破甲LV1";
    /// <summary>
    /// 破甲Lv1: 對命中的敵人造成-10%基礎防禦，持續4秒。			
    /// </summary>
    private void DebuffArmorBreak()
    {
        Debuff.Instance.ArmorBreakWithLevel(target, 1, 4f);
    }
}

     
