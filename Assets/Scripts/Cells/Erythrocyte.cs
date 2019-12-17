using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erythrocyte : Cell
{
    public int oxygenCapacity = 3;
    public float oxygenReleaseInterval = 2f;
    public float oxygenReleaseTick = 0f;
    public Stack<Oxygen> childOxygen;

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
                cellState = CellState.EnterHeart;
                break;
            case NodeType.OxygenEntrance:
                cellState = CellState.EnterOxygen;
                break;
            default:
                cellState = CellState.InVein;
                break;
        }
    }

    public void RegisterOxygen(Oxygen o)
    {
        childOxygen.Push(o);
        o.master = this;
        o.state = OxygenState.BeingCarried;
    }

    public void ReleaseOxygen()
    {
        Debug.Assert(childOxygen.Count > 0);
        Oxygen o = childOxygen.Pop();
        o.master = null;
        o.state = OxygenState.HeartArea;
    }
}
