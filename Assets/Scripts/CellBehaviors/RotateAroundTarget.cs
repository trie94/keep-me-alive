using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/RotateAroundTarget")]
public class RotateAroundTarget : CellMovement
{
    private Vector3 currentVelocity;

    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target = null)
    {
        Debug.Assert(target != null);
        Vector3 velocity = Vector3.up - creature.transform.position;
        velocity.Normalize();

        float dot = Vector3.Dot(velocity, creature.transform.forward);
        Vector3 up = Vector3.up * dot * 3f;
        velocity = (velocity + up) / 2f;

        velocity = Vector3.SmoothDamp(creature.transform.forward, velocity, ref currentVelocity, 0.5f);
        return velocity;
    }
}
