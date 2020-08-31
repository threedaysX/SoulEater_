using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Frag_m : MonoBehaviour, IPointerDownHandler
{
    public Fragment putFrag;
    int fId=-1;

    //要新增碎片的話，資料放這裡
    public void PutNewFrag(Fragment fragSO)
    {
        putFrag = fragSO;
        GetComponent<Image>().sprite = fragSO.m_Data.fragImage;
        GetComponent<Image>().color = new Color(1, 1, 1, 1);
    }

    public void Clean()
    {
        GetComponent<Image>().color = new Color(1, 1, 1, 0);
        GetComponent<Image>().sprite = null;
        putFrag = null;
        fId = -1;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (putFrag == null) return;

        AllFragment.Instance.fragments.Add(ScriptableObject.CreateInstance("Fragment") as Fragment);
        int id = AllFragment.Instance.fragments.Count - 1;
        fId = id;
        AllFragment.Instance.fragments[id].m_Data = new F_Data();
        CurrentData.Instance.currentFragmentID = id;
        AllFragment.Instance.fragments[id].m_Data.init(putFrag.m_Data.touchPos_v2, putFrag.m_Data.neighborRelativeInfo, id, putFrag.m_Data.fragImage);
        AllFragment.Instance.fragments[id].m_Data.fragColor= putFrag.m_Data.fragColor;

        //this.GetComponent<Image>().color = new Color(1, 1, 1, 0.6f);
        if (CurrentData.Instance.chip2Mouse.GetComponent<Image>() != null)
        {
            CurrentData.Instance.chip2Mouse.gameObject.SetActive(true);
            CurrentData.Instance.chip2Mouse.GetComponent<Image>().sprite = putFrag.m_Data.fragImage;
        }
        CurrentData.Instance.f_m = this;
        GetComponent<Image>().color=new Color(1, 1, 1, 0.2f);
    }

}