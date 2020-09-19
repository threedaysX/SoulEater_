using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/BasicAttack")]
public class BasicAttack : AiAction
{
    private float animDelay;
    private Character character;

    public override bool StartActHaviour()
    {
        return Attack();
    }

    private bool Attack()
    {
        if (character == null)
            character = AI<Character>();
        ApplyAttackAnimationDelay();
        if (character.StartAttack(AttackType.Attack, character.data.attackElement))
        {
            return true;
        }
        return false;
    }

    private void ApplyAttackAnimationDelay()
    {
        animDelay = character.preAttackAnimDuration + character.attackAnimDuration;
        ApplyActionDelay(animDelay);
    }
}
