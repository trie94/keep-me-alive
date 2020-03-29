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

    public Vector3 GetCurrent(List<NeighborSegments> neighborSegments)
    {
        Debug.Assert(neighborSegments.Count > 0);
        Vector3 current = Vector3.zero;
        for (int i = 0; i < neighborSegments.Count; i++)
        {
            var segment = neighborSegments[i];
            current += segment.direction * segment.weight;
        }
        if (current != Vector3.zero)
        {
            current.Normalize();
        }
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

    public CurrAndNeighborSegments GetCurrAndNeighborSegments(Segment currSeg, Vector3 position)
    {
        Segment potential = currSeg;
        float min = (GetClosestPointOnLine(potential, position) - position).sqrMagnitude;

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
                float sqrDist = (GetClosestPointOnLine(seg, position) - position).sqrMagnitude;
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
                float sqrDist = (GetClosestPointOnLine(seg, position) - position).sqrMagnitude;
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
                float sqrDist = (GetClosestPointOnLine(seg, position) - position).sqrMagnitude;
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
                float sqrDist = (GetClosestPointOnLine(seg, position) - position).sqrMagnitude;
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

    // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Vector_formulation
    public Vector3 GetClosestPointOnLine(Segment seg, Transform transform)
    {
        Vector3 startPointToTransform = transform.position - seg.n0.transform.position;
        if (startPointToTransform == Vector3.zero) return startPointToTransform;
        Vector3 segDir = (seg.n1.transform.position - seg.n0.transform.position);
        float t = Mathf.Clamp01(Vector3.Dot(startPointToTransform, segDir) / segDir.sqrMagnitude);
        return seg.n0.transform.position + t * segDir;
    }

    public Vector3 GetClosestPointOnLine(Segment seg, Vector3 position)
    {
        Vector3 startPointToTransform = position - seg.n0.transform.position;
        if (startPointToTransform == Vector3.zero) return startPointToTransform;
        Vector3 segDir = (seg.n1.transform.position - seg.n0.transform.position);
        float t = Mathf.Clamp01(Vector3.Dot(startPointToTransform, segDir) / segDir.sqrMagnitude);
        return seg.n0.transform.position + t * segDir;
    }

    private void DrawForceField()
    {
        Gizmos.color = Color.magenta;
        float numStepI = 1f;
        float progress = 0f;

        var segments = Path.Instance.segments;
        for (int i=0; i<segments.Count; i++)
        {
            var currSeg = segments[i];
            var currSegDir = (currSeg.n1.transform.position - currSeg.n0.transform.position);
            float currSegMagnitude = currSegDir.magnitude;
            float step = numStepI / currSegMagnitude;
            while (progress <= 1f)
            {
                float offset = -3f;
                while (offset <= 3f)
                {
                    var point1 = Path.Instance.GetPoint(currSeg, progress);
                    var point2 = Path.Instance.GetPoint(currSeg, progress) + new Vector3(offset, 0, 0);
                    var point3 = Path.Instance.GetPoint(currSeg, progress) + new Vector3(0, offset, 0);
                    var point4 = Path.Instance.GetPoint(currSeg, progress) + new Vector3(0, 0, offset);
                    var current1 = GetCurrent(GetCurrAndNeighborSegments(currSeg, point1).neighbors);
                    var current2 = GetCurrent(GetCurrAndNeighborSegments(currSeg, point2).neighbors);
                    var current3 = GetCurrent(GetCurrAndNeighborSegments(currSeg, point3).neighbors);
                    var current4 = GetCurrent(GetCurrAndNeighborSegments(currSeg, point4).neighbors);
                    Gizmos.DrawLine(point1, point1 + current1);
                    Gizmos.DrawLine(point2, point2 + current2);
                    Gizmos.DrawLine(point3, point3 + current3);
                    Gizmos.DrawLine(point4, point4 + current4);
                    offset += 0.5f;
                }
                progress += step;
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

    public NeighborSegments(Segment seg, float sqrDist)
    {
        direction = (seg.n1.transform.position - seg.n0.transform.position).normalized;
        // 0.1f is added to avoid dividing by zero.
        weight = 1f / (sqrDist + 0.1f);
    }

    public NeighborSegments(Vector3 direction, float weight)
    {
        this.direction = direction;
        this.weight = weight;
    }
}
