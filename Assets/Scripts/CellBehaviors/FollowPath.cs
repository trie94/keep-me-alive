using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/FollowPath")]
public class FollowPath : CellMovement
{
    public override Vector3 CalculateVelocity(Cell creature, Dictionary<CreatureTypes, List<Transform>> groups, Vector3? target)
    {
        creature.progress += Time.deltaTime * creature.speed;
        if (creature.currSeg.n0 == null || creature.currSeg.n1 == null) return Vector3.zero;
        float distSqrt = (creature.currSeg.n1.transform.position - creature.transform.position).sqrMagnitude;

        if (distSqrt <= 1.5f)
        {
            creature.progress = 0f;
            creature.currSeg = GetNextSegment(creature);
            creature.UpdateCellState();
        }

        Vector3 velocity = creature.currSeg.n1.transform.position
                                   - creature.currSeg.n0.transform.position;
        return velocity;
    }

    private Segment GetNextSegment(Cell cell)
    {
        var nextSeg = cell.currSeg.n1.nextSegments;
        if (nextSeg.Count > 1)
        {
            float weightSum = 0f;
            for (int i = 0; i < nextSeg.Count; i++)
            {
                weightSum += nextSeg[i].weight;
            }

            float rnd = Random.Range(0, weightSum);
            for (int i = 0; i < nextSeg.Count; i++)
            {
                if (rnd < nextSeg[i].weight)
                    return nextSeg[i];
                rnd -= nextSeg[i].weight;
            }
        }
        else
        {
            return cell.currSeg.n1.nextSegments[0];
        }

        return cell.currSeg.n1.nextSegments[0];
    }
}
