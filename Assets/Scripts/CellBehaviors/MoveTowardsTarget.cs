using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/MoveTowardsTarget")]
public class MoveTowardsTarget : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, List<Transform> neighbors, Transform target)
    {
        Vector3 velocity = target.position - creature.transform.position;
        velocity *= velocity.magnitude * 1.2f;

        return velocity;
    }
}
