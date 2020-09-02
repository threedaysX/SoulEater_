using System.Collections.Concurrent;
using UnityEngine;

public class CumulativeDataController : MonoBehaviour
{
    public const string Cumulative_DamageTake_KnockBack = "Cumulative_DamageTake_KnockBack";    // 累積受到的擊退傷害
    public const string Cumulative_DamageDealt_ManaAbsorb = "Cumulative_DamageDealt_ManaAbsorb";   // 累積造成的吸魔傷害

    private CumulativeData<int> damageTakeObservers;
    private CumulativeData<int> damageDealtObservers;
    private CumulativeData<int> attackHitTimesObservers;

    // Start is called before the first frame update
    private void Start()
    {
        damageTakeObservers = new CumulativeData<int>();
        damageDealtObservers = new CumulativeData<int>();
        attackHitTimesObservers = new CumulativeData<int>();
        AddData(Cumulative_DamageTake_KnockBack, CumulativeDataType.Take, 0);
        AddData(Cumulative_DamageDealt_ManaAbsorb, CumulativeDataType.Dealt, 0);
    }

    public void DataStore(CumulativeDataType type, int add)
    {
        var datas = GetCumulativeDataDict(type);
        foreach (var item in datas)
        {
            datas[item.Key] += add;
        }
    }

    public int GetData(string name, CumulativeDataType type)
    {
        var datas = GetCumulativeDataDict(type);
        if (datas != null && datas.TryGetValue(name, out int resultData))
        {
            return resultData;
        }
        return 0;
    }

    public void ModifyData(string name, CumulativeDataType type, int newData)
    {
        GetCumulativeDataDict(type)[name] = newData;
    }

    public void AddData(string name, CumulativeDataType type, int defaultValue)
    {
        var datas = GetCumulativeDataDict(type);
        if (!datas.ContainsKey(name))
            datas.TryAdd(name, defaultValue);
    }

    public void RemoveData(string name, CumulativeDataType type)
    {
        var datas = GetCumulativeDataDict(type);
        if (datas.ContainsKey(name))
            datas.TryRemove(name, out _);
    }

    private CumulativeData<int> GetCumulativeDataDict(CumulativeDataType type)
    {
        switch (type)
        {
            case CumulativeDataType.Take:
                return damageTakeObservers;
            case CumulativeDataType.Dealt:
                return damageDealtObservers;
            case CumulativeDataType.HitTimes:
                return attackHitTimesObservers;
            default:
                break;
        }
        return null;
    }

    public class CumulativeData<T> : ConcurrentDictionary<string, T> { }
}

public struct DamageStoreData 
{
    public CumulativeDataType type;
    public int value;
}


public enum CumulativeDataType
{
    Take,
    Dealt,
    HitTimes,
}
