using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallAllTriggeredAffixs : Singleton<CallAllTriggeredAffixs>
{

    public void _CallAllTriggeredAffixs()
    {
        for (int i = 0; i < AllFragment.Instance.fragments.Count; i++)
        {
            F_Data fragData = AllFragment.Instance.fragments[i].m_Data;
            Debug.Log(fragData.PrintAndExeAffixs(fragData.fName));
        }
    }
}
