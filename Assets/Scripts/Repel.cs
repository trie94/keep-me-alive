using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Repel")]
public class Repel : CellBehavior
{
    public override Vector3 CalculateMove(Cell cell, List<Transform> context)
    {
        if (context.Count == 0)
            return Vector3.zero;

        Vector3 move = Vector3.zero;
        int nAvoid = 0;

        for (int i = 0; i < context.Count; i++)
        {
            var curr = context[i];
            if (Vector3.SqrMagnitude(curr.position - cell.transform.position) < CellController.squareAvoidanceRadius)
            {
                nAvoid++;
                move += (cell.transform.position - curr.position);
            }
        }
        if (nAvoid > 0) move /= nAvoid;

        return move;
    }
}
