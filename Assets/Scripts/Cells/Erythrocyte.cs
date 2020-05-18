using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ErythrocyteState
{
    InVein, CollectOxygenInOxygenArea, ExitOxygenArea, BodyTissueArea, ExitBodyTissueArea
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
    public MoleculeCarrierBehavior Carrier { get { return carrier; } }
    private Segment nextSeg;

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
        creatureGroups[CreatureTypes.Oxygen] = GetAvailableOxygen();
        velocity = Vector3.zero;

        if (cellState == ErythrocyteState.InVein)
        {
            if (prevState != cellState)
            {
                prevState = cellState;
                if (nextSeg != null)
                {
                    currSeg = nextSeg;
                }
                else
                {
                    currSeg = GetNextSegment();
                }
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups);
        }
        else if (cellState == ErythrocyteState.CollectOxygenInOxygenArea)
        {
            if (target == null || prevState != cellState)
            {
                prevState = cellState;
                target = Path.Instance.OxygenZone.transform.position;
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);

            if (!carrier.hasVacancy())
            {
                prevState = cellState;
                cellState = ErythrocyteState.ExitOxygenArea;
            }
        }
        else if (cellState == ErythrocyteState.ExitOxygenArea)
        {
            if (target == null || prevState != cellState)
            {
                prevState = cellState;
                int targetIndex = Random.Range(0, Path.Instance.OxygenExitSegments.Count);
                nextSeg = Path.Instance.OxygenExitSegments[targetIndex];
                target = nextSeg.start.transform.position;
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
            if (prevState != cellState)
            {
                target = BodyTissueGenerator.Instance.center.position;
                prevState = cellState;
                creatureGroups[CreatureTypes.BodyTissue] = GetHungryBodyTissues(Random.Range(0, BodyTissueGenerator.Instance.bodyTissueGroups.Count));
            }
            velocity = behaviors[(int)cellState].CalculateVelocity(this, creatureGroups, target);

            if (!carrier.CanReleaseOxygen())
            {
                prevState = cellState;
                cellState = ErythrocyteState.ExitBodyTissueArea;
                oxygenReleaseTick = 0f;
                target = null;
            }
        }
        else if (cellState == ErythrocyteState.ExitBodyTissueArea)
        {
            if (target == null || prevState != cellState)
            {
                prevState = cellState;
                int targetIndex = Random.Range(0, Path.Instance.BodyTissueExitSegments.Count);
                nextSeg = Path.Instance.BodyTissueExitSegments[targetIndex];
                target = nextSeg.start.transform.position;
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
        var endNode = currSeg.end.type;

        if (endNode == NodeType.BodyTissue)
        {
            if ((Path.Instance.BodyTissueZone.transform.position - transform.position).sqrMagnitude < Path.Instance.BodyTissueZone.Radius * Path.Instance.BodyTissueZone.Radius)
            {
                prevState = cellState;
                cellState = ErythrocyteState.BodyTissueArea;
            }
        }
        else if (endNode == NodeType.Oxygen)
        {
            if ((Path.Instance.OxygenZone.transform.position - transform.position).sqrMagnitude < Path.Instance.OxygenZone.Radius * Path.Instance.OxygenZone.Radius)
            {
                prevState = cellState;
                cellState = ErythrocyteState.CollectOxygenInOxygenArea;
            }
        }
        else
        {
            prevState = cellState;
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
