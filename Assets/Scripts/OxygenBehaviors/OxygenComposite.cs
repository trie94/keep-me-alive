using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oxygen/Composite")]
public class OxygenComposite : OxygenMovement
{
    public OxygenMovement[] behaviors;
    public float[] weights;

    public override Vector3 CalculateVelocity(Oxygen creature, List<Transform> neighbors)
    {
        if (weights.Length != behaviors.Length)
        {
            Debug.LogError("Data mismatch in " + name, this);
            return Vector3.zero;
        }

        Vector3 velocity = Vector3.zero;
        for (int i = 0; i < behaviors.Length; i++)
        {
            Vector3 partialVelocity = behaviors[i].CalculateVelocity(creature, neighbors) * weights[i];

            if (partialVelocity != Vector3.zero)
            {
                if (partialVelocity.sqrMagnitude > weights[i] * weights[i])
                {
                    partialVelocity.Normalize();
                    partialVelocity *= weights[i];
                }

                velocity += partialVelocity;
            }
        }

        return velocity;
    }

    public override void DrawGizmos(Oxygen creature, List<Transform> context)
    {
        for (int i = 0; i < behaviors.Length; i++)
        {
            behaviors[i].DrawGizmos(creature, context);
        }
    }
}
