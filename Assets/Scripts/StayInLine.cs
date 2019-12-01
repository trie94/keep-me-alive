using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/StayInLine")]
/*
* https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Vector_formulation
*/
public class StayInLine : CellBehavior
{
    public override Vector3 CalculateMove(Cell cell, List<Transform> context)
    {
        Vector3 cellToStartPoint = cell.currSeg.n0.transform.position 
                                       - cell.transform.position;
        Vector3 segDir = (cell.currSeg.n1.transform.position 
                          - cell.currSeg.n0.transform.position).normalized;
        Vector3 cellToClosestPointOnLine = 
            (cellToStartPoint - Vector3.Dot(cellToStartPoint, segDir) * segDir);

        float dist = cellToClosestPointOnLine.magnitude;
        return cellToClosestPointOnLine * dist;
    }

    public override void DrawGizmos(Cell cell, List<Transform> context)
    {
        Vector3 cellToStartPoint = cell.currSeg.n0.transform.position
                                       - cell.transform.position;
        Vector3 segDir = (cell.currSeg.n1.transform.position
                          - cell.currSeg.n0.transform.position).normalized;
        Vector3 cellToClosestPointOnLine =
            (cellToStartPoint - Vector3.Dot(cellToStartPoint, segDir) * segDir);

        float dist = cellToClosestPointOnLine.magnitude;
        Gizmos.DrawLine(cell.transform.position, cell.transform.position+cellToClosestPointOnLine);
    }
}
