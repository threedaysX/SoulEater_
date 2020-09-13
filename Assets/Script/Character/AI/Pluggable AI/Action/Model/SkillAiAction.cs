using UnityEngine;

public abstract class SkillAiAction : AiAction
{
    [Header("技能Prefab")]
    public Skill skillActionObject;

    private Character character;

    public override bool StartActHaviour()
    {
        return TriggerSkill();
    }

    protected virtual bool TriggerSkill()
    {
        if (character == null)
            character = AI<Character>();
        ApplySkillActionDelay();
        return character.UseSkill(skillActionObject);
    }

    protected virtual void ApplySkillActionDelay()
    {
        float delay = skillActionObject.castTime.Value + skillActionObject.fixedCastTime.Value + skillActionObject.duration;
        ApplyActionDelay(delay);
    }
}
