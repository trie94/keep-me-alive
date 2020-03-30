using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Segment
{
    public Node start;
    public Node end;
    public float weight;
    private Vector3 direction;
    public Vector3 Direction { get { return direction; } set { direction = value; } }

    public Segment(Node start, Node end, float weight)
    {
        this.start = start;
        this.end = end;
        this.weight = weight;
    }
}

[ExecuteInEditMode]
public class Path : MonoBehaviour
{
    public List<Segment> segments = new List<Segment>();
    public List<Node> nodes = new List<Node>();
    public List<Zone> zones = new List<Zone>();
    public Zone OxygenZone { get { return zones[0]; } }
    public Zone HeartZone { get { return zones[1]; } }

    private HashSet<Node> oxygenExitNodes;
    private HashSet<Node> heartExitNodes;

    private List<Segment> oxygenExitSegments;
    public List<Segment> OxygenExitSegments { get { return oxygenExitSegments; } }
    private List<Segment> heartExitSegments;
    public List<Segment> HeartExitSegments { get {return heartExitSegments; } }

    private const float HIGH = 10f;
    private const float MID = 5f;
    private const float LOW = 1f;

    private static Path instance;
    public static Path Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Path>();
            }
            return instance;
        }
    }
    public bool debugMode = true;

    private void Awake()
    {
        for (int i=0; i<segments.Count; i++)
        {
            var currSeg = segments[i];
            currSeg.Direction = (currSeg.end.transform.position - currSeg.start.transform.position).normalized;
        }

        // build exit node segment lists
        oxygenExitNodes = new HashSet<Node>();
        heartExitNodes = new HashSet<Node>();
        oxygenExitSegments = new List<Segment>();
        heartExitSegments = new List<Segment>();

        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            if (node.type == NodeType.OxyenExit)
            {
                oxygenExitNodes.Add(node);
            }
            else if (node.type == NodeType.HeartExit)
            {
                heartExitNodes.Add(node);
            }
        }

        BuildExitSegments(oxygenExitNodes, oxygenExitSegments);
        BuildExitSegments(heartExitNodes, heartExitSegments);
    }

    private void Start()
    {
        CalculateWeight();
    }

    private void BuildExitSegments(HashSet<Node> exitNode, List<Segment> exitSegments)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            var currSeg = segments[i];
            if (exitNode.Contains(currSeg.start))
            {
                exitSegments.Add(currSeg);
            }
        }
    }

    private void CalculateWeight()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            var currSeg = segments[i];
            currSeg.weight
                   = GetWeight(currSeg.start.weight) + GetWeight(currSeg.end.weight);
        }
    }

    private float GetWeight(NodeWeight weight)
    {
        switch (weight)
        {
            case NodeWeight.High:
                return HIGH;
            case NodeWeight.Mid:
                return MID;
            case NodeWeight.Low:
                return LOW;
            default:
                return 0f;
        }
    }

    public Vector3 GetPoint(Segment s, float t)
    {
        return Vector3.Lerp(s.start.transform.position, s.end.transform.position, t);
    }
}
