using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerZoneState
{
    Vein, OxygenArea, HeartArea
}

public partial class PlayerBehavior : MonoBehaviour
{
    private PlayerZoneState currZoneState;

    private void Awake()
    {
        InitMovement();
        InitOxygenBehavior();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateOxygenBehavior();
    }
}
