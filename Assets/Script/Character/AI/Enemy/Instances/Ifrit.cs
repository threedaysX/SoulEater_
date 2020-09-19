using StatsModifierModel;
using UnityEngine;

/// <summary>
/// 伊夫利特
/// </summary>
public class Ifrit : BossModel
{
    [Header("血量UI震動")]
    public UIShake healthUI;

    [Header("攻擊速度(延遲)")]
    [Range(0f, 2f)]
    public float forceAttackDelay;

    [Header("型態改變")]
    public AudioClip typeChangingSound;
    public AudioClip typeChangedBurstSound;
    public ParticleSystem typeChangingEffect;
    public float typeChangeDuration;
    private bool flamethrowerTypeChanged;

    public override void Start()
    {
        base.Start();
        ResetStats();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        FlamethrowerTypeChange();
        NaraBurstTypeChange();
    }

    private void ResetStats()
    {
        healthUI = EnemyUIControl.Instance.healthWhite.GetComponent<UIShake>();
        ai._facement = new HorizontalFacement();
        SetEnemyLevel(EnemyLevel.Boss);
        ResetFlamethrowerData();
        ForceAdjustAttackDelay();
    }

    protected void ForceAdjustAttackDelay()
    {
        data.attackDelay.ForceToChangeValue(forceAttackDelay);
    }

    protected void NaraBurstTypeChange()
    {
        // 當血量小於30%，讓Burst爆裂更快速
    }

    private void ResetFlamethrowerData()
    {
        flamethrowerTypeChanged = false;
    }

    protected void FlamethrowerTypeChange()
    {
        // 當血量小於50%，讓噴射詠唱更短
        if (!flamethrowerTypeChanged && CurrentHealth > 0 && CurrentHealth <= data.maxHealth.Value * 0.5)
        {
            GetSkillByName(SkillNameDictionary.flamethrower).castTime.AddModifier(new StatModifier(-50, StatModType.PercentageTime));
            flamethrowerTypeChanged = true;
        }
    }

    ///// <summary>
    ///// Use AiAction to Call.
    ///// </summary>
    ///// <returns></returns>
    //public void ChangeType()
    //{
    //    StartCoroutine(TypeChange());
    //}
    //private IEnumerator TypeChange()
    //{
    //    this.CanAction = false;
    //    GetIntoImmune(typeChangeDuration);
    //    CameraShake.Instance.ShakeCamera(1f, 10f, typeChangeDuration, true);
    //    typeChangingEffect.Play();
    //    operationSoundController.PlaySound(typeChangingSound);

    //    yield return new WaitForSeconds(typeChangeDuration);

    //    operationSoundController.PlaySound(typeChangedBurstSound);
    //    UseSkill(GetSkillByName(SkillNameDictionary.shockWave), true, true);
    //    this.CanAction = true;
    //}

    public override bool TakeDamage(DamageData damageData)
    {
        bool isDamaged = base.TakeDamage(damageData);
        if (isDamaged && healthUI != null)
        {
            // 根據血量調整震動幅度
            if (CurrentHealth <= data.maxHealth.Value * 0.25f)
            {
                healthUI.Shake(0.15f);
            }
            else if (CurrentHealth <= data.maxHealth.Value * 0.5f)
            {
                healthUI.Shake(0.1f);
            }
            else if (CurrentHealth <= data.maxHealth.Value * 0.75f)
            {
                healthUI.Shake(0.06f);
            }
        }
        return isDamaged;
    }

    public override bool StartAttack(AttackType attackType = AttackType.Attack, ElementType elementType = ElementType.None, bool forceActToEnd = false)
    {
        if (attack.CanDo)
        {
            DoPreActHint();
        }
        return base.StartAttack(attackType, elementType, forceActToEnd);
    }

    public override void Die()
    {
        CameraControl.Shake.Instance.ShakeCamera(1f, 4f, dieController.dieDissolveDuration);
        base.Die();
    }

    public override void OnRootStart()
    {
        // Trigger Some story.
    }

    public override void MusicOpeningPlay()
    {
        AudioControl.TimeLine.Instance.PlayMusic(Music.Ifrit, 0);
    }

    public override void CameraOpeningMove()
    {
        var vcamIfrit = CharacterVcamControl.Instance.ifrit.vcam;
        var vcamPlayer = CharacterVcamControl.Instance.player.vcam;
        var styleIfrit = Cinemachine.CinemachineBlendDefinition.Style.EaseIn;
        var stylePlayer = Cinemachine.CinemachineBlendDefinition.Style.EaseOut;
        CameraFollowSetting[] sets = new CameraFollowSetting[]
        {
            new CameraFollowSetting(vcamIfrit, styleIfrit, 2f, 0.5f, 0.2f, false, delegate { ImageEffectController.Instance.SetMotionBlur(1, 0.2f); }, ResetAiSwitchOn),
            new CameraFollowSetting(vcamPlayer, stylePlayer, 0f, 0.5f, 0f, false, null, ImageEffectController.Instance.DisableMotionBlur)
        };
        CameraControl.Follow.Instance.AddSet(sets);
    }
}