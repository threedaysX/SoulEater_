using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FragInfoControl : Singleton<FragInfoControl>
{
    public GameObject fragInfoBackground;
    public Image infoImageArea;
    public Text infoTextArea;

    public void ShowFragInfo(Sprite toShowFragIndexImage, string text)
    {
        fragInfoBackground.SetActive(true);
        infoTextArea.text = text;
        infoImageArea.sprite = toShowFragIndexImage;
    }
}
