using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EndowmentInfoButtonControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject infoBackground;
    public Text infoTextArea;
    public CallAllTriggeredAffixs affixControl;

    public void OnPointerEnter(PointerEventData eventData)
    {
        infoBackground.SetActive(true);
        infoTextArea.text = affixControl.GetInfo();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoBackground.SetActive(false);
    }
}
