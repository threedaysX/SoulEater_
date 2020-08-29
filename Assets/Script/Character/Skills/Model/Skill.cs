﻿using StatsModel;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[CreateAssetMenu(menuName = "Character/Skill")]
public class Skill : ScriptableObject
{
    public Sprite icon;
    public SkillAttributesType[] attributes;
    public AttackType skillType;
    public ElementType elementType = ElementType.None;
    public string skillName;
    public string description;

    /// <summary>
    /// 技能施放距離(與自身相對距離)
    /// </summary>
    public float centerPositionOffset = 1f;

    /// <summary>
    /// 技能效果距離範圍
    /// </summary>
    public Stats range;

    /// <summary>
    /// 技能消耗
    /// </summary>
    public Stats cost;
    public CostType costType = CostType.Mana;

    /// <summary>
    /// 技能倍率(基於人物攻擊值)(%)
    /// </summary>
    public Stats damageMagnification;

    /// <summary>
    /// 技能攻擊次數
    /// </summary>
    public Stats damageHitTimes;

    /// <summary>
    /// 技能造成每次傷害所需的時間間隔(Ex: 每[0.2]秒造成100傷害...)
    /// </summary>
    public float timesOfPerDamage = 1f;

    /// <summary>
    /// 技能詠唱時間
    /// </summary>
    public Stats castTime;
    /// <summary>
    /// 技能固定詠唱時間
    /// </summary>
    public Stats fixedCastTime;

    /// <summary>
    /// 技能冷卻
    /// </summary>
    public Stats coolDown;
    public float trueCoolDown;
    public float coolDownTimer;
    public bool cooling = false;
    public SkillCoolDownType coolDownType;

    /// <summary>
    /// 技能的持續時間
    /// </summary>
    public float duration = 1f;

    public GameObject prefab;

    /// <summary>
    /// 詠唱時技能額外的觸發效果
    /// </summary>
    public UnityEvent immediatelyEvent;

    /// <summary>
    /// 命中時技能額外的觸發效果
    /// </summary>
    public UnityEvent hitEvent;
}

