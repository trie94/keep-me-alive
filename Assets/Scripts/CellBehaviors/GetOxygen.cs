using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Erythrocyte/GetOxygen")]
public class GetOxygen : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, List<Transform> neighbors, Vector3? target)
    {
        return Vector3.zero;
    }
}
