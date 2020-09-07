using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/Boss/OpeningAction")]
public class BossOpeningAction : AiAction
{
    public override bool StartActHaviour()
    {
        return StartBossOpeningAction();
    }

    private bool StartBossOpeningAction()
    {
        float duration = ai.GetComponent<IBossOpeningEvent>().StartOpeningAction();
        ApplyActionDelay(duration);
        // Just do this action once.
        SetAiActionSwitchOn(false);
        return true;
    }
}
