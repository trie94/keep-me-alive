using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ErythrocyteState
{
    InVein, EnterOxygenArea, WaitOxygen, ExitOxygenArea,
    EnterHeartArea, ReleaseOxygen, ExitHeartArea
}

public class Erythrocyte : Cell
{
    public int oxygenCapacity = 3;
    public float oxygenReleaseInterval = 2f;
    public float oxygenReleaseTick = 0f;
    public Stack<Oxygen> childOxygen;
    public OxygenHolder[] oxygenHolders;

    public override void Start()
    {
        base.Start();
        UpdateCellState();
        childOxygen = new Stack<Oxygen>();
    }

    public override void Update()
    {
        List<Transform> neighbors = GetNeighbors();
        Vector3 velocity = behaviors[(int)cellState].CalculateVelocity(this, neighbors);
        velocity *= velocityMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        Move(velocity);

        if (cellState == ErythrocyteState.InVein)
        {
            
        }
        else if (cellState == ErythrocyteState.EnterOxygenArea)
        {
            if (Vector3.SqrMagnitude(OxygenController.Instance.oxygenArea.position - transform.position) < 0.5f)
            {
                cellState = ErythrocyteState.WaitOxygen;
            }
        }
        else if (cellState == ErythrocyteState.WaitOxygen)
        {
            if (childOxygen.Count < oxygenCapacity)
            {
                Oxygen closest = null;
                for (int i = 0; i < OxygenController.Instance.oxygens.Count; i++)
                {
                    var curr = OxygenController.Instance.oxygens[i];

                    if (closest == null)
                    {
                        closest = curr;
                        continue;
                    }

                    if (childOxygen.Contains(curr) || curr.master != null) continue;

                    if (Vector3.SqrMagnitude(curr.transform.position - transform.position)
                        < Vector3.SqrMagnitude(closest.transform.position - transform.position))
                    {
                        closest = curr;
                    }
                }

                RegisterOxygen(closest);
            }
            else
            {
                bool isReadyToGo = true;
                for (int i = 0; i < oxygenHolders.Length; i++)
                {
                    if (oxygenHolders[i].isOccupied == false)
                    {
                        isReadyToGo = false;
                    }
                }

                if (isReadyToGo) cellState = ErythrocyteState.ExitOxygenArea;
            }
        }
        else if (cellState == ErythrocyteState.ExitOxygenArea)
        {
            if (Vector3.SqrMagnitude(CellController.Instance.oxygenExitNode.position - transform.position) < 0.5f)
            {
                cellState = ErythrocyteState.InVein;
            }
        }
        else if (cellState == ErythrocyteState.EnterHeartArea)
        {
            if (childOxygen.Count <= 0)
            {
                cellState = ErythrocyteState.ExitHeartArea;
            }
            else if (Vector3.SqrMagnitude(CellController.Instance.heart.position - transform.position) < 0.7f)
            {
                cellState = ErythrocyteState.ReleaseOxygen;
            }
        }
        else if (cellState == ErythrocyteState.ReleaseOxygen)
        {
            if (childOxygen.Count <= 0)
            {
                cellState = ErythrocyteState.ExitHeartArea;
                oxygenReleaseTick = 0f;
            }

            if (oxygenReleaseTick >= oxygenReleaseInterval)
            {
                ReleaseOxygen();
                oxygenReleaseTick = 0f;
            }
            oxygenReleaseTick += Time.deltaTime;
        }
        else if (cellState == ErythrocyteState.ExitHeartArea)
        {
            if (Vector3.SqrMagnitude(CellController.Instance.heardExitNode.position - transform.position) < 0.5f)
            {
                cellState = ErythrocyteState.InVein;
            }
        }

        // emotion
        if (pickTick > emotionPickInterval)
        {
            PickNextEmotionAndReset();
        }
        pickTick += Time.deltaTime;
        PlayFaceAnim();
    }

    // check the current node
    public override void UpdateCellState()
    {
        var nodeType = currSeg.n0.type;
        switch (nodeType)
        {
            case NodeType.HeartEntrance:
                cellState = ErythrocyteState.EnterHeartArea;
                break;
            case NodeType.OxygenEntrance:
                cellState = ErythrocyteState.EnterOxygenArea;
                break;
            default:
                cellState = ErythrocyteState.InVein;
                break;
        }
    }

    public void RegisterOxygen(Oxygen o)
    {
        if (childOxygen.Count >= oxygenCapacity) return;

        var holder = oxygenHolders[childOxygen.Count];
        o.hopOnHolder = holder;
        childOxygen.Push(o);
        o.transform.parent = holder.transform;
        o.master = this;
        o.state = OxygenState.HopOnCell;
    }

    public void ReleaseOxygen()
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

    //private void OnDrawGizmos()
    //{
    //    if (!Application.isPlaying) return;
    //    List<Transform> neighbors = GetNeighbors();
    //    behaviors[(int)cellState].DrawGizmos(this, neighbors);
    //}
}
