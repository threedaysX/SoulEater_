using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerSkillSlotKeyControl))]
[RequireComponent(typeof(PlayerMovement))]
public class Player : Character
{
    [Header("靈魂吸取")]
    public ParticleSystemForceField attractor;
    public ParticleSystem burstEffect;
    public ParticleSystem manaAbsorbEffect;

    [Header("死亡相關")]
    public ParticleSystem dieParticle;
    public Light2D playerLight;

    [Header("玩家UI")]
    public UIShake healthUI;
    public Image staminaBarUI;
    public Image manaAbsorbBarUI;

    private string dscManaAbsorbName;

    private void Start()
    {
        this.tag = "Player";
        this.gameObject.layer = LayerMask.NameToLayer("Player");
        // Set ManaAbsorb UIbar.
        dscManaAbsorbName = CumulativeDataController.Cumulative_DamageDealt_ManaAbsorb;
        manaAbsorbBarUI.fillAmount = cumulativeDataController.GetData(dscManaAbsorbName, CumulativeDataType.Dealt) / data.manaStealOfDamage.Value;
    }

    private void Update()
    {
        if (Input.GetKeyDown(HotKeyController.GetHotKey(HotKeyType.AttackKey1)) || Input.GetKeyDown(HotKeyController.GetHotKey(HotKeyType.AttackKey2)))
        {
            StartAttack(AttackType.Attack, data.attackElement);
        }

        ResetBarUI();
    }

    public override bool TakeDamage(DamageData damageData)
    {
        bool isDamaged = base.TakeDamage(damageData);
        if (isDamaged)
        {
            if (CurrentHealth <= data.maxHealth.Value * 0.25f)
            {
                healthUI.Shake(0.15f);
            }
            else if (CurrentHealth <= data.maxHealth.Value * 0.5f)
            {
                healthUI.Shake(0.12f);
            }
            else if (CurrentHealth <= data.maxHealth.Value * 0.75f)
            {
                healthUI.Shake(0.08f);
            }
        }
        return isDamaged;
    }

    public override void Die()
    {
        ResetBarUI();
        TimeScaleController.Instance.DoSlowMotion(0.05f, 0f, dieController.dieDuration);
        playerLight.gameObject.SetActive(false);
        base.Die();
        CameraControl.Follow.Instance.FollowTargetSimple(lastAttackMeTarget.transform);
    }

    protected override void ManaSteal(float damage)
    {
        float percent = cumulativeDataController.GetData(dscManaAbsorbName, CumulativeDataType.Dealt) / data.manaStealOfDamage.Value;
        if (percent >= 1)
        {
            percent = 1;
            manaAbsorbEffect.Play(true);
        }
        base.ManaSteal(damage);
        manaAbsorbBarUI.fillAmount = percent;
    }

    public void TriggerAttractorBurstEffect(float delay)
    {
        StartCoroutine(AbsorbSoulBurst(delay));
    }

    private IEnumerator AbsorbSoulBurst(float delay)
    {
        yield return new WaitForSeconds(delay);
        burstEffect.Play(true);
    }

    private void ResetBarUI()
    {
        if (isHealthDirty)
        {
            PlayerUIControl.Instance.SetHealthUI(data.maxHealth.Value, CurrentHealth);
            isHealthDirty = false;
        }

        if (isManaDirty)
        {
            PlayerUIControl.Instance.SetManaUI(data.maxMana.Value, CurrentMana);
            isManaDirty = false;
        }
    }
}
