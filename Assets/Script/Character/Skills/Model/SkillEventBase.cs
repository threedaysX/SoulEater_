using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 各類技能的事件。
/// 包含對自身的、對敵人...等等的行為模式(Buff、擊退、暈眩...事件)。
/// </summary>
public abstract class SkillEventBase : MonoBehaviour, ISkillGenerator, ISkillUse, ISkillCaster, ISkillOperationLock
{
    private const string skillAnimTrigger = "Trigger";

    [Header("碰撞設定")]
    public bool autoRenderCollider = true;
    public bool canTriggerSelf = false;

    // 基本資訊
    protected Skill currentSkill;
    protected Character sourceCaster;
    protected Character target;

    [Header("立即效果")]
    public UnityEvent immediatelyAffect;
    [Header("命中效果")]
    public UnityEvent hitAffect;

    // 動畫
    protected Animator anim;

    // 聲音
    protected OperationSoundController soundControl;
    public AudioClip inRenderSound;
    public AudioClip inCastSound;
    public AudioClip inUsingSound;

    // 特效
    public ParticleSystem inUsingParticle;

    protected virtual void Start()
    {
        if (autoRenderCollider)
        {
            this.gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
        }

        this.gameObject.layer = LayerMask.NameToLayer("Skill");
        
        // 附加技能效果
        AddAffectEvent();
    }

    /// <summary>
    /// 技能於物件池中生成至畫面時(尚未開始詠唱與使用前)
    /// </summary>
    public virtual void GenerateSkill(Character character, Skill skill)
    {
        sourceCaster = character;
        currentSkill = skill;

        anim = GetComponent<Animator>();
        soundControl = sourceCaster.opsc;

        // Active
        this.gameObject.SetActive(true);
        // 禁用技能Hitbox與貼圖 (避免生成技能時直接播放動畫與觸發效果)
        SetSkillCollisionEnable(false);

        if (soundControl != null && inRenderSound != null)
        {
            soundControl.PlaySound(inRenderSound);
        }
    }

    /// <summary>
    /// 技能使用(詠唱後)
    /// </summary>
    public virtual void UseSkill()
    {
        if (soundControl != null && inUsingSound != null)
        {
            soundControl.PlaySound(inUsingSound);
        }
        if (anim != null)
        {
            AnimationBase.Instance.PlayAnimationLoop(anim, skillAnimTrigger, currentSkill.duration, false, false);
        }
        if (inUsingParticle != null)
        {
            inUsingParticle.Play(true);
        }

        // 啟用技能
        SetSkillCollisionEnable(true);

        // 觸發立即性效果
        InvokeAffect(immediatelyAffect);
        InvokeAffect(currentSkill.immediatelyEvent);
    }

    public virtual void CastSkill()
    {
        PlayCastSound();
        SkillingOperationLock();
    }

    public virtual void SkillingOperationLock()
    {
        // 鎖定行動(預設技能使用期間，動作為全鎖定)
        sourceCaster.LockOperation(LockType.Operation, true);
    }

    /// <summary>
    /// 使用技能前，會撥放預設施放音效
    /// </summary>
    public virtual void PlayCastSound()
    {
        if (inCastSound == null)
        {
            soundControl.PlaySound(soundControl.castSound);
        }
        else
        {
            soundControl.PlaySound(this.inCastSound);
        }
    }

    /// <summary>
    /// 技能開始施放，可將Enabled為True
    /// </summary>
    protected void SetSkillCollisionEnable(bool enable)
    {
        // 技能貼圖
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.enabled = enable;
        }

        // 技能碰撞
        Collider2D col = this.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = enable;
        }
    }

    /// <summary>
    /// 技能開始施放，可將Enabled為True
    /// </summary>
    protected IEnumerator SetSkillCollisionEnable(bool enable, float delaySet)
    {
        yield return new WaitForSeconds(delaySet);

        SetSkillCollisionEnable(enable);
    }

    protected IEnumerator SetActiveAfterSkillDone(float duration)
    {
        yield return new WaitForSeconds(duration);
        SetActiveAfterSkillDone(false);
    }

    protected void SetActiveAfterSkillDone(bool active)
    {
        this.gameObject.SetActive(active);
    }

    protected void InvokeHitAffect()
    {
        InvokeAffect(hitAffect);
        InvokeAffect(currentSkill.hitEvent);
    }

    private void InvokeAffect(UnityEvent affectEvent)
    {
        if (affectEvent == null)
            return;
        affectEvent.Invoke();
    }

    public bool DamageTarget(Character target, float firstHitDelay = 0, float damageDirectionX = 0)
    {
        if (currentSkill.damageHitTimes.Value > 1)
        {
            StartCoroutine(DamageTargetTimes(target, firstHitDelay, damageDirectionX));
            return true;
        }
        else
        {
            return Damage(target, damageDirectionX);
        }
    }

    /// <summary>
    /// 每下傷害多次打擊用
    /// </summary>
    /// <param name="firstHitDelay">造成第一次傷害前的延遲(預設為0，命中立即造成傷害)</param>
    /// <param name="damageDirectionX">傷害來源方向</param>
    /// <returns></returns>
    protected virtual IEnumerator DamageTargetTimes(Character target, float firstHitDelay = 0, float damageDirectionX = 0)
    {        
        yield return new WaitForSeconds(firstHitDelay);

        for (int i = 0; i < currentSkill.damageHitTimes.Value; i++)
        {
            Damage(target, damageDirectionX);
            yield return new WaitForSeconds(currentSkill.timesOfPerDamage);
        }
    }

    protected virtual bool Damage(Character target, float damageDirectionX = 0)
    {
        float damage = GetSkillDamage(out bool isCritical);
        sourceCaster.DamageDealtSteal(damage, false);
        sourceCaster.combatController.SetLastAttackTarget(target);
        DamageData damageData = new DamageData(sourceCaster.gameObject, currentSkill.elementType, (int)damage, isCritical, damageDirectionX);
        return target.TakeDamage(damageData);
    }

    public virtual float GetSkillDamage(out bool isCritical)
    {
        return DamageController.Instance.GetSkillDamage(sourceCaster, target, currentSkill, out isCritical);
    }

    protected abstract void AddAffectEvent();

    #region 技能通用效果
    protected string lockDirectionBuffName = "方向鎖定";
    public virtual void LockDirectionTillEnd()
    {
        void affect() { sourceCaster.freeDirection.Lock(LockType.Operation); }
        void remove() { sourceCaster.freeDirection.UnLock(LockType.Operation); }
        sourceCaster.buffController.AddBuff(lockDirectionBuffName, affect, remove, currentSkill.duration);
    }

    /// <summary>
    /// 讓技能跟隨施法者
    /// </summary>
    /// <returns></returns>
    protected IEnumerator FollowCaster(float duration, params Action[] callback)
    {
        float timeleft = duration;
        while (timeleft > 0)
        {
            if (sourceCaster == null)
                yield break;

            this.transform.position = sourceCaster.transform.position;
            timeleft -= Time.deltaTime;
            yield return null;
        }

        foreach (var call in callback)
        {
            call.Invoke();
        }
    }
    #endregion
}

public interface ISkillGenerator
{
    void GenerateSkill(Character character, Skill skill);
}

public interface ISkillCaster
{
    void CastSkill();

    void PlayCastSound();
}

public interface ISkillUse
{
    void UseSkill();
}

public interface ISkillOperationLock
{
    void SkillingOperationLock();
}
