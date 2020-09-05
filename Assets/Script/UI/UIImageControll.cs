using UnityEngine.UI;

public class UIImageControll : Singleton<UIImageControll>
{
    public float SetImageFillAmount(Image image, float max, float current)
    {
        return (image.fillAmount = current / max);
    }

    public float SetImageFillAmount(Image image, Image fadeImage, float max, float current)
    {
        float resultPercentage = (image.fillAmount = current / max);
        Counter.Instance.StartCountDownInTimes(fadeImage.fillAmount, resultPercentage, 0.15f, false, (x) => fadeImage.fillAmount = x);
        return resultPercentage;
    }
}
