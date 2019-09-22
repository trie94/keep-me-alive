using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Cohesion")]
public class Cohesion : CellBehavior
{
    Vector3 currentVelocity;
    public float agentSmoothTime = 0.5f;

    public override Vector3 CalculateMove(Cell cell, List<Transform> context)
    {
        if (CellController.Instance.cells.Count == 0)
            return Vector3.zero;

        // average all cells, not just neighbors
        Vector3 cohesionMove = Vector3.zero;
        for (int i = 0; i < CellController.Instance.cells.Count; i++)
        {
            var curr = CellController.Instance.cells[i];
            cohesionMove += curr.transform.position;
        }
        cohesionMove /= CellController.Instance.cells.Count;

        cohesionMove -= cell.transform.position;
        cohesionMove = Vector3.SmoothDamp(cell.transform.forward, cohesionMove, ref currentVelocity, agentSmoothTime);
        return cohesionMove;
    }
}
