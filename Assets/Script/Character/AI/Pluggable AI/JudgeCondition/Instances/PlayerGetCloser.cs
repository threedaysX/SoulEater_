using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/JudgeCondition/PlayerGetCloser")]
public class PlayerGetCloser : JudgeCondition
{
    [Header("玩家接近多少")]
    public float distancePlayerGetClose;
    [Header("在幾秒內")]
    public float timeToGetClose;
    
    public override bool CheckActConditionHaviour()
    {
        Ai.detectControl.customDistance = distancePlayerGetClose;
        Ai.detectControl.timeToAct = timeToGetClose;
        return Ai.detectControl.hasGetClose;
    }
}
