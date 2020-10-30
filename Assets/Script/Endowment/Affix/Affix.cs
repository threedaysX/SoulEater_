using UnityEngine;

/// <summary>
/// 詞綴
/// </summary>
/// 複製用: [CreateAssetMenu(menuName = "Endowment/Affix/對應詞綴名稱")]
public abstract class Affix : ScriptableObject
{
    public Character owner;
    public string description;
    protected string affectName;
    private bool initNameTrigger;

    /// <summary>
    /// 激活碎片時，觸發效果
    /// </summary>
    public void Trigger()
    {
        // Warning: need to avoid duplicate with another affix(Cannot exist same name buff).
        if (!initNameTrigger)
        {
            initNameTrigger = true;
            affectName = "Frag_" + Time.time + "." + System.DateTime.UtcNow.ToString();
        }
        InitAffix();
    }

    /// <summary>
    /// 拔除碎片時，觸發移除效果
    /// </summary>
    public void Remove()
    {
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
