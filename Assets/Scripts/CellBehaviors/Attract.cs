using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/Attract")]
public class Attract : CellMovement
{
    public CreatureTypes type = CreatureTypes.Cell;
    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        List<Transform> neighbors = groups[type];
        if (neighbors == null || neighbors.Count == 0) return Vector3.zero;

        Vector3 velocity = Vector3.zero;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var curr = neighbors[i];
            velocity += curr.position;
        }
        velocity /= neighbors.Count;
        velocity -= creature.transform.position;

        return velocity;
    }
}
