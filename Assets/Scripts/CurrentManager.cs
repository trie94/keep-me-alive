using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentManager : MonoBehaviour
{
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

    public Vector3 GetCurrent(List<NeighborSegments> neighborSegments)
    {
        Debug.Assert(neighborSegments.Count > 0);
        Vector3 current = Vector3.zero;
        for (int i = 0; i < neighborSegments.Count; i++)
        {
            var segment = neighborSegments[i];
            current += segment.direction * segment.weight;
        }
        current.Normalize();
        return current;
    }

    public Vector3 GetCurrent(PlayerZoneState currZoneState,
        List<NeighborSegments> neighborSegments)
    {
        Debug.Assert(neighborSegments.Count > 0);
        Vector3 current = Vector3.zero;
        if (currZoneState == PlayerZoneState.Vein)
        {
            for (int i = 0; i < neighborSegments.Count; i++)
            {
                var segment = neighborSegments[i];
                current += segment.direction * segment.weight;
            }
            current.Normalize();
        }
        else
        {
            current = Vector3.zero;
        }
        return current;
    }

    // we need to check all the possible segments nearby because this does not
    // reflect the direction of the cell. If we don't count each node's prev and
    // next segments, it will cause an issue especially in the zone, where start
    // and end node are mixed up. In order to avoid the same computation,
    // HashSet is used.
    public CurrAndNeighborSegments GetCurrAndNeighborSegments(Segment currSeg, Transform transform)
    {
        Segment potential = currSeg;
        float min = (GetClosestPointOnLine(potential, transform) - transform.position).sqrMagnitude;

        HashSet<Segment> computedSegments = new HashSet<Segment>();
        List<NeighborSegments> neighborSegments = new List<NeighborSegments>();
        computedSegments.Add(potential);
        neighborSegments.Add(new NeighborSegments(potential, min));

        var currStartNode = currSeg.n0;
        var currEndNode = currSeg.n1;

        for (int i = 0; i < currStartNode.prevSegments.Count; i++)
        {
            var seg = currStartNode.prevSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg, transform) - transform.position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
                neighborSegments.Add(new NeighborSegments(seg, sqrDist));
            }
        }

        for (int i = 0; i < currStartNode.nextSegments.Count; i++)
        {
            var seg = currStartNode.nextSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg, transform) - transform.position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
                neighborSegments.Add(new NeighborSegments(seg, sqrDist));
            }
        }

        for (int i = 0; i < currEndNode.prevSegments.Count; i++)
        {
            var seg = currEndNode.prevSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg, transform) - transform.position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
                neighborSegments.Add(new NeighborSegments(seg, sqrDist));
            }
        }

        for (int i = 0; i < currEndNode.nextSegments.Count; i++)
        {
            var seg = currEndNode.nextSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg, transform) - transform.position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
                neighborSegments.Add(new NeighborSegments(seg, sqrDist));
            }
        }

        return new CurrAndNeighborSegments(potential, neighborSegments);
    }

    public Vector3 GetClosestPointOnLine(Segment seg, Transform transform)
    {
        Vector3 startPointToPlayer = transform.position - seg.n0.transform.position;
        if (startPointToPlayer == Vector3.zero) return startPointToPlayer;
        Vector3 segDir = (seg.n1.transform.position - seg.n0.transform.position);
        float t = Mathf.Clamp01(Vector3.Dot(startPointToPlayer, segDir) / segDir.sqrMagnitude);
        return seg.n0.transform.position + t * segDir;
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

    public NeighborSegments(Segment seg, float sqrDist)
    {
        direction = (seg.n1.transform.position - seg.n0.transform.position).normalized;
        weight = 1f / sqrDist;
    }

    public NeighborSegments(Vector3 direction, float weight)
    {
        this.direction = direction;
        this.weight = weight;
    }
}
