using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Composite")]
public class Composite : CellMovement
{
    public CellMovement[] behaviors;
    public float[] weights;
    public Vector3 debug;

    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        if (weights.Length != behaviors.Length)
        {
            Debug.LogError("Data mismatch in " + name, this);
            return Vector3.zero;
        }

        Vector3 velocity = Vector3.zero;
        float totalWeight = 0f;
        for (int i = 0; i < behaviors.Length; i++)
        {
            Vector3 partialVelocity = behaviors[i].CalculateVelocity(creature, groups, target) * weights[i];
            float currWeight = weights[i];
            if (partialVelocity != Vector3.zero)
            {
                if (partialVelocity.sqrMagnitude > currWeight * currWeight)
                {
                    partialVelocity.Normalize();
                    partialVelocity *= weights[i];
                }
                totalWeight += currWeight;
                velocity += partialVelocity;
            }
        }
        if (totalWeight > 0f) velocity /= totalWeight;

        debug = velocity;
        return velocity;
    }

    public override void DrawGizmos(Cell creature, List<Transform> context)
    {
        for (int i = 0; i < behaviors.Length; i++)
        {
            behaviors[i].DrawGizmos(creature, context);
        }
        Gizmos.DrawLine(creature.transform.position, creature.transform.position + debug);
    }
}
