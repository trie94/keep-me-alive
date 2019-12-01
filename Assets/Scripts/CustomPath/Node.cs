﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum NodeWeight
{
    High, Mid, Low
}

[System.Serializable]
public class Node : MonoBehaviour
{
    public NodeWeight weight;
    public Vector3 direction;
    public float forward = 0.5f;
    public float backward = 0.5f;
    public List<Segment> nextSegments;

    private void Awake()
    {
        GetSegments();
        ColorWeight();
    }

    private void GetSegments()
    {
        nextSegments = new List<Segment>();
        var segments = Path.Instance.segments;
        for (int i = 0; i < segments.Count; i++)
        {
            Segment segment = segments[i];
            if (segment.n0 == this)
            {
                nextSegments.Add(segment);
            }
        }
    }

    private void ColorWeight()
    {
        Renderer rend = GetComponent<Renderer>();
        if (CellController.Instance && CellController.Instance.debugMode
            || CellControllerPathTestVersion.Instance && CellControllerPathTestVersion.Instance.isDebugMode)
        {
            rend.material.SetFloat("_Weight", GetWeigthOpac(weight));
            rend.material.SetFloat("_Opacity", 1f);
        }
        else
        {
            rend.material.SetFloat("_Opacity", 0f);
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