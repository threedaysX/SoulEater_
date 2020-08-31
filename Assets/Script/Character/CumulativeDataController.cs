using System.Collections.Concurrent;
using UnityEngine;

public class CumulativeDataController : MonoBehaviour
{
    public const string Cumulative_DamageTake_KnockBack = "Cumulative_DamageTake_KnockBack";    // 累積受到的擊退傷害
    public const string Cumulative_DamageDealt_ManaAbsorb = "Cumulative_DamageDealt_ManaAbsorb";   // 累積造成的吸魔傷害

    private ConcurrentDictionary<string, int> damageTakeStoreDatas;
    private ConcurrentDictionary<string, int> damageDealtStoreDatas;
    private ConcurrentDictionary<string, int> attackHitTimesStoreDatas;

    // Start is called before the first frame update
    private void Start()
    {
        damageTakeStoreDatas = new ConcurrentDictionary<string, int>();
        damageDealtStoreDatas = new ConcurrentDictionary<string, int>();
        attackHitTimesStoreDatas = new ConcurrentDictionary<string, int>();
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

    private ConcurrentDictionary<string, int> GetCumulativeDataDict(CumulativeDataType type)
    {
        switch (type)
        {
            case CumulativeDataType.Take:
                return damageTakeStoreDatas;
            case CumulativeDataType.Dealt:
                return damageDealtStoreDatas;
            case CumulativeDataType.HitTimes:
                return attackHitTimesStoreDatas;
            default:
                break;
        }
        return null;
    }
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
