using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/BasicAttack")]
public class BasicAttack : AiAction
{
    [Header("動作動畫")]
    public AnimationClip[] clips;
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
        animDelay = 0;
        if (clips != null)
        {
            foreach (var clip in clips)
            {
                animDelay += clip.length;
            }
        }
        ApplyActionDelay(animDelay);
    }
}
