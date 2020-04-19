using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeCarrierBehavior : MonoBehaviour
{
    private int moleculeNumber = 0;
    private int moleculeCapacity = 4;
    private Molecule[] childMolecules;
    [SerializeField]
    private MoleculeHolder[] holders;

    private void Awake()
    {
        childMolecules = new Molecule[moleculeCapacity];
        for (int i = 0; i < holders.Length; i++)
        {
            holders[i].cell = this.transform;
        }
    }

    public void GrabOxygen(Oxygen o)
    {
        Debug.Assert(CanGrab());
        for (int i=0; i<moleculeCapacity; i++)
        {
            if (childMolecules[i] == null)
            {
                childMolecules[i] = o;
                o.hopOnHolder = holders[i];
                break;
            }
        }
        moleculeNumber++;
        o.carrier = this;
        o.state = MoleculeState.HopOnCell;
    }

    public void ReleaseOxygen(BodyTissue bodyTissue)
    {
        Oxygen o = null;
        for (int i = 0; i < moleculeCapacity; i++)
        {
            var molecule = childMolecules[i];
            if (molecule != null && molecule is Oxygen)
            {
                o = (Oxygen)molecule;
                childMolecules[i] = null;
                break;
            }
        }
        moleculeNumber--;
        Debug.Assert(o != null);
        o.hopOnHolder.Reset();
        o.hopOnHolder = null;
        o.carrier = null;
        o.targetBodyTissue = bodyTissue;
        bodyTissue.ReceiveOxygen();
        o.state = MoleculeState.BodyTissueArea;
    }

    public void AbandonOxygen(Oxygen o)
    {
        Debug.Assert(CanReleaseOxygen());
        for (int i = 0; i < moleculeCapacity; i++)
        {
            if (childMolecules[i] == o)
            {
                childMolecules[i] = null;
                break;
            }
        }
        moleculeNumber--;
        Debug.Assert(o != null);
        o.hopOnHolder.Reset();
        o.carrier = null;
        o.state = MoleculeState.FallFromCell;
    }

    public void GrabOxygens()
    {
        for (int i = 0; i < moleculeCapacity; i++)
        {
            Oxygen oxygen = OxygenController.Instance.oxygens.Pop();
            GrabOxygen(oxygen);
        }
    }

    public bool CanGrab()
    {
        return moleculeNumber < moleculeCapacity;
    }

    public bool CanReleaseOxygen()
    {
        return moleculeNumber > 0;
    }

    public bool IsReadyToGo()
    {
        bool isReadyToGo = true;

        for (int i = 0; i < holders.Length; i++)
        {
            if (holders[i].isOccupied == false)
            {
                isReadyToGo = false;
            }
        }

        return isReadyToGo;
    }
}
