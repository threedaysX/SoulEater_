using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallAllTriggeredAffixs : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            for(int i=0;i< AllFragment.Instance.fragments.Count; i++)
            {
                Debug.Log(AllFragment.Instance.fragments[i].m_Data.PrintAndExeAffixs());
            }
        }
    }
}
