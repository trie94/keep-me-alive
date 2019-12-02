using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Align")]
public class Align : CellBehavior
{
    public override Vector3 CalculateVelocity(Cell cell, List<Transform> neighbors)
    {
        if (neighbors == null || neighbors.Count == 0) return cell.transform.up;

        //add all points together and average
        Vector3 velocity = Vector3.zero;
        for (int i=0; i<neighbors.Count; i++)
        {
            var curr = neighbors[i];
            velocity += curr.transform.up;
        }

        velocity /= neighbors.Count;
        return velocity;
    }
}
