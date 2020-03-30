using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/FollowPath")]
public class FollowPath : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        creature.progress += Time.deltaTime * creature.speed;
        if (creature.currSeg.start == null || creature.currSeg.end == null) return Vector3.zero;
        float distSqrt = (creature.currSeg.end.transform.position - creature.transform.position).sqrMagnitude;

        if (distSqrt <= 1.5f)
        {
            creature.progress = 0f;
            creature.currSeg = creature.GetNextSegment();
            creature.UpdateCellState();
        }

        if (creature.currSeg == null) return Vector3.zero;
        Vector3 velocity = creature.currSeg.end.transform.position
                                   - creature.currSeg.start.transform.position;
        return velocity;
    }
}
