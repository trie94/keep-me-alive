using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/StayOnLine")]
/*
* https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Vector_formulation
*/
public class StayOnLine : CellBehavior
{
    private Vector3 debugVelocity;
    public override Vector3 CalculateVelocity(Cell cell, List<Transform> neighbors)
    {
        if (cell.currSeg.n0 == null || cell.currSeg.n1 == null) return Vector3.zero;
        Vector3 cellToStartPoint = cell.currSeg.n0.transform.position 
                                       - cell.transform.position;
        Vector3 segDir = (cell.currSeg.n1.transform.position 
                          - cell.currSeg.n0.transform.position).normalized;
        Vector3 cellToClosestPointOnLine = 
            (cellToStartPoint - Vector3.Dot(cellToStartPoint, segDir) * segDir);

        debugVelocity = cellToClosestPointOnLine;
        float dist = cellToClosestPointOnLine.magnitude;
        return cellToClosestPointOnLine * dist;
    }

    public override void DrawGizmos(Cell cell, List<Transform> context)
    {
        Gizmos.color = Color.red;
        if (debugVelocity != Vector3.zero)
        {
            Gizmos.DrawLine(cell.transform.position, cell.transform.position+debugVelocity);
        }
    }
}
