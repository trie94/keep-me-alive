using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/Align")]
public class OxygenAlign : OxygenMovement
{
    public override Vector3 CalculateVelocity(Oxygen creature, List<Transform> neighbors, Transform target)
    {
        if (neighbors == null || neighbors.Count == 0) return creature.transform.forward;

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
