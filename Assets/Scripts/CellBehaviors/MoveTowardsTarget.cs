using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/MoveTowardsTarget")]
public class MoveTowardsTarget : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, List<Transform> neighbors)
    {
        Vector3 targetNode = Vector3.zero;
        Erythrocyte erythrocyte = (Erythrocyte)creature;

        if (erythrocyte.cellState == ErythrocyteState.EnterOxygenArea)
        {
            targetNode = OxygenController.Instance.oxygenArea.position;
        }
        else if (erythrocyte.cellState == ErythrocyteState.EnterHeartArea
                 || erythrocyte.cellState == ErythrocyteState.ReleaseOxygen)
        {
            targetNode = OxygenController.Instance.heart.position;
        }
        else if (erythrocyte.cellState == ErythrocyteState.ExitOxygenArea)
        {
            targetNode = CellController.Instance.oxygenExitNode.position;
        }
        else if (erythrocyte.cellState == ErythrocyteState.ExitHeartArea)
        {
            targetNode = CellController.Instance.heardExitNode.position;
        }

        Vector3 velocity = (targetNode - erythrocyte.transform.position);
        velocity *= velocity.magnitude;

        return velocity;
    }
}
