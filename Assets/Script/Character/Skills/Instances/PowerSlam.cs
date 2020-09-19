using System.Collections;
using UnityEngine;

public class PowerSlam : DisposableSkill
{
    public ParticleSystem powerStoreSparkEffect;
    public ParticleSystem powerStoreEnergeEffect;

    protected override void AddAffectEvent()
    {
        hitAffect.AddListener(delegate
        {
            DebuffArmorBreak();
            DamageTarget(target);
        });
    }

    public override void CastSkill()
    {
        base.CastSkill();
        PowerStore();
    }

    private void DebuffArmorBreak()
    {
        Debuff.Instance.ArmorBreakWithLevel(target, 1, 4f);
    }

    private void PowerStore()
    {
        float castTime = SkillController.GetCastTime(sourceCaster, currentSkill);
        var sparkMain = powerStoreSparkEffect.main;
        var energeMain = powerStoreEnergeEffect.main;
        sparkMain.duration = castTime;
        energeMain.startLifetime = castTime;
        powerStoreSparkEffect.transform.position = sourceCaster.transform.position;
        powerStoreEnergeEffect.transform.position = sourceCaster.transform.position;
        powerStoreSparkEffect.Play(true);
        powerStoreEnergeEffect.Play(true);
        ZoomInSetting[] zoomInSetting = new ZoomInSetting[]
        {
            new ZoomInSetting { finalZoomSize = 5.5f, duration = castTime, startDelay = 0f },
            new ZoomInSetting { finalZoomSize = 6f, duration = 0.1f, startDelay = 0.05f }
        };

        StartCoroutine(LockCasterPosition(castTime));
        CameraControl.Zoom.Instance.AddSet(zoomInSetting);
        CameraControl.Shake.Instance.ShakeCameraLinear(2f, 5f, castTime);
    }

    private IEnumerator LockCasterPosition(float castTime)
    {
        sourceCaster.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(castTime);
        sourceCaster.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}

     
