using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/BasicAttack")]
public class BasicAttack : AiAction
{
    [Header("動作動畫")]
    public AnimationClip[] clips;
    private float animDelay;

    public override bool StartActHaviour()
    {
        return Attack();
    }

    private bool Attack()
    {
        ApplyAttackAnimationDelay();
        if (ai.StartAttack(AttackType.Attack, ai.data.attackElement))
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
