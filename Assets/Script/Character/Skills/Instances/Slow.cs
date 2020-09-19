using StatsModifierModel;

public class Slow : DisposableSkill
{
    protected override void AddAffectEvent()
    {
        hitAffect.AddListener(DebuffSlowDown);
    }

    public string debuff = "緩速";
    ///<summary>
    /// 緩速: 移動速度-50%，持續4秒
    /// </summary>
    public void DebuffSlowDown()
    {
        var speedstat = target.data.moveSpeed;
        void affect() { speedstat.AddModifier(new StatModifier(-0.5f, StatModType.PercentageTime, debuff)); }
        void remove() { speedstat.RemoveModifier(new StatModifier(0.5f, StatModType.PercentageTime, debuff)); }
        target.buffController.AddBuff(debuff, affect, remove, 4f);
    }
}