using UnityEngine;

public abstract class SkillAiAction : AiAction
{
    [Header("技能Prefab")]
    public Skill skillActionObject;

    public override bool StartActHaviour()
    {
        return TriggerSkill();
    }

    protected virtual bool TriggerSkill()
    {
        ApplySkillActionDelay();
        return ai.UseSkill(skillActionObject);
    }

    protected virtual void ApplySkillActionDelay()
    {
        float delay = skillActionObject.castTime.Value + skillActionObject.fixedCastTime.Value + skillActionObject.duration;
        ApplyActionDelay(delay);
    }
}
