using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathJoint : MonoBehaviour
{
    private Renderer rend;
    public Renderer Rend { get { return rend; } }

    private Material material;
    public Material Material { get { return material; } }

    public int cylinderDimension;
    public int cylinderInverseTransform;
    public int cylinderNum;
    public int pulseDirection;
    public int veinTiling;
    public int veinScale;
    public int veinWarpTiling;

    public int zoneRadius;
    public int zoneInverseTransform;
    public int zoneNum;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend) material = rend.material;
        cylinderNum = Shader.PropertyToID("_CylinderNum");
        cylinderDimension = Shader.PropertyToID("_CylinderDimension");
        cylinderInverseTransform = Shader.PropertyToID("_CylinderInverseTransform");

        zoneNum = Shader.PropertyToID("_ZoneNum");
        zoneInverseTransform = Shader.PropertyToID("_ZoneInverseTransform");
        zoneRadius = Shader.PropertyToID("_ZoneRadius");

        pulseDirection = Shader.PropertyToID("_PulseDirection");
        veinTiling = Shader.PropertyToID("_Tiling");
        veinScale = Shader.PropertyToID("_WarpScale");
        veinWarpTiling = Shader.PropertyToID("_WarpTiling");
    }
}
