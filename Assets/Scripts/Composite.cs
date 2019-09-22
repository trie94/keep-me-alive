using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Composite")]
public class Composite : CellBehavior
{
    public CellBehavior[] behaviors;
    public float[] weights;

    public override Vector3 CalculateMove(Cell cell, List<Transform> context)
    {
        if (weights.Length != behaviors.Length)
        {
            Debug.LogError("Data mismatch in " + name, this);
            return Vector3.zero;
        }

        float sum = 0f;
        Vector3 move = Vector3.zero;
        for (int i = 0; i < behaviors.Length; i++)
        {
            Vector3 partialMove = behaviors[i].CalculateMove(cell, context) * weights[i];
            sum += weights[i];

            if (partialMove != Vector3.zero)
            {
                if (partialMove.sqrMagnitude > weights[i] * weights[i])
                {
                    partialMove.Normalize();
                    partialMove *= weights[i];
                }

                move += partialMove;

            }
        }

        // if (sum != 0f) move /= sum;
        return move;
    }
}
