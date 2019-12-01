﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Repel")]
public class Repel : CellBehavior
{
    public override Vector3 CalculateMove(Cell cell, List<Transform> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 move = Vector3.zero;
        int nAvoid = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var curr = neighbors[i];
            if (Vector3.SqrMagnitude(curr.position - cell.transform.position) < CellController.Instance.squareAvoidanceRadius)
            {
                nAvoid++;
                move += (cell.transform.position - curr.position);
            }
        }
        if (nAvoid > 0) move /= nAvoid;

        return move;
    }
}