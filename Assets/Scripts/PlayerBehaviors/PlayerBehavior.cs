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
    public OxygenCarrierBehavior carrier;

    private void Awake()
    {
        instance = this;
        carrier = GetComponent<OxygenCarrierBehavior>();
        InitMovement();
    }

    private void Update()
    {
        UpdateMovement();
    }
}
