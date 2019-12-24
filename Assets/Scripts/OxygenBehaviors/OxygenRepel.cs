using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/Repel")]
public class OxygenRepel : OxygenMovement
{
    private Vector3 currentVelocity;
    public override Vector3 CalculateVelocity(Oxygen creature, List<Transform> neighbors, Vector3? target)
    {
        if (neighbors == null || neighbors.Count == 0) return Vector3.zero;

        Vector3 velocity = Vector3.zero;
        int nAvoid = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var curr = neighbors[i];
            if (Vector3.SqrMagnitude(curr.position - creature.transform.position) < creature.squareAvoidanceRadius)
            {
                nAvoid++;
                velocity += (creature.transform.position - curr.position);
            }
        }
        if (nAvoid > 0) velocity /= nAvoid;

        // since repel causes jittery movement, we need to smooth out here
        // and add more weight when composite all the velocity
        velocity = Vector3.SmoothDamp(creature.transform.forward, velocity, ref currentVelocity, 0.5f);
        return velocity;
    }
}
