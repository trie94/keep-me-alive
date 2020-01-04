using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    private static UIController instance;
    public static UIController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIController>();
            }
            return instance;
        }
    }
    
    [SerializeField]
    private TextMeshProUGUI debugText;

    public void SetDebugText(string text)
    {
        debugText.SetText(text);
    }
}
