using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/StayOnLine")]
/*
* https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Vector_formulation
*/
public class StayOnLine : CellMovement
{
    private Vector3 debugVelocity;
    public override Vector3 CalculateVelocity(Cell creature, List<Transform> neighbors, Vector3? target)
    {
        if (creature.currSeg.n0 == null || creature.currSeg.n1 == null) return Vector3.zero;
        Vector3 cellToStartPoint = creature.currSeg.n0.transform.position 
                                       - creature.transform.position;
        Vector3 segDir = (creature.currSeg.n1.transform.position 
                          - creature.currSeg.n0.transform.position).normalized;
        Vector3 cellToClosestPointOnLine = 
            (cellToStartPoint - Vector3.Dot(cellToStartPoint, segDir) * segDir);

        debugVelocity = cellToClosestPointOnLine;
        float dist = cellToClosestPointOnLine.magnitude;
        return cellToClosestPointOnLine * dist;
    }
}
