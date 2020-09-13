using System.Collections;
using UnityEngine;

public class DashSlash : DisposableSkill
{
    public ParticleSystem chargeTrailEffect; // 衝刺特效
    public ParticleSystem drawnSwordEffect; // 拔劍特效
    public ParticleSystem slashHitEffect;   // 斬擊特效
    public AudioClip slashHitSound; // 斬擊音效
    private bool isHit;

    public override void CastSkill()
    {
        base.CastSkill();

        sourceCaster.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        drawnSwordEffect.transform.position = sourceCaster.transform.position + sourceCaster.transform.right * 0.5f;
        drawnSwordEffect.Play(true);
        GetImmune();
    }

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(GetInDarkScreenAndZoomIn);
        immediatelyAffect.AddListener(MoveFoward);
        immediatelyAffect.AddListener(delegate { StartCoroutine(HitDetect()); });
        hitAffect.AddListener(delegate 
        {
            isHit = true;
            BindEnemyAction(target, 1.8f);
            DamageTarget(1f);
        });
    }

    protected override bool Damage(float damageDirectionX = 0)
    {
        TriggerHitEffect(target);
        CameraShakeWhenHit();
        return base.Damage(transform.right.x);
    }

    private void TriggerHitEffect(Character target)
    {
        if (target == null)
            return;
        slashHitEffect.transform.position = target.transform.position;
        slashHitEffect.Play(true);
        soundControl.PlaySound(slashHitSound);
    }

    // 命中後，立即鎖定敵人動作
    private void BindEnemyAction(Character target, float duration)
    {
        Debuff.Instance.Bind(target, duration);
    }

    // 詠唱技能時，就立即進入無敵狀態
    private void GetImmune()
    {
        sourceCaster.GetIntoImmune(1f);
    }

    // 使用技能後，立即進入模糊畫面與畫面特寫
    private void GetInDarkScreenAndZoomIn()
    {
        ZoomInSetting[] zoomInSetting = new ZoomInSetting[] 
        {
            new ZoomInSetting { finalZoomSize = 5.5f, duration = 0.05f, startDelay = 0f },
            new ZoomInSetting { finalZoomSize = 6f, duration = 0.2f, startDelay = 0.6f }
        };
        CameraControl.Zoom.Instance.AddSet(zoomInSetting);

        ImageEffectController.Instance.SetMotionBlur(1f, 0.2f, 0.3f);
    }

    /// <summary>
    /// 0.1秒內瞬移到前方6m處。	
    /// </summary>
    private void MoveFoward()
    {
        StartCoroutine(MoveToPosition(sourceCaster.transform
            , sourceCaster.transform.position + sourceCaster.transform.right * currentSkill.range.Value
            , 0.1f));
    }
    public IEnumerator MoveToPosition(Transform transform, Vector3 destination, float timeToMove)
    {
        var originPos = transform.position;
        var t = 0f;
        while (t < 1)
        {
            // 讓技能跟隨玩家，並偵測是否命中
            this.transform.position = sourceCaster.transform.position + sourceCaster.transform.right * 0.8f;
            t += Time.deltaTime / timeToMove;
            transform.position = Vector3.Lerp(originPos, destination, t);
            yield return null;
        }

        chargeTrailEffect.Play(true);
    }

    private IEnumerator HitDetect()
    {
        yield return new WaitForSeconds(currentSkill.castTime.Value + 0.1f);
        StartCoroutine(SetActiveAfterSkillDone(1.9f));
        if (!isHit)
        {
            SetSkillCollisionEnable(false);
        }
    }

    private void CameraShakeWhenHit()
    {
        CameraControl.Shake.Instance.ShakeCamera(2f, 8f, 0.05f, false, 0f, true);
    }
}