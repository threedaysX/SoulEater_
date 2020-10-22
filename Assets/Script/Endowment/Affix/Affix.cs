using UnityEngine;

/// <summary>
/// 詞綴
/// </summary>
/// 複製用: [CreateAssetMenu(menuName = "Endowment/Affix/對應詞綴名稱")]
public abstract class Affix : ScriptableObject
{
    public Character character; // 誰擁有這個詞綴效果
    public string description;
    public string affectName;

    /// <summary>
    /// 激活碎片時，觸發效果
    /// </summary>
    public void Trigger()
    {
        AddBuff();
    }

    /// <summary>
    /// 拔除碎片時，觸發移除效果
    /// </summary>
    public void Remove()
    {
        character.buffController.RemoveBuff(affectName);
    }

    /// <summary>
    /// 當詞綴被觸發時，觸發的效果內容
    /// (Ex: 能力增減、獲得技能、n%機率觸發、觸發效果並持續N秒，於效果結束後再觸發其他效果)
    /// </summary>
    protected abstract void SetAffect();
    protected abstract void RemoveAffixAffect();
    protected abstract void AddBuff();
}

public abstract class NormalAffix : Affix
{
    protected override void AddBuff()
    {
        // duration: -1, 代表永久
        character.buffController.AddBuff(affectName, SetAffect, RemoveAffixAffect, -1, null);
    }
}

public abstract class ConditionAffix : Affix
{
    protected override void AddBuff()
    {
        // duration: -1, 代表永久
        character.buffController.AddBuff(affectName, SetAffect, RemoveAffixAffect, -1, delegate { return GetAffectCondition(); });
    }

    protected abstract bool GetAffectCondition();
}