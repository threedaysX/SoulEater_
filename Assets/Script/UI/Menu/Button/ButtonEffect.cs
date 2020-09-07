using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

namespace HurlGame.UIComponents
{
    public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public ButtonEffectSetting setting;
        public Text text;

        public void OnPointerEnter(PointerEventData eventData)
        {
            text.DOFade(setting.hoverAlpha, setting.transitionDuration).SetUpdate(true);
            transform.DOScale(setting.hoverScale, setting.transitionDuration).SetUpdate(true);
        }   

        public void OnPointerExit(PointerEventData eventData)
        {
            text.DOFade(1f, setting.transitionDuration).SetUpdate(true);
            transform.DOScale(1f, setting.transitionDuration).SetUpdate(true);
        }
    }
}
