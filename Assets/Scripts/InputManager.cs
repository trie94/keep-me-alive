using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField]
    private GameObject controllerParent;
    [SerializeField]
    private GameObject controllerCenter;
    [SerializeField]
    private GameObject controllerDirection;
    [SerializeField]
    private GameObject controllerExpansion;
    private Vector3? initTouch;
    private bool isEditor = false;
    private float controllerDirectionOriginalOpac;
    private Image controllerExpansionImage;
    private Image controllerDirectionImage;

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
    }

    private void Update()
    {
        if (isEditor)   // editor test
        {
            if (Input.GetMouseButtonDown(0))    // init touch
            {
                initTouch = Input.mousePosition;
                controllerParent.transform.position = initTouch.Value;
                controllerDirection.SetActive(true);
                controllerExpansion.SetActive(true);
            }
            else if (Input.GetMouseButton(0))   // holding
            {
                Vector2 dir = Input.mousePosition - initTouch.Value;
                float angle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
                float length = dir.magnitude;
                controllerCenter.transform.localRotation = Quaternion.Euler(0, 0, angle);
                if (length < controllerCenter.transform.localScale.x)
                {
                    controllerDirectionImage.color = new Color(1, 1, 1, length / controllerCenter.transform.localScale.x);
                }
                else
                {
                    controllerDirectionImage.color = new Color(1, 1, 1, controllerDirectionOriginalOpac);
                }
            }
            else
            {
                initTouch = null;
                controllerDirection.SetActive(false);
                controllerExpansion.SetActive(false);
            }
        }
        else
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Canceled || touch.phase != TouchPhase.Ended)
                {
                    if (initTouch == null)
                    {
                        initTouch = touch.position;
                        controllerParent.transform.position = initTouch.Value;
                    }
                }
            }
            else
            {
                initTouch = null;
            }
        }
    }
}
