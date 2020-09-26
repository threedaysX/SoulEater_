using StatsModifierModel;
using System.Collections;
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
    private bool typeChanged;
    private const string stageOpening = "Stage_Opening";
    private const string stage1 = "Stage_1";
    private const string stage2 = "Stage_2";
    private const string stageDie = "Stage_Die";

    public override void Start()
    {
        base.Start();
        ResetStats();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    private void ResetStats()
    {
        healthUI = EnemyUIControl.Instance.healthWhite.GetComponent<UIShake>();
        ai._facement = new HorizontalFacement();
        SetEnemyLevel(EnemyLevel.Boss);
        ForceAdjustAttackDelay();
    }

    protected void ForceAdjustAttackDelay()
    {
        data.attackDelay.ForceToChangeValue(forceAttackDelay);
    }

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
        CheckSwitchToStage2();
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
        SetupStage(stageDie);
    }

    public override void MusicOpeningPlay()
    {
        SetupStage(stageOpening);
        Counter.Instance.StartCountDown(
            openingEffect.main.startLifetime.constant
            , false
            , null
            , delegate { SetupStage(stage1); });
    }

    public void CheckSwitchToStage2()
    {
        if (!typeChanged && CurrentHealth <= data.maxHealth.Value * 0.2f)
        {
            typeChanged = true;
            SetupStage(stage2);
        }
    }

    public override void CameraLockOn(float lockDuration)
    {
        var vcamIfrit = CharacterVcamControl.Instance.ifrit.vcam;
        var vcamPlayer = CharacterVcamControl.Instance.player.vcam;
        var styleIfrit = Cinemachine.CinemachineBlendDefinition.Style.EaseIn;
        var stylePlayer = Cinemachine.CinemachineBlendDefinition.Style.EaseOut;
        CameraFollowSetting[] sets = new CameraFollowSetting[]
        {
            new CameraFollowSetting(vcamIfrit, styleIfrit, lockDuration, 0.5f, 0.2f, false, delegate { ImageEffectController.Instance.SetMotionBlur(1, 0.2f); }, ResetAiSwitchOn),
            new CameraFollowSetting(vcamPlayer, stylePlayer, 0f, 0.5f, 0f, false, null, ImageEffectController.Instance.DisableMotionBlur)
        };
        CameraControl.Follow.Instance.AddSet(sets);
    }

    public override void SetupStage(string stageName)
    {
        switch (stageName)
        {
            case stageOpening:
                _stage = new Stage_Opening();
                break;
            case stage1:
                _stage = new Stage_1();
                break;
            case stage2:
                _stage = new Stage_2(this);
                break;
            case stageDie:
                _stage = new Stage_Die();
                break;
            default:
                break;
        }
        _stage.StartStageChangeAction();
    }

    #region Stage Class 
    public class Stage_Opening : IBossStageChangeEvent
    {
        public void StartStageChangeAction()
        {
            AudioSwitch();
        }

        private void AudioSwitch()
        {
            var audio = AudioControl.Fmod.Instance;
            audio.Startup(Music.Ifrit, true);
        }
    }

    public class Stage_1 : IBossStageChangeEvent
    {
        public void StartStageChangeAction()
        {
            AudioSwitch();
        }

        private void AudioSwitch()
        {
            var audio = AudioControl.Fmod.Instance;
            audio.Setup(Music.Ifrit, stage1, 1);
            audio.Setup(Music.Ifrit, stage2, 0);
            audio.Setup(Music.Ifrit, stageDie, 0);
        }
    }

    public class Stage_2 : IBossStageChangeEvent
    {
        private Ifrit ifrit;

        public Stage_2(Ifrit ifrit)
        {
            this.ifrit = ifrit;
        }

        public void StartStageChangeAction()
        {
            AudioSwitch();
            ifrit.StartCoroutine(TypeChange());
            FlamethrowerTypeChange();
        }

        private void AudioSwitch()
        {
            var audio = AudioControl.Fmod.Instance;
            audio.Setup(Music.Ifrit, stage1, 0);
            audio.Setup(Music.Ifrit, stage2, 1);
            audio.Setup(Music.Ifrit, stageDie, 0);
        }

        private IEnumerator TypeChange()
        {
            ifrit.LockOperation(LockType.TypeChange, true);
            ifrit.GetIntoImmune(ifrit.typeChangeDuration);
            CameraControl.Shake.Instance.ShakeCamera(0.2f, 12f, 2f, true);
            ifrit.typeChangingEffect.Play(true);
            ifrit.opsc.PlaySound(ifrit.typeChangingSound);

            yield return new WaitForSeconds(ifrit.typeChangeDuration);

            ifrit.opsc.PlaySound(ifrit.typeChangedBurstSound);
            ifrit.LockOperation(LockType.TypeChange, false);
            ifrit.UseSkill(ifrit.GetSkillByName(SkillNameDictionary.shockWave), true, true);
        }

        private void FlamethrowerTypeChange()
        {
            // 讓噴射詠唱更短
            ifrit.GetSkillByName(SkillNameDictionary.flamethrower)
                .castTime
                .AddModifier(new StatModifier(-50, StatModType.PercentageTime));
        }

        private void NaraBurstTypeChange()
        {
            // 讓Burst爆裂更快速
        }
    }

    public class Stage_Die : IBossStageChangeEvent
    {
        public void StartStageChangeAction()
        {
            AudioSwitch();
        }

        private void AudioSwitch()
        {
            var audio = AudioControl.Fmod.Instance;
            audio.Setup(Music.Ifrit, stage1, 0);
            audio.Setup(Music.Ifrit, stage2, 0);
            audio.Setup(Music.Ifrit, stageDie, 1);
        }
    }
    #endregion
}