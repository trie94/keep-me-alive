using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/Attract")]
public class OxygenAttract : OxygenMovement
{
    public override Vector3 CalculateVelocity(Oxygen creature, List<Transform> neighbors)
    {
        if (neighbors == null || neighbors.Count == 0) return Vector3.zero;

        Vector3 velocity = Vector3.zero;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var curr = neighbors[i];
            velocity += curr.position;
        }
        velocity /= neighbors.Count;
        velocity -= creature.transform.position;

        return velocity;
    }
}
