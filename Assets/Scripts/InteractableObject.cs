using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/**
 * This script should be attached to any interactable object. This is only
 * responsible for showing and hiding ui indicator; providing a function where
 * you can register callbacks for button events.
 * This script is only responsible for what to conduct based on the boolean.
 * There should be another script that is responsible for setting the boolean,
 * isInteractable. Callback registeration should happen in Start, not Awake.
 */
public class InteractableObject : MonoBehaviour
{
    [SerializeField]
    private GameObject hoveringUIPrefab;
    private GameObject hoveringUI;

    private Camera mainCam;

    public delegate void PointerEventDelegate(PointerEventData data);
    public PointerEventDelegate pointerEventDelegate;
    private EventTrigger eventTrigger;

    private bool isInteractable = false;
    public bool IsInteractable { get { return isInteractable; } set { isInteractable = value; } }

    private void Awake()
    {
        mainCam = Camera.main;
        hoveringUI = Instantiate(hoveringUIPrefab, InputManager.Instance.canvas.transform);
        eventTrigger = hoveringUI.GetComponentInChildren<EventTrigger>();
        hoveringUI.SetActive(false);
    }

    private void Update()
    {
        if (PlayerBehavior.Instance == null) return;

        if (isInteractable)
        {
            hoveringUI.SetActive(true);
            Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position);
            hoveringUI.transform.position = screenPos;
            isInteractable = true;
        }
        else
        {
            hoveringUI.SetActive(false);
            isInteractable = false;
        }
    }

    public void RegisterCallback(EventTriggerType eventTriggerType, PointerEventDelegate callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventTriggerType;
        entry.callback.AddListener((data) => { callback((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }
}
