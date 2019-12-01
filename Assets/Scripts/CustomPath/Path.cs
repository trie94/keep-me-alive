using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Segment
{
    public Node n0;
    public Node n1;
    public float weight;
}

[ExecuteInEditMode]
public class Path : MonoBehaviour
{
    public List<Segment> segments = new List<Segment>();

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

    private void Start()
    {
        CalculateWeight();
    }

    private void CalculateWeight()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            var currSeg = segments[i];
            currSeg.weight
                   = GetWeight(currSeg.n0.weight) + GetWeight(currSeg.n1.weight);
        }
    }

    private float GetWeight(NodeWeight weight)
    {
        switch(weight)
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
        return Vector3.Lerp(s.n0.transform.position, s.n1.transform.position, t);
    }
}
