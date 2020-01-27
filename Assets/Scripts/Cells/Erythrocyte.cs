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
    [SerializeField]
    private int oxygenCapacity = 3;
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

    public override void Awake()
    {
        base.Awake();
        childOxygen = new Stack<Oxygen>();
        cellType = CellType.Erythrocyte;
    }

    public override void Start()
    {
        base.Start();
        UpdateCellState();
    }

    public override void Update()
    {
        creatureGroups[CreatureTypes.Cell] = GetCellNeighbors();
        creatureGroups[CreatureTypes.Germ] = GetGermNeighbors();

        Vector3 velocity = Vector3.zero;

        if (cellState == ErythrocyteState.InVein)
        {
            if (prevState != cellState)
            {
                currSeg = GetNextSegment();
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups);
        }
        else if (cellState == ErythrocyteState.EnterOxygenArea)
        {
            if (target == null || prevState != cellState)
            {
                prevState = cellState;
                target = CellController.Instance.GetRandomPositionInOxygenArea();
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);

            if (Vector3.SqrMagnitude(target.Value - transform.position) < 0.5f)
            {
                prevState = cellState;
                cellState = ErythrocyteState.WaitOxygen;
            }
        }
        else if (cellState == ErythrocyteState.WaitOxygen)
        {
            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, Path.Instance.OxygenZone.transform.position);
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
                int targetIndex = Random.Range(0, Path.Instance.OxygenExitSegments.Count);
                target = Path.Instance.OxygenExitSegments[targetIndex].n0.transform.position;
            }

            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);
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
                velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);

                if (Vector3.SqrMagnitude(target.Value - transform.position) < 0.7f)
                {
                    prevState = cellState;
                    cellState = ErythrocyteState.ReleaseOxygen;
                }
            }
        }
        else if (cellState == ErythrocyteState.ReleaseOxygen)
        {
            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);

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
                int targetIndex = Random.Range(0, Path.Instance.HeartExitSegments.Count);
                target = Path.Instance.HeartExitSegments[targetIndex].n0.transform.position;
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);
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

    public override void UpdateCellState()
    {
        prevState = cellState;
        var startNode = currSeg.n0.type;
        if (startNode == NodeType.HeartEntrance)
        {
            cellState = ErythrocyteState.EnterHeartArea;
        }
        else if (startNode == NodeType.OxygenEntrance)
        {
            cellState = ErythrocyteState.EnterOxygenArea;
        }
        else
        {
            cellState = ErythrocyteState.InVein;
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (cellState == ErythrocyteState.InVein)
        {
            behaviors[(int)cellState].DrawGizmos(this, creatureGroups);
        }
    }
}
