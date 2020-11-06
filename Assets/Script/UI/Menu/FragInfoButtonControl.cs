using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FragInfoButtonControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite toShowFragIndexImage;
    public Frag_m self;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (self != null)
        {
            FragInfoControl.Instance.ShowFragInfo(toShowFragIndexImage, self.putFrag.m_Data.GetFragAllAffixsInfo());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FragInfoControl.Instance.fragInfoBackground.SetActive(false);
    }   
}
