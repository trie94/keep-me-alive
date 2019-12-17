using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/MoveTowardsTarget")]
public class MoveTowardsTarget : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, List<Transform> neighbors)
    {
        Vector3 exitNode = Vector3.zero;

        if (creature.cellState == CellState.ExitOxygen)
        {
            exitNode = CellController.Instance.oxygenExitNode.position;
        }
        else if (creature.cellState == CellState.ExitHeart)
        {
            exitNode = CellController.Instance.heardExitNode.position;
        }

        if (Vector3.SqrMagnitude(exitNode - creature.transform.position) < 0.3f)
        {
            creature.cellState = CellState.InVein;
            return Vector3.zero;
        }

        return exitNode - creature.transform.position;
    }
}
