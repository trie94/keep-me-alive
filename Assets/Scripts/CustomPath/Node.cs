using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum NodeWeight
{
    High, Mid, Low
}

[System.Serializable]
public enum NodeType
{
    Vein, OxygenEntrance, OxyenExit, HeartEntrance, HeartExit,
    Oxygen, Heart
}

[System.Serializable]
public class Node : MonoBehaviour
{
    public NodeWeight weight;
    public NodeType type;
    public Vector3 direction;
    public float forward = 0.5f;
    public float backward = 0.5f;
    public List<Segment> nextSegments;
    public List<Segment> prevSegments;

    private void Awake()
    {
        GetSegments();
        ColorWeight();
    }

    private void GetSegments()
    {
        nextSegments = new List<Segment>();
        prevSegments = new List<Segment>();

        var segments = Path.Instance.segments;
        for (int i = 0; i < segments.Count; i++)
        {
            Segment segment = segments[i];
            if (segment.n0 == this)
            {
                nextSegments.Add(segment);
            }

            if (segment.n1 == this)
            {
                prevSegments.Add(segment);
            }
        }
    }

    private void ColorWeight()
    {  
        Renderer rend = GetComponent<Renderer>();
        if (Path.Instance.debugMode)
        {
            rend.material.SetFloat("_Weight", GetWeigthOpac(weight));
            rend.material.SetFloat("_Opacity", 1f);
        }
        else
        {
            rend.enabled = false;
        }
    }

    private float GetWeigthOpac(NodeWeight nodeWeight)
    {
        switch(nodeWeight)
        {
            case NodeWeight.High:
                return 1f;
            case NodeWeight.Mid:
                return 0.5f;
            case NodeWeight.Low:
            default:
                return 0f;
        }
    }
}
