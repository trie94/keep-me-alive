﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerBehavior : MonoBehaviour
{
    [SerializeField]
    private int oxygenCapacity;
    private Stack<Oxygen> childOxygen;
    [SerializeField]
    private OxygenHolder[] oxygenHolders;
    [SerializeField]
    private float oxygenReleaseInterval = 1f;
    [SerializeField]
    public float oxygenReleaseTick = 0f;

    private void InitOxygenBehavior()
    {
        childOxygen = new Stack<Oxygen>();
    }

    private void UpdateOxygenBehavior()
    {
        if (currZoneState == PlayerZoneState.Vein)
        {

        }
        else if (currZoneState == PlayerZoneState.OxygenArea)
        {
            // check nearby oxygens
            if (childOxygen.Count < oxygenCapacity)
            {
                for (int i = 0; i < oxygenCapacity; i++)
                {
                    Oxygen oxygen = OxygenController.Instance.oxygens.Pop();
                    GrabOxygen(oxygen);
                }
            }
        }
        else if (currZoneState == PlayerZoneState.HeartArea)
        {
            if (Vector3.SqrMagnitude(CellController.Instance.heart.position - transform.position) < 1f)
            {
                if (childOxygen.Count <= 0)
                {
                    oxygenReleaseTick = 0f;
                }
                else if (oxygenReleaseTick >= oxygenReleaseInterval)
                {
                    ReleaseOxygen();
                    oxygenReleaseTick = 0f;
                }
                else
                {
                    oxygenReleaseTick += Time.deltaTime;
                }
            }
        }
    }

    private void GrabOxygen(Oxygen o)
    {
        Debug.Assert(childOxygen.Count < oxygenCapacity);
        var holder = oxygenHolders[childOxygen.Count];
        o.hopOnHolder = holder;
        childOxygen.Push(o);
        o.transform.parent = holder.transform;
        o.playerMaster = this;
        o.state = OxygenState.HopOnCell;
    }

    private void ReleaseOxygen()
    {
        Debug.Assert(childOxygen.Count > 0);
        Oxygen o = childOxygen.Pop();
        o.hopOnHolder = null;
        o.transform.parent = null;
        o.master = null;
        for (int i = 0; i < oxygenHolders.Length; i++)
        {
            oxygenHolders[i].Reset();
        }
        o.state = OxygenState.HeartArea;
    }

    private void ShootOxygen()
    {

    }
}
