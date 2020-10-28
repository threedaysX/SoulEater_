using UnityEngine;

[CreateAssetMenu(menuName = "Endowment/Affix/LearnSkill")]
public class LearnSkilllAffix : ImediatellyTriggerAffix
{
    public Skill skill;

    protected override void SetAffect()
    {
        owner.LearnSkill(skill);
    }

    protected override void RemoveAffixAffect()
    {
        owner.RemoveSkill(skill);
    }
}
