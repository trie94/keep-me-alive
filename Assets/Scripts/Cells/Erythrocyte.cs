using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erythrocyte : Cell
{
    public int oxygenCapacity = 3;
    public int currOxygen = 0;

    public override void Start()
    {
        base.Start();
        UpdateCellState();
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
}
