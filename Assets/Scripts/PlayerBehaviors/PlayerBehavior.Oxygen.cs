using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerBehavior : MonoBehaviour
{
    [SerializeField]
    private int oxygenCapacity;
    private Stack<Oxygen> childOxygen;
    [SerializeField]
    private OxygenHolder[] oxygenHolders;

    private void InitOxygenBehavior()
    {
        childOxygen = new Stack<Oxygen>();
    }

    public void GrabOxygen(Oxygen o)
    {
        if (childOxygen.Count >= oxygenCapacity) return;
        Debug.Log("grab oxygen!");
        var holder = oxygenHolders[childOxygen.Count];
        o.hopOnHolder = holder;
        childOxygen.Push(o);
        o.transform.parent = holder.transform;
        o.playerMaster = this;
        o.state = OxygenState.HopOnCell;
    }

    public void ReleaseOxygen()
    {
        if (childOxygen.Count <= 0) return;
        Debug.Log("release oxygen!");
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

    public void AbandonOxygen(Oxygen o)
    {
        // TODO: this happens when the player moves to fast and lose oxyge
        // that goes beyond the detach threashold
    }

    public void ShootOxygen()
    {

    }

    public bool CanGrabOxygen()
    {
        return childOxygen.Count < oxygenCapacity;
    }

    public bool CanReleaseOxygen()
    {
        return childOxygen.Count > 0;
    }
}
