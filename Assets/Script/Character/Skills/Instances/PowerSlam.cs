using System.Collections;
using UnityEngine;

public class PowerSlam : DisposableSkill
{
    public ParticleSystem powerStoreEffect;
    public ParticleSystem hitEffect;

    public float knockBackForce = 5f;
    public float armorBreakDuration = 8f;

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(delegate
        {
            StartCoroutine(SetActiveAfterSkillDone(2f));
            StartCoroutine(SetSkillCollisionEnable(false, 0.1f));
        });
        hitAffect.AddListener(delegate
        {
            DebuffArmorBreak();
            DamageTarget(target);
            TriggerHitEffect();
            KnockBack();
        });
    }

    public override void CastSkill()
    {
        base.CastSkill();
        AdjustHitRange();
        PowerStore();
    }

    public override void SkillingOperationLock()
    {
        sourceCaster.move.Lock(LockType.Operation);
        sourceCaster.attack.Lock(LockType.Operation);
        sourceCaster.evade.Lock(LockType.Operation);
        sourceCaster.jump.Lock(LockType.Operation);
        sourceCaster.useSkill.Lock(LockType.Operation);
    }

    private void TriggerHitEffect()
    {
        // Adjust effect pos.
        SpriteRenderer rdr = target.GetComponent<SpriteRenderer>();
        float yHalfBodySize = rdr.bounds.size.y / 2;
        Vector3 center = target.transform.position;
        center.y -= yHalfBodySize;
        hitEffect.transform.position = center;
        hitEffect.Play(true);

        // Camera Control.
        CameraControl.Shake.Instance.ShakeCamera(3f, 20f, 0.2f);
    }

    private void DebuffArmorBreak()
    {
        Debuff.Instance.ArmorBreakWithLevel(target, 2, armorBreakDuration);
    }

    private void KnockBack()
    {
        KnockStunSystem targetKnock = target.GetComponent<KnockStunSystem>();
        float directionX = new Vector2(sourceCaster.transform.position.x - target.transform.position.x, 0).normalized.x;
        targetKnock.KnockStun(target, directionX, knockBackForce);
    }

    private void PowerStore()
    {
        // Effect Control.
        float castTime = SkillController.GetCastTime(sourceCaster, currentSkill);
        var energeMain = powerStoreEffect.main;
        energeMain.startLifetime = castTime;
        powerStoreEffect.transform.position = sourceCaster.transform.position;
        powerStoreEffect.Play(true);

        // Camera Control.
        ZoomInSetting[] zoomInSetting = new ZoomInSetting[]
        {
            new ZoomInSetting { finalZoomSize = 5.5f, duration = castTime, startDelay = 0f },
            new ZoomInSetting { finalZoomSize = 6f, duration = 0.1f, startDelay = 0.05f }
        };

        CameraControl.Zoom.Instance.AddSet(zoomInSetting);
        CameraControl.Shake.Instance.ShakeCameraLinear(2f, 10f, castTime);

        // Character Control.
        StartCoroutine(LockCasterPosition(castTime));
    }

    private void AdjustHitRange()
    {
        GetComponent<CircleCollider2D>().radius = currentSkill.range.Value;
    }

    private IEnumerator LockCasterPosition(float castTime)
    {
        sourceCaster.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(castTime);
        sourceCaster.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        sourceCaster.rb.velocity = new Vector2(0, 0);
    }
}

     