public abstract class ConditionTriggerAffix : Affix
{
    protected override void InitAffix()
    {
        // duration: -1, 代表永久
        owner.buffController.AddBuff(affectName, SetAffect, RemoveAffixAffect, -1, delegate { return GetAffectCondition(); });
    }

    /// <summary>
    /// 觸發的條件
    /// </summary>
    protected abstract bool GetAffectCondition();
}
