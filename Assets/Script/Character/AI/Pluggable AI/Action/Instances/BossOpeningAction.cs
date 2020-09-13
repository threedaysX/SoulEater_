using UnityEngine;

[CreateAssetMenu(menuName = "Character/AI/Action/Boss/OpeningAction")]
public class BossOpeningAction : AiAction
{
    private Character character;

    public override bool StartActHaviour()
    {
        return StartBossOpeningAction();
    }

    private bool StartBossOpeningAction()
    {
        if (character == null)
            character = AI<Character>();
        float duration = character.GetComponent<IBossOpeningEvent>().StartOpeningAction();
        ApplyActionDelay(duration);
        // Just do this action once.
        SetAiActionSwitchOn(false);
        return true;
    }
}
