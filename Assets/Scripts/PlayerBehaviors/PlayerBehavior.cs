using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerZoneState
{
    Vein, OxygenArea, HeartArea
}

public partial class PlayerBehavior : MonoBehaviour
{
    private static PlayerBehavior instance;
    public static PlayerBehavior Instance
    {
        get
        {
            return instance;
        }
    }

    private PlayerZoneState currZoneState;

    private void Awake()
    {
        instance = this;
        InitMovement();
        InitOxygenBehavior();
    }

    private void Update()
    {
        UpdateMovement();
    }
}
