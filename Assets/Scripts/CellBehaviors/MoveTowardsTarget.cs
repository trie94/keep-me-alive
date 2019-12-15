using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/MoveTowardsTarget")]
public class MoveTowardsTarget : CellBehavior
{
    public override Vector3 CalculateVelocity(Cell cell, List<Transform> neighbors)
    {
        Vector3 exitNode = Vector3.zero;

        if (cell.cellState == CellState.ExitOxygen)
        {
            exitNode = CellController.Instance.oxygenExitNode.position;
        }
        else if (cell.cellState == CellState.EnterHeart)
        {
            exitNode = CellController.Instance.oxygenExitNode.position;
        }

        if (Vector3.SqrMagnitude(exitNode - cell.transform.position) < 0.3f)
        {
            cell.cellState = CellState.InVein;
            return Vector3.zero;
        }

        return exitNode - cell.transform.position;
    }
}
