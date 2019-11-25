using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Main, Sub
}

[System.Serializable]
public struct Node
{
    public NodeType type;
    public Vector3 position;
    public Vector3 direction;
    public float forward;
    public float backward;

    public Node (NodeType type, Vector3 position, Vector3 direction, float forward, float backward)
    {
        this.type = type;
        this.position = position;
        this.direction = direction;
        this.forward = forward;
        this.backward = backward;
    }
}

[System.Serializable]
public struct Segment
{
    public Node n0;
    public Node n1;
    public float weight;

    public Segment(Node n0, Node n1, float weight)
    {
        this.n0 = n0;
        this.n1 = n1;
        this.weight = weight;
    }
}

[ExecuteInEditMode]
public class Path : MonoBehaviour
{
    public Transform[] endpoints;
    public Node[] nodes;
    public Segment[] segments;
    private const float MAIN_WEIGHT = 0.7f;
    private const float SUB_WEIGHT = 0.2f;

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

    private void Awake()
    {
        BuildNodes();
    }

    private void BuildNodes()
    {
        nodes = new Node[endpoints.Length];
        segments = new Segment[endpoints.Length];

        for (int i=0; i<endpoints.Length; i++)
        {
            Transform currPoint = endpoints[i];
            Node node = new Node(
                NodeType.Main, currPoint.position, currPoint.forward, 0.5f, 0.5f
            );
            nodes[i] = node;
        }

        for (int i=0; i<nodes.Length; i++)
        {
            Segment segment = new Segment(
                nodes[i], nodes[(i+1)%nodes.Length], MAIN_WEIGHT
                );
            segments[i] = segment;
        }
    }

    public Vector3 GetPoint(Segment s, float t)
    {
        return s.n0.position + (s.n1.position-s.n0.position) * t;
    }
}
