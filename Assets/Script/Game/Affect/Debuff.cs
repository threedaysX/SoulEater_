﻿using StatsModifierModel;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Debuff : Singleton<Debuff>
{
    [Header("Ignite")]
    public Sprite igniteIcon;
    public const string igniteDebuffName = "Ignite";

    [Header("Bind")]
    public Sprite bindIcon;
    public const string bindDebuffName = "Bind";

    [Header("Lame")]
    public Sprite lameIcon;
    public const string lameDebuffName = "Lame";

    [Header("Slow")]
    public Sprite slowIcon;
    public const string slowDebuffName = "Slow";

    [Header("ArmorBreak")]
    public Sprite armorBreakIcon;
    public const string armorBreakDebuffName = "ArmorBreak";

    private void ApplyDebuff(Character target, DebuffData data)
    {
        if (target.iconController == null)
            return;

        UnityEvent affectEvent = new UnityEvent();
        UnityEvent endEvent = new UnityEvent();
        affectEvent.AddListener(data.applyAffect);
        endEvent.AddListener(data.endAffect);

        // Pop Text Hint
        affectEvent.AddListener(delegate { target.PopTextMessage(data.name, Color.white, 0.8f); });
        endEvent.AddListener(delegate { target.PopTextMessage("-" + data.name, Color.white, 0.4f); });
        // Add icon remove event when debuff end.
        endEvent.AddListener(delegate { target.iconController.RemoveIcon(data.name); });

        target.buffController.AddBuff(data.name, affectEvent, endEvent, data.duration);
        target.iconController.SetIcon(data.name, data.icon);
    }

    #region public use
    /// <summary>
    /// 【定身】鎖定目標所有動作
    /// </summary>
    public void Bind(Character target, float duration)
    {
        DebuffData debuffData = new DebuffData
        {
            icon = bindIcon,
            name = bindDebuffName,
            duration = duration,
            applyAffect = delegate 
            { 
                target.move.Lock(LockType.Bind);
                target.attack.Lock(LockType.Bind);
                target.evade.Lock(LockType.Bind);
                target.jump.Lock(LockType.Bind);
                target.useSkill.Lock(LockType.Bind);
            },
            endAffect = delegate 
            {
                target.move.UnLock(LockType.Bind);
                target.attack.UnLock(LockType.Bind);
                target.evade.UnLock(LockType.Bind);
                target.jump.UnLock(LockType.Bind);
                target.useSkill.UnLock(LockType.Bind);
            },
        };

        ApplyDebuff(target, debuffData);
    }

    /// <summary>
    /// 【跛腳】目標無法使用跳躍，持續X秒。
    /// </summary>
    public void Lame(Character target, float duration)
    {
        DebuffData debuffData = new DebuffData
        {
            icon = lameIcon,
            name = lameDebuffName,
            duration = duration,
            applyAffect = delegate { target.jump.Lock(LockType.Lame); },
            endAffect = delegate { target.jump.UnLock(LockType.Lame); },
        };

        ApplyDebuff(target, debuffData);
    }

    /// <summary>
    /// 【緩速】減緩目標N%移動速度，持續X秒。
    /// </summary>
    /// <param name="percentage">N => [Range 1 ~ 100]%, retrun if out of range</param>
    /// <param name="duration">X => Debuff duration</param>
    public void SlowMoveSpeed(Character target, float percentage, float duration)
    {
        if (percentage > 100f || percentage <= 0f)
        {
            return;
        }

        var speedstat = target.data.moveSpeed;
        var mod = new StatModifier(-percentage, StatModType.PercentageTime, slowDebuffName);
        DebuffData debuffData = new DebuffData
        {
            icon = slowIcon,
            name = slowDebuffName,
            duration = duration,
            applyAffect = delegate { speedstat.AddModifier(mod); },
            endAffect = delegate { speedstat.RemoveModifier(mod); },
        };

        ApplyDebuff(target, debuffData);
    }

    /// <summary>
    /// 破甲Lv1: 對命中的目標造成-20%基礎防禦，持續N秒。			
    /// 破甲Lv2: -30%			
    /// 破甲Lv3: -45%		
    /// 破甲Lv4: -60%			
    /// 破甲Lv5: -80%			
    /// </summary>
    public void ArmorBreakWithLevel(Character target, int level, float duration)
    {
        var armorStat = target.data.defense;
        float value = 0;
        switch (level) 
        {
            case 1:
                value = -20f;
                break;
            case 2:
                value = -30f;
                break;
            case 3:
                value = -45f;
                break;
            case 4:
                value = -60f;
                break;
            case 5:
                value = -80f;
                break;
        }

        var mod = new StatModifier(value, StatModType.PercentageAdd, armorBreakDebuffName);
        DebuffData debuffData = new DebuffData
        {
            icon = armorBreakIcon,
            name = armorBreakDebuffName,
            duration = duration,
            applyAffect = delegate { armorStat.AddModifier(mod); },
            endAffect = delegate { armorStat.RemoveModifier(mod); },
        };

        ApplyDebuff(target, debuffData);
    }

    /// <summary>
    /// 【燃燒】點燃目標，並造成基於攻擊力與最大生命的傷害，持續X秒。
    /// </summary>
    /// <param name="source">造成異常的來源施法者</param>
    public void Ignite(Character source, Character target, float duration)
    {
        var currentFireResis = target.GetResistance(ElementType.Fire);
        // 火抗小於等於50不會被燃燒。
        if (currentFireResis.Value <= 50)
        {
            return;
        }

        DebuffData debuffData = new DebuffData
        {
            icon = igniteIcon,
            name = igniteDebuffName,
            duration = duration,
            applyAffect = delegate { IgniteEffect(source, target, duration); },
            endAffect = delegate { RemoveIgniteEffect(target); },
        };

        ApplyDebuff(target, debuffData);
    }
    #endregion

    #region Ignite
    protected virtual void IgniteEffect(Character source, Character target, float duration)
    {
        var currentFireResis = target.GetResistance(ElementType.Fire);
        // 進入燃燒狀態，提升火抗至50。
        currentFireResis.ForceToChangeValue(50f);
        IgniteDamage(source, target, duration);
    }

    protected void IgniteDamage(Character source, Character target, float duration)
    {
        // Damage target per sec.
        float damagePerTimes = 1f;
        // Damage Formula.
        float damage = (target.data.status.strength.BaseValue + 1) / 2 * ((target.data.status.intelligence.BaseValue + 1) / 2) * (target.data.maxHealth.Value * 0.001f);
        // [false] => Ignite would not trigger critical damage and damage target immediately.
        DamageData damageData = new DamageData(source.gameObject, ElementType.Fire, (int)damage, false, 0, 0, damagePerTimes, duration, false);
        target.TakeDamage(damageData);
    }

    protected virtual void RemoveIgniteEffect(Character target)
    {
        var currentFireResis = target.GetResistance(ElementType.Fire);
        currentFireResis.CancelForceValue();
    }
    #endregion
}

[Serializable]
public struct DebuffData
{
    public Sprite icon;
    public string name;
    public string description;
    public float duration;
    public UnityAction applyAffect;
    public UnityAction endAffect;
}

public enum DebuffEnum 
{ 
    Ignite,
    Bind,
    Lame,
    Slow,
    ArmorBreak,
}
