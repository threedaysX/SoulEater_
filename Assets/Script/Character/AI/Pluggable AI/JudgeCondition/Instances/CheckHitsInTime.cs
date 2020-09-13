using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/CheckHitsInTime")]
public class CheckHitsInTime : JudgeCondition
{
    [Header("在幾秒內有受傷")]
    public float customTime;

    private Character character;

    public override bool CheckActConditionHaviour()
    {
        if (character == null)
            character = AI<Character>();

        character.combatController.hasHitInTime = customTime;
        return character.combatController.hasHit;
    }
}