using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Align")]
public class Align : CellBehavior
{
    // Vector3 currentVelocity;
    public float agentSmoothTime = 0.5f;

    public override Vector3 CalculateMove(Cell cell, List<Transform> context)
    {
        if (context.Count == 0)
            return cell.transform.up;

        //add all points together and average
        Vector3 move = Vector3.zero;
        for (int i=0; i<context.Count; i++)
        {
            var curr = context[i];
            move += curr.transform.up;
        }
        move /= context.Count;

        // move = Vector3.SmoothDamp(cell.transform.up, move, ref currentVelocity, 3f);

        return move;
    }
}
