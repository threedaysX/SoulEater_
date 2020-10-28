using UnityEngine;
using UnityEngine.UI;

public class EnemyUIControl : Singleton<EnemyUIControl>
{
    public GameObject UIObject;

    [Header("Hp")]
    public Image health;
    public Image healthWhite;

    [Header("Name")]
    public Text enemyName;
    public Text enemyNameShadow;

    public void SetHealthUI(string enemyName, float percentage)
    {
        if (this.enemyName == null || enemyNameShadow == null)
            return;

        this.enemyName.text = enemyName;
        this.enemyNameShadow.text = enemyName;
        UIImageControll.Instance.SetImageFillAmount(health, healthWhite, percentage);

        if (percentage <= 0)
        {
            UIObject.SetActive(false);
        }
        else
        {
            UIObject.SetActive(true);
        }
    }
}
