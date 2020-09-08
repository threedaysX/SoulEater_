using UnityEngine;

/// <summary>
/// 持續性技能
/// </summary>
public abstract class LastingSkill : SkillEventBase
{
    protected float nextDamageTime = 0;

    /// <summary>
    /// [針對各個技能]
    /// 當技能碰觸到物件時 (造成傷害、異常...)
    /// 該技能的目標為何？
    /// </summary>
    public virtual void OnTriggerStay2D(Collider2D col)
    {
        // Basic filter.
        if (sourceCaster == null || col == null)
            return;

        // 如果目標是自己，除非技能可以對自己造成效果，否則略過
        if (!canTriggerSelf && col.CompareTag(sourceCaster.tag))
            return;

        // Get target.
        this.target = col.GetComponent<Character>();
        if (this.target == null || this.target.GetImmuneState())
            return;

        // Trigger affect(with duration).
        if (Time.time >= nextDamageTime)
        {
            InvokeHitAffect();
            nextDamageTime = Time.time + currentSkill.timesOfPerDamage;
        }
    }
}
