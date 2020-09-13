using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/GreaterThanCustomHealth")]
public class GreaterThanCustomHealth : JudgeCondition
{
    [Range(0, 100)]
    public float healthPercentageToCheck;

    private Character character;

    public override bool CheckActConditionHaviour()
    {
        return CheckIfHealthy();
    }

    private bool CheckIfHealthy()
    {
        if (character == null)
            character = AI<Character>();

        //healthPercentage
        if ((character.CurrentHealth / character.data.maxHealth.Value) * 100 > healthPercentageToCheck)
        {
            return true;
        }
        return false;
    }
}
