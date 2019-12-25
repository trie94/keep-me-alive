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
    [SerializeField]
    private Vector3? target = null;

    [SerializeField]
    private ErythrocyteState cellState;
    [SerializeField]
    private ErythrocyteState prevState;

    public override void Start()
    {
        base.Start();
        UpdateCellState();
        childOxygen = new Stack<Oxygen>();
    }

    public override void Update()
    {
        List<Transform> neighbors = GetNeighbors();
        Vector3 velocity = Vector3.zero;

        if (cellState == ErythrocyteState.InVein)
        {
            velocity = behaviors[(int)cellState].CalculateVelocity(this, neighbors);
        }
        else if (cellState == ErythrocyteState.EnterOxygenArea)
        {
            if (target == null || prevState != cellState)
            {
                prevState = cellState;
                target = CellController.Instance.GetRandomPositionInOxygenArea();
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, neighbors, target);

            if (Vector3.SqrMagnitude(target.Value - transform.position) < 0.5f)
            {
                prevState = cellState;
                cellState = ErythrocyteState.WaitOxygen;
            }
        }
        else if (cellState == ErythrocyteState.WaitOxygen)
        {
            velocity = behaviors[(int)cellState].CalculateVelocity(this, neighbors, CellController.Instance.oxygenArea.position);
            if (childOxygen.Count < oxygenCapacity)
            {
                for (int i = 0; i < oxygenCapacity; i++)
                {
                    Oxygen oxygen = OxygenController.Instance.oxygens.Pop();
                    RegisterOxygen(oxygen);
                }
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

                if (isReadyToGo)
                {
                    prevState = cellState;
                    cellState = ErythrocyteState.ExitOxygenArea;
                }
            }
        }
        else if (cellState == ErythrocyteState.ExitOxygenArea)
        {
            if (target == null || prevState != cellState)
            {
                prevState = cellState;
                target = CellController.Instance.oxygenExitNode.position;
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, neighbors, target);
            if (Vector3.SqrMagnitude(target.Value - transform.position) < 0.5f)
            {
                prevState = cellState;
                cellState = ErythrocyteState.InVein;
            }
        }
        else if (cellState == ErythrocyteState.EnterHeartArea)
        {
            if (childOxygen.Count <= 0)
            {
                prevState = cellState;
                cellState = ErythrocyteState.ExitHeartArea;
            }
            else
            {
                if (target == null || prevState != cellState)
                {
                    prevState = cellState;
                    target = CellController.Instance.GetRandomPositionInHeartArea();
                }
                velocity = behaviors[(int)cellState].CalculateVelocity(this, neighbors, target);

                if (Vector3.SqrMagnitude(target.Value - transform.position) < 0.7f)
                {
                    prevState = cellState;
                    cellState = ErythrocyteState.ReleaseOxygen;
                }
            }
        }
        else if (cellState == ErythrocyteState.ReleaseOxygen)
        {
            velocity = behaviors[(int)cellState].CalculateVelocity(this, neighbors);

            if (childOxygen.Count <= 0)
            {
                prevState = cellState;
                cellState = ErythrocyteState.ExitHeartArea;
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
        else if (cellState == ErythrocyteState.ExitHeartArea)
        {
            if (target == null || prevState != cellState)
            {
                prevState = cellState;
                target = CellController.Instance.heartExitNode.position;
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, neighbors, target);
            if (Vector3.SqrMagnitude(target.Value - transform.position) < 0.5f)
            {
                prevState = cellState;
                cellState = ErythrocyteState.InVein;
            }
        }

        velocity *= velocityMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        Move(velocity);

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
        prevState = cellState;
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
        Debug.Assert(childOxygen.Count < oxygenCapacity);
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        List<Transform> neighbors = GetNeighbors();
        behaviors[(int)cellState].DrawGizmos(this, neighbors);
    }
}
