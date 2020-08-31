using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPutFrag : MonoBehaviour
{
    public GameObject leftUp;         //拉 左上那格
    public GameObject leftDown;       //拉 左下那格
    public Fragment fragSO;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            leftUp.GetComponent<Frag_m>().PutNewFrag(fragSO);
        if (Input.GetKeyDown(KeyCode.N))
            leftDown.GetComponent<Frag_m>().PutNewFrag(fragSO);

    }
}
