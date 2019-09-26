using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Attract")]
public class Attract : CellBehavior
{
    private Vector3 currentVelocity;
    public override Vector3 CalculateMove(Cell cell, List<Transform> context)
    {
        if (context.Count == 0)
            return Vector3.zero;

        Vector3 move = Vector3.zero;
        for (int i = 0; i < context.Count; i++)
        {
            move += context[i].position;
        }
        move /= context.Count;
        move -= cell.transform.position;

        move = Vector3.SmoothDamp(cell.transform.right, move, ref currentVelocity, 0.5f);

        return move;
    }
}
