public abstract class ImediatellyTriggerAffix : Affix
{
    protected override void InitAffix()
    {
        // duration: -1, 代表永久
        owner.buffController.AddBuff(affectName, SetAffect, RemoveAffixAffect, -1, null);
    }
}
