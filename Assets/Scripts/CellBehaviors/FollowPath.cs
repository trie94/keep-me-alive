using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cell/FollowPath")]
public class FollowPath : CellBehavior
{
    public override Vector3 CalculateVelocity(Cell cell, List<Transform> neighbors)
    {
        cell.progress += Time.deltaTime * cell.speed;
        if (cell.currSeg.n0 == null || cell.currSeg.n1 == null) return Vector3.zero;
        float distSqrt = (cell.currSeg.n1.transform.position - cell.transform.position).sqrMagnitude;

        if (distSqrt <= 1.5f)
        {
            cell.progress = 0f;
            cell.currSeg = GetNextSegment(cell);
            cell.UpdateCellStateOnEnterNewNode();
        }

        Vector3 target = Path.Instance.GetPoint(cell.currSeg, cell.progress);
        return target - cell.transform.position;
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
