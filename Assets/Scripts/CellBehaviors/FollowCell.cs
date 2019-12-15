using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/FollowCell")]
public class FollowCell : CellBehavior
{
    public override Vector3 CalculateVelocity(Cell cell, List<Transform> neighbors)
    {
        // this makes the oxygen follow the master cell
        Oxygen curr = (Oxygen)cell;
        return curr.master.transform.position - curr.transform.position;
    }
}
