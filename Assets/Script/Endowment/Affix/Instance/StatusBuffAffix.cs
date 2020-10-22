using StatsModel;
using StatsModifierModel;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Endowment/Affix/StatusBuff")]
public class StatusBuffAffix : NormalAffix
{
    public List<StatusAdds> stats;

    protected override void SetAffect()
    {
        foreach (StatusAdds s in stats)
        {
            Stats stats = GetStatus(character, s.status);
            stats.AddModifier(s.Modifier);
        }
    }

    protected override void RemoveAffixAffect()
    {
        foreach (StatusAdds s in stats)
        {
            Stats stats = GetStatus(character, s.status);
            stats.RemoveModifier(s.Modifier);
        }
    }  

    public Stats GetStatus(Character character, Status status)
    {
        switch (status)
        {
            case Status.STR:
                return character.data.status.strength;
            case Status.AGI:
                return character.data.status.agility;
            case Status.VIT:
                return character.data.status.vitality;
            case Status.INT:
                return character.data.status.intelligence;
            case Status.DEX:
                return character.data.status.dexterity;
            case Status.LUK:
                return character.data.status.lucky;
        }

        return null;
    }

    [System.Serializable]
    public struct StatusAdds
    {
        [Header("對應數值設定")]
        public Status status;
        public StatModType modType;
        private bool resetModTrigger;
        private StatModifier _modifier;
        public StatModifier Modifier
        {
            get
            {
                if (resetModTrigger)
                {
                    resetModTrigger = false;
                    _modifier = new StatModifier(value, modType);
                    return _modifier;
                }
                else
                {
                    return _modifier;
                }
            }
        }

        [Header("增減量")]
        public int value;

        public StatModifier GetModifier()
        {
            return new StatModifier(value, modType);
        }
    }

    public enum Status
    {
        STR,
        AGI,
        VIT,
        INT,
        DEX,
        LUK,
    }
}
