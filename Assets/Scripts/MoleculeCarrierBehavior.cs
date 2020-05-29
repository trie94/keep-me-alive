using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeCarrierBehavior : MonoBehaviour
{
    private int moleculeNumber = 0;
    private int moleculeCapacity = 4;
    [SerializeField]
    private MoleculeHolder[] holders;
    public MoleculeHolder[] Holders { get { return holders; } }
    private bool isPlayer = false;
    public bool IsPlayer { get { return isPlayer; } }
    private float grabDist = 2f;

    private void Awake()
    {
        for (int i = 0; i < holders.Length; i++)
        {
            holders[i].cell = this.transform;
        }
        moleculeCapacity = holders.Length;
        // only player can abandon oxygen
        if (GetComponent<PlayerBehavior>())
        {
            isPlayer = true;
        }
    }

    private void Update()
    {
        if (!CellController.Instance.IsInBodyTissueArea(this.transform) && hasVacancy())
        {
            FindClosestOxygen();
        }
    }

    public bool hasVacancy()
    {
        return moleculeNumber < moleculeCapacity;
    }

    public bool CanReleaseOxygen()
    {
        return moleculeNumber > 0;
    }

    private void FindClosestOxygen()
    {
        Oxygen closest = null;
        float min = float.MaxValue;
        for (int i = 0; i < OxygenController.Instance.oxygenList.Count; i++)
        {
            Oxygen curr = OxygenController.Instance.oxygenList[i];
            if (curr.carrier != null) continue;
            float distSqr = (curr.transform.position - transform.position).sqrMagnitude;
            if (distSqr < min)
            {
                closest = curr;
                min = distSqr;
            }
        }

        if (closest != null && min < grabDist * grabDist)
        {
            GrabOxygen(closest);
        }
    }

    private void GrabOxygen(Oxygen o)
    {
        o.state = MoleculeState.HopOnCell;
        o.carrier = this;
        for (int i = 0; i < moleculeCapacity; i++)
        {
            var curr = holders[i];
            if (!curr.isOccupied)
            {
                o.hopOnHolder = curr;
                curr.Occupy();
                break;
            }
        }
        moleculeNumber++;
    }

    public void ReleaseOxygen(Oxygen o, BodyTissue bodyTissue)
    {
        o.state = MoleculeState.Released;
        o.targetBodyTissue = bodyTissue;
        moleculeNumber--;
        o.carrier = null;
        o.hopOnHolder.Reset();
        o.hopOnHolder = null;
    }

    public void AbandonOxygen(Oxygen o)
    {
        o.state = MoleculeState.FallFromCell;
        moleculeNumber--;
        o.hopOnHolder.Reset();
        o.carrier = null;
    }
}
