using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/HitHeart")]
public class HitHeart : OxygenMovement
{
    public override Vector3 CalculateVelocity(Oxygen creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        Vector3 velocity = target.Value - creature.transform.position;
        return velocity * velocity.magnitude;
    }
}
