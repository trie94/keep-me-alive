using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Germ/GermMoveToTarget")]
public class GermMoveToTarget : GermMovement
{
    public override Vector3 CalculateVelocity(Germ creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target = null)
    {
        Debug.Assert(target != null);
        Vector3 velocity = target.Value - creature.transform.position;
        //velocity *= velocity.magnitude;

        return velocity;
    }
}
