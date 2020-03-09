using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenCarrierBehavior : MonoBehaviour
{
    private int oxygenNumber = 0;
    private int oxygenCapacity = 4;
    private Oxygen[] childOxygens;
    [SerializeField]
    private OxygenHolder[] oxygenHolders;

    private void Awake()
    {
        childOxygens = new Oxygen[oxygenCapacity];
        for (int i = 0; i < oxygenHolders.Length; i++)
        {
            oxygenHolders[i].cell = this.transform;
        }
    }

    public void GrabOxygen(Oxygen o)
    {
        Debug.Assert(CanGrabOxygen());
        for (int i=0; i<oxygenCapacity; i++)
        {
            if (childOxygens[i] == null)
            {
                childOxygens[i] = o;
                o.hopOnHolder = oxygenHolders[i];
                break;
            }
        }
        oxygenNumber++;
        o.carrier = this;
        o.state = OxygenState.HopOnCell;
    }

    public void ReleaseOxygen()
    {
        Debug.Assert(CanReleaseOxygen());
        Oxygen o = null;
        for (int i = 0; i < oxygenCapacity; i++)
        {
            var oxygen = childOxygens[i];
            if (oxygen != null)
            {
                o = oxygen;
                childOxygens[i] = null;
                break;
            }
        }
        oxygenNumber--;
        Debug.Assert(o != null);
        o.hopOnHolder.Reset();
        o.hopOnHolder = null;
        o.carrier = null;
        o.state = OxygenState.HeartArea;
    }

    public void AbandonOxygen(Oxygen o)
    {
        Debug.Assert(CanReleaseOxygen());
        for (int i = 0; i < oxygenCapacity; i++)
        {
            if (childOxygens[i] == o)
            {
                childOxygens[i] = null;
                break;
            }
        }
        oxygenNumber--;
        Debug.Assert(o != null);
        o.hopOnHolder.Reset();
        o.carrier = null;
        o.state = OxygenState.FallFromCell;
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

        for (int i = 0; i < oxygenHolders.Length; i++)
        {
            if (oxygenHolders[i].isOccupied == false)
            {
                isReadyToGo = false;
            }
        }

        return isReadyToGo;
    }
}
