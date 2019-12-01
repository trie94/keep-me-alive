using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Attract")]
public class Attract : CellBehavior
{
    public override Vector3 CalculateVelocity(Cell cell, List<Transform> neighbors)
    {
        if (neighbors.Count == 0)
            return Vector3.zero;

        Vector3 velocity = Vector3.zero;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var curr = neighbors[i];
            velocity += curr.position;
        }
        velocity /= neighbors.Count;
        velocity -= cell.transform.position;

        return velocity;
    }
}
