using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/BasicAttack")]
public class BasicAttack : AiAction
{
    [Header("動作動畫")]
    public ActionAnimationClip[] clips;
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
        if (character.StartAttack(AttackType.Attack, character.data.attackElement))
        {
            ApplyAttackAnimationDelay();
            return true;
        }
        return false;
    }

    private void ApplyAttackAnimationDelay()
    {
        animDelay = 0;
        if (clips != null)
        {
            foreach (var c in clips)
            {
                float length = c.clip.length;
                animDelay += length;
            }
        }
        ApplyActionDelay(animDelay);
    }

    [System.Serializable]
    public struct ActionAnimationClip
    {
        public AttackActionType type;
        public AnimationClip clip;
    }

    public enum AttackActionType
    {
        PreAttack,
        InAttack
    }
}
