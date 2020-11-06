using UnityEngine;

public class Teleport : DisposableSkill
{
    public AudioClip teleportSound;
    public float timeSlowDuration;

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(TriggerMotionBlur);
        immediatelyAffect.AddListener(TeleportToTargetBack);
        immediatelyAffect.AddListener(TimeSlow);
    }

    /// <summary>
    /// Teleport to last attack target back. If target is null, teleport foward.
    /// </summary>
    private void TeleportToTargetBack()
    {
        Character lastAttackTarget = sourceCaster.combatController.lastAttackTarget;
        // Still can teleport.
        if (lastAttackTarget == null)
        {
            sourceCaster.transform.position = new Vector3(sourceCaster.transform.position.x + sourceCaster.transform.right.x * currentSkill.range.Value, sourceCaster.transform.position.y);
            soundControl.PlaySound(teleportSound);
            return;
        }

        // Need in skill range.
        if (Vector3.Distance(lastAttackTarget.transform.position, sourceCaster.transform.position) > currentSkill.range.Value)
            return;

        float x = (lastAttackTarget.transform.position + lastAttackTarget.transform.right * -5f).x;
        sourceCaster.transform.position = new Vector3(x, sourceCaster.transform.position.y); 
        soundControl.PlaySound(teleportSound);
    }

    private void TimeSlow()
    {
        TimeScaleController.Instance.DoSlowMotion(0.1f, 0f, timeSlowDuration);
    }

    private void TriggerMotionBlur()
    {
        float blurDuration = 0.25f;
        var ec = ImageEffectController.Instance;
        ec.SetMotionBlur(1f, 0.2f);
        Counter.Instance.StartCountDown(blurDuration, false, null, delegate { ec.DisableMotionBlur(); });
    }
}