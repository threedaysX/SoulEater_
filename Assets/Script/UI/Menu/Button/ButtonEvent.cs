﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonEvent : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Key Control")]
    public bool canPressAnyKey;
    public KeyCode clickKey;
    public ClickKeyEvent[] otherClickEvents;

    [Header("Select Control")]
    public bool interactableWhenMouseOver;
    private bool originInteractableState;
    public GameObject pointer;
    public RectTransform targetPos;
    public UnityEvent onSelectEvent;
    public UnityEvent onDeselectEvent;

    private Button _this;

    private void Start()
    {
        _this = GetComponent<Button>();
        originInteractableState = _this.interactable;
    }

    /// <summary>
    /// Click this button.
    /// </summary>
    public void Click()
    {
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(gameObject, pointer, ExecuteEvents.submitHandler);
    }

    public void OnSelect(BaseEventData e)
    {
        if (pointer != null && !pointer.activeSelf)
        {
            pointer.SetActive(true);
            return;
        }
        if (targetPos != null)
        {
            pointer.transform.position = targetPos.position;
        }
        ButtonEvents.Instance.selectedButton = _this;
        AudioControl.Instance.PlaySound(ButtonEvents.Instance.selectButtonSound);
        onSelectEvent.Invoke();
    }

    public void OnDeselect(BaseEventData e)
    {
        onDeselectEvent.Invoke();
        ButtonEvents.Instance.DeselectButton();
    }

    /// <summary>
    /// Reset Button State.
    /// </summary>
    public void OnPointerEnter(PointerEventData e)
    {
        if (interactableWhenMouseOver)
        {
            originInteractableState = _this.interactable;
            _this.interactable = true;
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (interactableWhenMouseOver)
        {
            _this.interactable = originInteractableState;
        }
    }
}

[System.Serializable]
public struct ClickKeyEvent
{
    public bool allowKeyHeldToTriggerEvent;
    public KeyCode clickKey;
    public UnityEvent triggerEvent;
}
