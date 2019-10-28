using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/RepelObstacles")]
public class RepelObstacles : CellBehavior
{
    private float dist = 1f;
    public override Vector3 CalculateMove(Cell cell, List<Transform> neighbors)
    {
        Vector3 move = Vector3.zero;
        RaycastHit hit;
        
        if (Physics.Raycast(cell.transform.position, cell.transform.up, out hit, dist))
        {
            // if it hits obstacles
            if (CellController.obstacles.Contains(hit.collider))
            {
                move = hit.normal;
            }
        }

        return move;
    }
}
