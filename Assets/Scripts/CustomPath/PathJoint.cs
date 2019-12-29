using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathJoint : MonoBehaviour
{
    private Renderer rend;
    public Renderer Rend { get { return rend; } }

    [SerializeField]
    private Material materialPrefab;
    private Material material;
    public Material Material { get { return material; } }
    public int cylinderPosition;
    public int cylinderDimension;
    public int cylinderInverseTransform;
    public int cylinderNum;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        material = rend.material;
        cylinderNum = Shader.PropertyToID("_CylinderNum");
        cylinderPosition = Shader.PropertyToID("_CylinderPosition");
        cylinderDimension = Shader.PropertyToID("_CylinderDimension");
        cylinderInverseTransform = Shader.PropertyToID("_CylinderInverseTransform");
    }
}
