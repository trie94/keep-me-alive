using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Align")]
public class Align : CellBehavior
{
    public float agentSmoothTime = 0.5f;

    public override Vector3 CalculateMove(Cell cell, List<Transform> neighbors)
    {
        if (neighbors.Count == 0)
            return cell.transform.up;

        //add all points together and average
        Vector3 move = Vector3.zero;
        for (int i=0; i<neighbors.Count; i++)
        {
            var curr = neighbors[i];
            move += curr.transform.up;
        }
        move /= neighbors.Count;
        return move;
    }
}
