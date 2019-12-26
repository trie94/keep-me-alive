using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GermFollowTarget : GermMovement
{
    public override Vector3 CalculateVelocity(Germ creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target = null)
    {
        return Vector3.zero;
    }
}
