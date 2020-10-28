using StatsModel;
using StatsModifierModel;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Endowment/Affix/StatusBuff")]
public class StatusBuffAffix : ImediatellyTriggerAffix
{
    public List<StatusAdds> stats;

    protected override void SetAffect()
    {
        foreach (StatusAdds s in stats)
        {
            Stats stats = GetStatus(owner, s.status);
            stats.AddModifier(s.Modifier);
        }
        owner.ResetBaseData();
    }

    protected override void RemoveAffixAffect()
    {
        foreach (StatusAdds s in stats)
        {
            Stats stats = GetStatus(owner, s.status);
            stats.RemoveModifier(s.Modifier);
        }
        owner.ResetBaseData();
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
            case Status.AP_Value:
                return character.data.penetrationValue;
            case Status.AP_Percentage:
                return character.data.penetrationMagnification;
            case Status.DEF:
                return character.data.defense;
            case Status.HP:
                return character.data.maxHealth;
            case Status.Mana:
                return character.data.maxMana;
            case Status.AttackLifeSteal:
                return character.data.attackLifeSteal;
            case Status.SkillLifeSteal:
                return character.data.skillLifeSteal;
            case Status.SkillCoolDown:
                return character.data.reduceSkillCoolDown;
            case Status.FireResistance:
                return character.data.resistance.fire;
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
        AP_Value,
        AP_Percentage,
        DEF,
        HP,
        Mana,
        AttackLifeSteal,
        SkillLifeSteal,
        SkillCoolDown,
        FireResistance,
    }
}
