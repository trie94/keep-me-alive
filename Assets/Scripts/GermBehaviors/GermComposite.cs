﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Germ/Composite")]
public class GermComposite : GermMovement
{
    public GermMovement[] behaviors;
    public float[] weights;

    public override Vector3 CalculateVelocity(Germ creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        if (weights.Length != behaviors.Length)
        {
            Debug.LogError("Data mismatch in " + name, this);
            return Vector3.zero;
        }

        Vector3 velocity = Vector3.zero;
        for (int i = 0; i < behaviors.Length; i++)
        {
            Vector3 partialVelocity = behaviors[i].CalculateVelocity(creature, groups, target) * weights[i];

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

    public override void DrawGizmos(Germ creature, Dictionary<CreatureTypes, List<Transform>> groups)
    {
        for (int i = 0; i < behaviors.Length; i++)
        {
            behaviors[i].DrawGizmos(creature, groups);
        }
    }
}
