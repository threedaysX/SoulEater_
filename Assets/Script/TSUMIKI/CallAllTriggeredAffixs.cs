using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallAllTriggeredAffixs : Singleton<CallAllTriggeredAffixs>
{
    public List<Fragment> frags;

    private void Start()
    {
        foreach (var frag in frags)
        {
            frag.m_Data.ResetAffixOnStart();
        }
    }

    public void _CallAllTriggeredAffixs()
    {
        info = string.Empty;
        for (int i = 0; i < AllFragment.Instance.fragments.Count; i++)
        {
            F_Data fragData = AllFragment.Instance.fragments[i].m_Data;
            string affixInfo = fragData.PrintAndExeAffixs(fragData.fName);
            info += "\n\n" + affixInfo;
            Debug.Log(affixInfo);
        }
    }

    private string info;

    public string GetInfo()
    {
        return info;
    }
}
