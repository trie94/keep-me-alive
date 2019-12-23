using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/FollowCell")]
public class FollowCell : OxygenMovement
{
    public override Vector3 CalculateVelocity(Oxygen creature, List<Transform> neighbors)
    {
        return creature.hopOnHolder.transform.position
                       - creature.transform.position;
    }
}
