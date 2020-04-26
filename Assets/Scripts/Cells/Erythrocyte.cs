using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ErythrocyteState
{
    InVein, EnterOxygenArea, WaitOxygen, ExitOxygenArea, BodyTissueArea, ExitBodyTissueArea
}

public class Erythrocyte : Cell
{
    private float oxygenReleaseInterval = 1f;
    private float oxygenReleaseTick = 0f;
    private Vector3? target = null;

    [SerializeField]
    private ErythrocyteState cellState;
    [SerializeField]
    private ErythrocyteState prevState;
    // TODO change this with direction
    private Vector3 velocity;
    public Vector3 Velocity { get { return velocity; } }
    private MoleculeCarrierBehavior carrier;
    private BodyTissue targetBodyTissue;

    public override void Awake()
    {
        base.Awake();
        carrier = GetComponent<MoleculeCarrierBehavior>();
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

        velocity = Vector3.zero;

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
            velocity = behaviors[(int)cellState].CalculateVelocity(
                this, creatureGroups, Path.Instance.OxygenZone.transform.position);
            if (carrier.CanGrab())
            {
                carrier.GrabOxygens();
            }
            else
            {
                if (carrier.IsReadyToGo())
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
                target = Path.Instance.OxygenExitSegments[targetIndex].start.transform.position;
            }

            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);
            if (Vector3.SqrMagnitude(target.Value - transform.position) < 0.5f)
            {
                prevState = cellState;
                cellState = ErythrocyteState.InVein;
            }
        }
        else if (cellState == ErythrocyteState.BodyTissueArea)
        {
            if (!carrier.CanReleaseOxygen())
            {
                prevState = cellState;
                cellState = ErythrocyteState.ExitBodyTissueArea;
                oxygenReleaseTick = 0f;
                targetBodyTissue = null;
                target = null;
            }
            else
            {
                if (targetBodyTissue == null)
                {
                    targetBodyTissue = BodyTissueGenerator.Instance.GetTargetBodyTissue();
                    if (targetBodyTissue == null)
                    {
                        prevState = cellState;
                        cellState = ErythrocyteState.ExitBodyTissueArea;
                        oxygenReleaseTick = 0f;
                        target = null;
                    }
                    else
                    {
                        targetBodyTissue.IsOccupied = true;
                        target = targetBodyTissue.Head;
                        prevState = cellState;
                    }
                }
                else
                {
                    Debug.Assert(targetBodyTissue != null);
                    velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);
                    if ((targetBodyTissue.Head - transform.position).sqrMagnitude < 3f)
                    {
                        carrier.ReleaseOxygen(targetBodyTissue);
                        oxygenReleaseTick = 0f;
                        if (!targetBodyTissue.NeedOxygen())
                        {
                            // unoccupy the current target and find a new one
                            // if there's no bodytissue available, exit the room
                            targetBodyTissue.IsOccupied = false;
                            targetBodyTissue = BodyTissueGenerator.Instance.GetTargetBodyTissue();
                            if (targetBodyTissue == null)
                            {
                                prevState = cellState;
                                cellState = ErythrocyteState.ExitBodyTissueArea;
                                oxygenReleaseTick = 0f;
                                target = null;
                            }
                            else
                            {
                                targetBodyTissue.IsOccupied = true;
                                target = targetBodyTissue.Head;
                            }
                        }
                    }
                }
            }
        }
        else if (cellState == ErythrocyteState.ExitBodyTissueArea)
        {
            if (target == null || prevState != cellState)
            {
                prevState = cellState;
                int targetIndex = Random.Range(0, Path.Instance.HeartExitSegments.Count);
                target = Path.Instance.HeartExitSegments[targetIndex].start.transform.position;
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
        var startNode = currSeg.start.type;
        if (startNode == NodeType.HeartEntrance)
        {
            cellState = ErythrocyteState.BodyTissueArea;
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (cellState == ErythrocyteState.InVein)
        {
            behaviors[(int)cellState].DrawGizmos(this, creatureGroups);
        }
    }
}
