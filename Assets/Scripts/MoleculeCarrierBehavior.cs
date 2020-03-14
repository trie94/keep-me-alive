using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleculeCarrierBehavior : MonoBehaviour
{
    private int oxygenNumber = 0;
    private int oxygenCapacity = 4;
    private Molecule[] childMoledules;
    [SerializeField]
    private MoleculeHolder[] holders;

    private void Awake()
    {
        childMoledules = new Molecule[oxygenCapacity];
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
            if (childMoledules[i] == null)
            {
                childMoledules[i] = o;
                o.hopOnHolder = holders[i];
                break;
            }
        }
        oxygenNumber++;
        o.carrier = this;
        o.state = MoleculeState.HopOnCell;
    }

    public void ReleaseOxygen()
    {
        Debug.Assert(CanReleaseOxygen());
        Molecule o = null;
        for (int i = 0; i < oxygenCapacity; i++)
        {
            var oxygen = childMoledules[i];
            if (oxygen != null)
            {
                o = oxygen;
                childMoledules[i] = null;
                break;
            }
        }
        oxygenNumber--;
        Debug.Assert(o != null);
        o.hopOnHolder.Reset();
        o.hopOnHolder = null;
        o.carrier = null;
        o.state = MoleculeState.HeartArea;
    }

    public void AbandonOxygen(Oxygen o)
    {
        Debug.Assert(CanReleaseOxygen());
        for (int i = 0; i < oxygenCapacity; i++)
        {
            if (childMoledules[i] == o)
            {
                childMoledules[i] = null;
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
