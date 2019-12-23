using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Erythrocyte/GetOxygen")]
public class GetOxygen : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, List<Transform> neighbors)
    {
        // TO-DO: idle movement
        return Vector3.zero;
    }
}
