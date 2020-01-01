using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : Node
{
    [SerializeField]
    private float radius = 3f;
    public float Radius { get { return radius; }}
    public NodeType nodeType { get { return nodeType; }}

}
