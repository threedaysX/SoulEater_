using UnityEngine;

public class SelfHeal : DisposableSkill
{
    public float effectDuration;

    protected override void AddAffectEvent()
    {
        immediatelyAffect.AddListener(Heal);
    }

    /// <summary>
    /// 恢復自身[最大生命10%+DEX*15+INT*50]生命
    /// </summary>
    private void Heal()
    {
        StartCoroutine(FollowCaster(effectDuration, delegate { this.SetActiveAfterSkillDone(false); }));
        sourceCaster.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        sourceCaster.CurrentHealth += (int)(sourceCaster.data.maxHealth.Value * 0.1f + sourceCaster.data.status.dexterity.Value * 15 + sourceCaster.data.status.intelligence.Value * 50);
    }
}