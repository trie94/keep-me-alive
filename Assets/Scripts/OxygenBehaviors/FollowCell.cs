using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/FollowCell")]
public class FollowCell : OxygenMovement
{
    public override Vector3 CalculateVelocity(Oxygen creature, List<Transform> neighbors)
    {
        // this makes the oxygen follow the master cell
        return creature.master.transform.position - creature.transform.position;
    }
}
