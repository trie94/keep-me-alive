using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/StayInRadius")]
public class StayInRadius : CellMovement
{
    public float radius = 15f;
    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        Vector3 centerOffset = target.Value - creature.transform.position;
        float t = centerOffset.magnitude / radius;

        if (t < 0.9f)
        {
            return Vector3.zero;
        }
        return centerOffset * t * t;
    }
}
