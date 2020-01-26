﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InputManager>();
            }
            return instance;
        }

    }
    #region navigation controller
    [SerializeField]
    private GameObject controllerParent;
    [SerializeField]
    private GameObject controllerCenter;
    [SerializeField]
    private GameObject controllerDirection;
    [SerializeField]
    private GameObject controllerExpansion;
    private Vector2? initTouch;
    private bool isEditor = false;
    private float controllerDirectionOriginalOpac;
    private Image controllerExpansionImage;
    private Image controllerDirectionImage;
    [SerializeField]
    private EventTrigger eventTrigger;
    #endregion

    #region accelerator
    [SerializeField]
    private GameObject accelCenter;
    [SerializeField]
    private GameObject accelExpansion;
    private Image accelExpansionImage;
    private float accelExpansionOriginalSize;
    private float accelExpansionDuration = 2f;
    private bool isHoldingAccel;
    #endregion

    #region player movement
    private Vector2 turn;
    public Vector2 Turn { get { return turn; } }
    private float speed;
    public float Speed { get { return speed; } }
    private float speedFactor = 2f;
    #endregion

    private float pressTime;
    [SerializeField]
    private float maxPressTime = 2f;
    [SerializeField]
    private float sensitivity = 1f;

    private void Awake()
    {
        instance = this;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        isEditor = true;
#else
        isEditor = false;
#endif
        controllerExpansionImage = controllerExpansion.GetComponent<Image>();
        controllerDirectionImage = controllerDirection.GetComponent<Image>();
        controllerDirectionOriginalOpac = controllerDirectionImage.color.a;
        accelExpansionImage = accelExpansion.GetComponent<Image>();
        accelExpansionOriginalSize = accelExpansion.transform.localScale.x;

        // register callbacks
        EventTrigger.Entry drag = new EventTrigger.Entry();
        drag.eventID = EventTriggerType.Drag;
        drag.callback.AddListener((data) => { Dragging((PointerEventData)data); });
        eventTrigger.triggers.Add(drag);

        EventTrigger.Entry beginDrag = new EventTrigger.Entry();
        beginDrag.eventID = EventTriggerType.BeginDrag;
        beginDrag.callback.AddListener((data) => { BeginDrag((PointerEventData)data); });
        eventTrigger.triggers.Add(beginDrag);

        EventTrigger.Entry endDrag = new EventTrigger.Entry();
        endDrag.eventID = EventTriggerType.EndDrag;
        endDrag.callback.AddListener((data) => { EndDrag((PointerEventData)data); });
        eventTrigger.triggers.Add(endDrag);
    }

    private void Update()
    {
        if (isEditor) AccelWithSpaceInputForDebugging();

        if (isHoldingAccel)
        {
            pressTime += Time.deltaTime;
            if (pressTime > maxPressTime) pressTime = maxPressTime;
        }
        else
        {
            pressTime -= Time.deltaTime;
            if (pressTime < 0) pressTime = 0;
        }

        speed = pressTime * speedFactor;
        float size = pressTime * accelExpansionOriginalSize / accelExpansionDuration;
        accelExpansion.transform.localScale = new Vector3(size, size, size);
    }

    public void HoldAccelerator()
    {
        isHoldingAccel = true;
    }

    public void ReleaseAccelerator()
    {
        isHoldingAccel = false;
    }

    public void BeginDrag(PointerEventData data)
    {
        initTouch = (isEditor)? new Vector2(Input.mousePosition.x, Input.mousePosition.y)
            : data.position;
        controllerParent.transform.position = initTouch.Value;
        controllerDirection.SetActive(true);
        controllerExpansion.SetActive(true);
    }

    public void EndDrag(PointerEventData data)
    {
        initTouch = null;
        controllerDirection.SetActive(false);
        controllerExpansion.SetActive(false);
        turn = Vector2.zero;
    }

    public void Dragging(PointerEventData data)
    {
        Vector2 dir = (isEditor) ?
            new Vector2(Input.mousePosition.x, Input.mousePosition.y) - initTouch.Value
            : data.position - initTouch.Value;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
        float length = dir.magnitude;
        turn = dir * sensitivity;
        controllerCenter.transform.localRotation = Quaternion.Euler(0, 0, angle);
        if (length < controllerCenter.transform.localScale.x)
        {
            controllerDirectionImage.color
                = new Color(1, 1, 1, length / controllerCenter.transform.localScale.x);
        }
        else
        {
            controllerDirectionImage.color
                = new Color(1, 1, 1, controllerDirectionOriginalOpac);
        }
    }

    private void AccelWithSpaceInputForDebugging()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isHoldingAccel) isHoldingAccel = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isHoldingAccel) isHoldingAccel = false;
        }
    }
}
