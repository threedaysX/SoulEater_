using UnityEngine;

namespace HurlGame.UIComponents
{
    [CreateAssetMenu(fileName = "ButtonEffectSetting", menuName = "UI/Setting/ButtonEffect", order = 0)]
    public class ButtonEffectSetting : ScriptableObject 
    {
        public float hoverAlpha = 0.8f;
        public float hoverScale = 1.05f;
        public float transitionDuration = 0.3f;
    }
}