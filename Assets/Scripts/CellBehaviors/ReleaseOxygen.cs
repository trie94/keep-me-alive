using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Erythrocyte/ReleaseOxygen")]
public class ReleaseOxygen : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, List<Transform> neighbors)
    {
        Erythrocyte erythrocyte = (Erythrocyte)creature;

        if (erythrocyte.childOxygen.Count <= 0)
        {
            erythrocyte.cellState = CellState.ExitHeart;
            erythrocyte.oxygenReleaseTick = 0f;

            return Vector3.zero;
        }

        // get closer to the heart
        // if close enough, release oxygen
        if (Vector3.SqrMagnitude(erythrocyte.transform.position
                                 - CellController.Instance.heart.position) < 0.2f)
        {
            // stop the movement..! or spin around the heart
            // --> this will be implemented in the other behavior
            // release oxygen one by one
            if (erythrocyte.oxygenReleaseTick >= erythrocyte.oxygenReleaseInterval)
            {
                erythrocyte.oxygenReleaseTick = 0f;
            }
            else
            {
                erythrocyte.oxygenReleaseTick += Time.deltaTime;
                erythrocyte.ReleaseOxygen();
            }
            return Vector3.zero;
        }
        else
        {
            Vector3 velocity = CellController.Instance.heart.position - erythrocyte.transform.position;
            float multiplier = velocity.magnitude;

            // we don't want to add too much force when it's close
            return velocity * multiplier;
        }
    }
}
