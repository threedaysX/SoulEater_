using UnityEngine;

/// <summary>
/// 詞綴
/// </summary>
/// 複製用: [CreateAssetMenu(menuName = "Endowment/Affix/對應詞綴名稱")]
public abstract class Affix : ScriptableObject
{
    [HideInInspector] public string affectName;
    public Character owner;
    public string description;

    protected bool eventSubscribedTrigger = false;

    public void OnStart()
    {
        eventSubscribedTrigger = false;
    }

    public void ResetAffectName(string fragName)
    {
        affectName = fragName + "_" + description;
    }

    /// <summary>
    /// 激活碎片時，觸發效果
    /// </summary>
    public void Trigger()
    {       
        InitAffix();
    }

    /// <summary>
    /// 拔除碎片時，觸發移除效果
    /// </summary>
    public void Remove()
    {
        if (affectName != string.Empty && affectName != null)
            owner.buffController.RemoveBuff(affectName);
    }

    protected Character GetAffectTarget()
    {
        return owner.combatController.lastAttackTarget;
    }

    /// <summary>
    /// 當詞綴被觸發時，觸發的效果內容
    /// (Ex: 能力增減、獲得技能、n%機率觸發、觸發效果並持續N秒，於效果結束後再觸發其他效果)
    /// </summary>
    protected abstract void SetAffect();
    protected abstract void RemoveAffixAffect();
    protected abstract void InitAffix();
}
