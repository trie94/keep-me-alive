using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeCarrierBehavior : MonoBehaviour
{
    private int oxygenNumber = 0;
    private int oxygenCapacity = 4;
    private Molecule[] childMolecules;
    [SerializeField]
    private MoleculeHolder[] holders;

    private void Awake()
    {
        childMolecules = new Molecule[oxygenCapacity];
        for (int i = 0; i < holders.Length; i++)
        {
            holders[i].cell = this.transform;
        }
    }

    public void GrabOxygen(Molecule o)
    {
        Debug.Assert(CanGrabOxygen());
        for (int i=0; i<oxygenCapacity; i++)
        {
            if (childMolecules[i] == null)
            {
                childMolecules[i] = o;
                o.hopOnHolder = holders[i];
                break;
            }
        }
        oxygenNumber++;
        o.carrier = this;
        o.state = MoleculeState.HopOnCell;
    }

    public void ReleaseOxygen(BodyTissue bodyTissue)
    {
        Molecule o = null;
        for (int i = 0; i < oxygenCapacity; i++)
        {
            var molecule = childMolecules[i];
            if (molecule != null)
            {
                o = molecule;
                childMolecules[i] = null;
                break;
            }
        }
        oxygenNumber--;
        Debug.Assert(o != null);
        o.hopOnHolder.Reset();
        o.hopOnHolder = null;
        o.carrier = null;
        // TODO - clean up
        if (o is Oxygen)
        {
            ((Oxygen)o).targetBodyTissue = bodyTissue;
            bodyTissue.ReceiveOxygen();
        }
        o.state = MoleculeState.BodyTissueArea;
    }

    public void AbandonOxygen(Oxygen o)
    {
        Debug.Assert(CanReleaseOxygen());
        for (int i = 0; i < oxygenCapacity; i++)
        {
            if (childMolecules[i] == o)
            {
                childMolecules[i] = null;
                break;
            }
        }
        oxygenNumber--;
        Debug.Assert(o != null);
        o.hopOnHolder.Reset();
        o.carrier = null;
        o.state = MoleculeState.FallFromCell;
    }

    public void GrabOxygens()
    {
        for (int i = 0; i < oxygenCapacity; i++)
        {
            Oxygen oxygen = OxygenController.Instance.oxygens.Pop();
            GrabOxygen(oxygen);
        }
    }

    public bool CanGrabOxygen()
    {
        return oxygenNumber < oxygenCapacity;
    }

    public bool CanReleaseOxygen()
    {
        return oxygenNumber > 0;
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
