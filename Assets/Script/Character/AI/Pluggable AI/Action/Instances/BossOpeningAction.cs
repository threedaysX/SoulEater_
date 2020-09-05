public class BossOpeningAction : AiAction
{
    public override bool StartActHaviour()
    {
        return StartBossOpeningAction();
    }

    private bool StartBossOpeningAction()
    {
        return ai.GetComponent<IBossOpeningEvent>().StartOpeningAction();
    }
}
