using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonEvents : Singleton<ButtonEvents>
{
    public Button selectedButton;
    public AudioClip selectButtonSound;
    private bool deselectButtonTrigger;

    [Header("Pressing Button (Key Held Down)")]
    public float UpperPressTimeRange = 1f;
    public float pressingTime = 0f;

    public void Update()
    {
        // If lost select and not in control with slider, 
        // then click any key to re-select last button (except mouse click).
        if (deselectButtonTrigger && GetAnyKeyDown(false))
        {
            selectedButton?.Select();
            deselectButtonTrigger = false;
        }

        if (selectedButton != null)
        {
            var btnEvent = selectedButton.GetComponent<ButtonEvent>();
            if (btnEvent == null)
                return;

            if (btnEvent.canPressAnyKey && GetAnyKeyDown(true))
            {
                selectedButton.onClick.Invoke();
            }
            else if (btnEvent.otherClickEvents.Length == 0 && Input.GetKeyDown(btnEvent.clickKey))
            {
                btnEvent.Click();
            }
            else if (btnEvent.otherClickEvents.Length > 0)
            {
                foreach (var item in btnEvent.otherClickEvents)
                {
                    if ((!item.allowKeyHeldToTriggerEvent && Input.GetKeyDown(item.clickKey))
                        || (item.allowKeyHeldToTriggerEvent && Input.GetKey(item.clickKey)))
                    {
                        item.triggerEvent.Invoke();
                    }
                }
            }
        }
    }

    private bool GetAnyKeyDown(bool containsMouseClick)
    {
        if (containsMouseClick)
        {
            return Input.anyKeyDown;
        }
        else
        {
            return Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2);
        }
    }

    public void SelectButton(Button button)
    {
        selectedButton = button;
    }

    public void DeselectButton()
    {
        if (!deselectButtonTrigger)
            deselectButtonTrigger = true;
    }

    public void LoadScene(string sceneName)
    {
        SceneController.Instance.LoadScene(sceneName);
    }

    public void DelayLoadScene(string sceneName)
    {
        SceneController.Instance.DelayLoadScene(sceneName, 2.5f);
    }

    public void QuitGameImmediately()
    {
        AppControl.Instance.QuitGameImmediately();
    }

    public void QuitGame()
    {
        AppControl.Instance.QuitGame(1f);
    }
}
