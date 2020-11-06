using StatsModifierModel;
using StatsModel;

public class PowerUp : DisposableSkill
{
    public float duration = 12f;

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(BuffStr);
        immediatelyAffect.AddListener(BuffAgi);
        immediatelyAffect.AddListener(BuffDex);
        immediatelyAffect.AddListener(BuffDEF);
    }

    public string strBuffName = "STR上升";
    /// <summary>
    /// 微幅增加自身力量，持續6秒
    /// </summary>
    private void BuffStr()
    {
        var str = sourceCaster.data.status.strength;
        int value = 0;
        if (str.Value < 20)
            value += 5;
        else
            value += 2;
        BuffStat(str, value, strBuffName, duration);
    }

    public string agiBuffName = "AGI上升";
    /// <summary>
    /// 微幅增加自身敏捷屬性，持續6秒
    /// </summary>
    private void BuffAgi()
    {
        var agi = sourceCaster.data.status.agility;
        int value = 0;
        if (agi.Value < 20)
            value += 5;
        else
            value += 2;
        BuffStat(agi, value, agiBuffName, duration);
    }

    public string defBuffName = "DEF上升";
    /// <summary>
    /// 增加自身防禦，持續6秒
    /// </summary>
    private void BuffDEF()
    {
        var def = sourceCaster.data.defense;
        float value = 50f;
        BuffStat(def, value, defBuffName, duration);
    }

    public string dexBuffName = "DEF上升";
    /// <summary>
    /// "微幅增加自身靈巧屬性，持續6秒
    /// </summary>
    private void BuffDex()
    {
        var dex = sourceCaster.data.status.dexterity;
        int value = 0;
        if (dex.Value < 20)
            value += 5;
        else
            value += 2;
        BuffStat(dex, value, dexBuffName, duration);
    }
    private void BuffStat(Stats stats, float value, string buffName, float duration)
    {
        var mod = new StatModifier(value, StatModType.FlatAdd, buffName);
        void affect()
        {
            stats.AddModifier(mod);
        }
        void remove()
        {
            stats.RemoveModifier(mod);
        }
        sourceCaster.buffController.AddBuff(buffName, affect, remove, duration);
    }
}

