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
        var mod = new StatModifier(-0.5f, StatModType.PercentageTime, debuff);
        void affect() { speedstat.AddModifier(mod); }
        void remove() { speedstat.RemoveModifier(mod); }
        target.buffController.AddBuff(debuff, affect, remove, 4f);
    }
}