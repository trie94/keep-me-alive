using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/AvoidGerms")]
public class AvoidGerms : CellMovement
{
    public CreatureTypes type = CreatureTypes.Germ;
    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        List<Transform> neighbors = groups[type];
        return Vector3.zero;
    }
}
