using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellBehaviorTester : Cell
{
    public override void Start()
    {
    }

    public override void Update()
    {
        creatureGroups[CreatureTypes.Cell] = GetCellNeighbors();
        creatureGroups[CreatureTypes.Germ] = GetGermNeighbors();

        Vector3 velocity = behaviors[0].CalculateVelocity(this, creatureGroups);
        velocity *= velocityMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        Move(velocity);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        behaviors[0].DrawGizmos(this, creatureGroups);
    }
}
