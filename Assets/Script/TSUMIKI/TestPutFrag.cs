using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPutFrag : MonoBehaviour
{
    public Fragment fragSO;     //得到碎片的 Scriptable object

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            PutFrag.Instance.PutFragment(fragSO, FragSpot.left_Up); //把這個碎片放在左上位置
        if (Input.GetKeyDown(KeyCode.N))
            PutFrag.Instance.PutFragment(fragSO,FragSpot.right_Up); //把這個碎片放在右上位置
    }
}
