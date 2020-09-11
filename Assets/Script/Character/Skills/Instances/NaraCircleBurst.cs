using System.Collections;
using UnityEngine;

public class NaraCircleBurst : DisposableSkill
{
    public NaraCircleBurstData data;
    public bool isBursted;

    protected override void AddAffectEvent()
    {
        hitAffect.AddListener(delegate
        {
            Ignite();
            DamageTarget();
        });
    }

    public override void GenerateSkill(Character character, Skill skill)
    {
        StartCoroutine(CheckExistState());
        base.GenerateSkill(character, skill);
    }

    public override void UseSkill()
    {
        isBursted = true;
        CameraShakeWhenBurst();
        StartCoroutine(SetActiveAfterSkillDone(0.3f));
        base.UseSkill();
    }

    private void Ignite()
    {
        Debuff.Instance.Ignite(sourceCaster, target, data.igniteDuration);
    }

    private void CameraShakeWhenBurst()
    {
        CameraControl.Shake.Instance.ShakeCamera(0.5f, 10f, 0.1f, false, 0f, true);
    }

    /// <summary>
    /// Check object need to set active false or not (Exists too long but not burst).
    /// </summary>
    private IEnumerator CheckExistState()
    {
        yield return new WaitForSeconds(data.delay + 6f);
        if (!isBursted)
        {
            SetActiveAfterSkillDone(false);
        }
    }
}

[System.Serializable]
public struct NaraCircleBurstData
{
    public Transform transform;
    public float delay;
    public float igniteDuration;
}
