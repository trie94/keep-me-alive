using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentManager : MonoBehaviour
{
    // if you enable this, the game will be super super slow.
    [SerializeField]
    private bool drawForceField = false;

    private static CurrentManager instance;
    public static CurrentManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CurrentManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public Vector3 GetCurrent(PlayerZoneState currZoneState, Segment currSeg, Vector3 position)
    {
        if (currZoneState == PlayerZoneState.Vein)
        {
            return GetCurrent(currSeg, position);
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 GetCurrent(Segment currSeg, Vector3 position)
    {
        Node startNode = currSeg.start;
        Node endNode = currSeg.end;

        float sqrDistBetweenPosAndStartNode = (startNode.transform.position - position).sqrMagnitude;
        float sqrDistBetweenPosAndEndNode = (endNode.transform.position - position).sqrMagnitude;

        float sqrRad = PathBuilder.Instance.radius * PathBuilder.Instance.radius * 2f;

        if (sqrDistBetweenPosAndStartNode <= sqrRad || sqrDistBetweenPosAndEndNode <= sqrRad)
        {
            Node closerNode = sqrDistBetweenPosAndStartNode < sqrDistBetweenPosAndEndNode ?
    startNode : endNode;
            Vector3 nodeToPosition = (position - closerNode.transform.position).normalized;
            Vector3 current = Vector3.zero;
            List<float> weights = new List<float>();

            for (int i=0; i<closerNode.nextSegments.Count; i++)
            {
                Vector3 seg = closerNode.nextSegments[i].Direction;
                weights.Add(1f);
            }

            for (int i=0; i<closerNode.prevSegments.Count; i++)
            {
                Vector3 seg = closerNode.prevSegments[i].Direction;
                weights.Add(1f);
            }

            for (int i = 0; i < closerNode.nextSegments.Count; i++)
            {
                Vector3 seg = closerNode.nextSegments[i].Direction;
                float weight = Mathf.Cos(Vector3.Angle(seg, nodeToPosition) * Mathf.Deg2Rad) * 0.5f + 0.5f;
                weight = 1 - weight;
                for (int j=0; j<weights.Count; j++)
                {
                    if (j == i) continue;
                    weights[j] *= weight;
                }
            }

            for (int i = 0; i < closerNode.prevSegments.Count; i++)
            {
                Vector3 seg = closerNode.prevSegments[i].Direction;
                float weight = Mathf.Cos(Vector3.Angle(-seg, nodeToPosition) * Mathf.Deg2Rad) * 0.5f + 0.5f;
                weight = 1 - weight;
                for (int j = 0; j < weights.Count; j++)
                {
                    if (j == i + closerNode.nextSegments.Count) continue;
                    weights[j] *= weight;
                }
            }

            float sum = 0f;
            for (int i=0; i<weights.Count; i++)
            {
                sum += weights[i];
            }

            for (int i = 0; i < closerNode.nextSegments.Count; i++)
            {
                current += closerNode.nextSegments[i].Direction * weights[i] / sum;
            }

            for (int i = 0; i < closerNode.prevSegments.Count; i++)
            {
                current += closerNode.prevSegments[i].Direction * weights[i+closerNode.nextSegments.Count] / sum;
            }

            if (current != Vector3.zero) current.Normalize();
            return current;
        }
        else
        {
            return currSeg.Direction;
        }
    }

    // we need to check all the possible segments nearby because this does not
    // reflect the direction of the cell. If we don't count each node's prev and
    // next segments, it will cause an issue especially in the zone, where start
    // and end node are mixed up. In order to avoid the same computation,
    // HashSet is used.
    public Segment UpdateCurrentSegment(Segment currSeg, Vector3 position)
    {
        Segment potential = currSeg;
        float min = (GetClosestPointOnLine(potential, position) - position).sqrMagnitude;
        var currStartNode = currSeg.start;
        var currEndNode = currSeg.end;
        HashSet<Segment> computedSegments = new HashSet<Segment>();
        computedSegments.Add(potential);

        for (int i = 0; i < currStartNode.prevSegments.Count; i++)
        {
            var seg = currStartNode.prevSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg, position) - position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
            }
        }

        for (int i = 0; i < currStartNode.nextSegments.Count; i++)
        {
            var seg = currStartNode.nextSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg, position) - position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
            }
        }

        for (int i = 0; i < currEndNode.prevSegments.Count; i++)
        {
            var seg = currEndNode.prevSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg, position) - position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
            }
        }

        for (int i = 0; i < currEndNode.nextSegments.Count; i++)
        {
            var seg = currEndNode.nextSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg, position) - position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
            }
        }
        return potential;
    }

    // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Vector_formulation
    public Vector3 GetClosestPointOnLine(Segment seg, Vector3 position)
    {
        Vector3 startPointToTransform = position - seg.start.transform.position;
        if (startPointToTransform == Vector3.zero) return startPointToTransform;
        Vector3 segDir = (seg.end.transform.position - seg.start.transform.position);
        float t = Mathf.Clamp01(Vector3.Dot(startPointToTransform, segDir) / segDir.sqrMagnitude);
        return seg.start.transform.position + t * segDir;
    }

    private void DrawForceField()
    {
        Gizmos.color = Color.magenta;
        float numStep = 1f;
        float progress = 0f;
        var stepSize = 1f;
        var radiusSize = 3.5f;

        var segments = Path.Instance.segments;
        for (int i=0; i<segments.Count; i++)
        {
            var currSeg = segments[i];
            float currSegMagnitude = (currSeg.end.transform.position - currSeg.start.transform.position).magnitude;
            float segmentStepSize = numStep / currSegMagnitude;

            while (progress <= 1f)
            {
                float stepX = -radiusSize;
                while (stepX < radiusSize)
                {
                    float stepY = radiusSize;
                    while (stepY > -radiusSize)
                    {
                        var point = Path.Instance.GetPoint(currSeg, progress) +
                        Vector3.Cross(currSeg.Direction, Vector3.up) * stepX
                        + Vector3.up * stepY;
                        var current = GetCurrent(currSeg, point);
                        Gizmos.DrawLine(point, point + current);
                        stepY -= stepSize;
                    }
                    stepX += stepSize;
                }
                progress += segmentStepSize;
            }
            progress = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !drawForceField) return;
        DrawForceField();
    }
}

public struct CurrAndNeighborSegments
{
    public Segment currSeg;
    public List<NeighborSegments> neighbors;

    public CurrAndNeighborSegments(Segment currSeg, List<NeighborSegments> neighbors)
    {
        this.currSeg = currSeg;
        this.neighbors = neighbors;
    }
}

public struct NeighborSegments
{
    public Vector3 direction;
    public float weight;

    public NeighborSegments(Vector3 direction, float weight)
    {
        this.direction = direction;
        this.weight = weight;
    }
}
