using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TunnelParts
{
    public List<PathJoint> tunnel;
    public List<Vector4> dimension;
    public List<Matrix4x4> inverseTransformMatrix;

    public TunnelParts(List<PathJoint> tunnel, List<Vector4> dimension, List<Matrix4x4> matrix)
    {
        this.tunnel = tunnel;
        this.dimension = dimension;
        this.inverseTransformMatrix = matrix;
    }
}

public class PathBuilder : MonoBehaviour
{
    [SerializeField]
    private PathJoint tubePrefab;
    [SerializeField]
    private PathJoint spherePrefab;
    [SerializeField]
    private PathJoint zonePrefab;

    [SerializeField]
    public float radius = 4f;

    private float scale;
    private Dictionary<Node, TunnelParts> tunnelsOnNode;
    private Dictionary<Node, PathJoint> jointOnNode;
    private TunnelParts entireTunnels;
    private TunnelParts entireZones;
    private List<float> entireZoneRads;

    private void Awake()
    {
        scale = radius * 2;
        tunnelsOnNode = new Dictionary<Node, TunnelParts>();
        jointOnNode = new Dictionary<Node, PathJoint>();
        entireTunnels = new TunnelParts(new List<PathJoint>(), new List<Vector4>(), new List<Matrix4x4>());
        entireZones = new TunnelParts(new List<PathJoint>(), new List<Vector4>(), new List<Matrix4x4>());
        entireZoneRads = new List<float>();
        InitPathObjects();
        BuildPath();
    }

    private void InitPathObjects()
    {
        var zones = Path.Instance.zones;
        for (int i = 0; i < zones.Count; i++)
        {
            Zone zone = zones[i];
            PathJoint joint = Instantiate(zonePrefab, zone.transform.position, Quaternion.identity);
            float scale = zone.Radius * 2f;
            joint.transform.localScale = new Vector3(scale, scale, scale);
            entireZoneRads.Add(zone.Radius);
            Matrix4x4 inverseMatrix = Matrix4x4.TRS(zone.transform.position, zone.transform.rotation, Vector3.one).inverse;

            entireZones.tunnel.Add(joint);
            entireZones.inverseTransformMatrix.Add(inverseMatrix);
            jointOnNode.Add(zone, joint);
        }

        var nodes = Path.Instance.nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            PathJoint joint = Instantiate(spherePrefab, nodes[i].transform.position, Quaternion.identity);
            joint.transform.localScale = new Vector3(scale, scale, scale);
            Node currNode = nodes[i];
            jointOnNode.Add(currNode, joint);
        }

        for (int i = 0; i < Path.Instance.segments.Count; i++)
        {
            // put sphere on the node
            Segment currSeg = Path.Instance.segments[i];
            Node startNode = currSeg.n0;
            Node endNode = currSeg.n1;

            Vector3 direction = endNode.transform.position - startNode.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction.normalized);

            float length = direction.magnitude;
            Vector3 center = (startNode.transform.position + endNode.transform.position) / 2f;
            Matrix4x4 inverseMatrix = Matrix4x4.TRS(center, rotation, Vector3.one).inverse;
            Vector4 dimension = new Vector3(radius, radius, length / 2f);

            PathJoint tube = Instantiate(tubePrefab, startNode.transform.position,
            rotation);
            tube.transform.localScale = new Vector3(scale, scale, length);

            // pulse
            tube.Material.SetVector(tube.pulseDirection, direction.normalized);
            tube.Material.SetInt(tube.veinTiling, Random.Range(5, 8));
            tube.Material.SetFloat(tube.veinWarpTiling, Random.Range(2f, 2.5f));
            tube.Material.SetFloat(tube.veinScale, Random.Range(0.2f, 0.65f));

            addToDictionary(startNode, tube, dimension, inverseMatrix);
            addToDictionary(endNode, tube, dimension, inverseMatrix);

            entireTunnels.tunnel.Add(tube);
            entireTunnels.dimension.Add(dimension);
            entireTunnels.inverseTransformMatrix.Add(inverseMatrix);
        }
    }

    private void BuildPath()
    {
        var zones = Path.Instance.zones;
        for (int i = 0; i < zones.Count; i++)
        {
            Zone zone = zones[i];
            var joint = jointOnNode[zone];

            joint.Material.SetInt(joint.cylinderNum, entireTunnels.tunnel.Count);
            joint.Material.SetVectorArray(joint.cylinderDimension, entireTunnels.dimension);
            joint.Material.SetMatrixArray(joint.cylinderInverseTransform, entireTunnels.inverseTransformMatrix);
        }

        var nodes = Path.Instance.nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            var tunnels = tunnelsOnNode[node];
            var joint = jointOnNode[node];

            joint.Material.SetInt(joint.cylinderNum, tunnels.tunnel.Count);
            joint.Material.SetVectorArray(joint.cylinderDimension, tunnels.dimension);
            joint.Material.SetMatrixArray(joint.cylinderInverseTransform, tunnels.inverseTransformMatrix);

            joint.Material.SetInt(joint.zoneNum, entireZones.tunnel.Count);
            joint.Material.SetFloatArray(joint.zoneRadius, entireZoneRads);
            joint.Material.SetMatrixArray(joint.zoneInverseTransform, entireZones.inverseTransformMatrix);
        }

        for (int i = 0; i < entireTunnels.tunnel.Count; i++)
        {
            var currTube = entireTunnels.tunnel[i];

            List<Vector4> dimensions = new List<Vector4>();
            List<Matrix4x4> matrixes = new List<Matrix4x4>();

            for (int j = 0; j < entireTunnels.tunnel.Count; j++)
            {
                if (i == j) continue;
                dimensions.Add(entireTunnels.dimension[j]);
                matrixes.Add(entireTunnels.inverseTransformMatrix[j]);
            }

            currTube.Material.SetInt(currTube.cylinderNum, entireTunnels.tunnel.Count - 1);
            currTube.Material.SetVectorArray(currTube.cylinderDimension, dimensions);
            currTube.Material.SetMatrixArray(currTube.cylinderInverseTransform, matrixes);

            // zone
            currTube.Material.SetInt(currTube.zoneNum, entireZones.tunnel.Count);
            currTube.Material.SetFloatArray(currTube.zoneRadius, entireZoneRads);
            currTube.Material.SetMatrixArray(currTube.zoneInverseTransform, entireZones.inverseTransformMatrix);
        }
    }

    private void addToDictionary(Node keyNode, PathJoint tube, Vector4 dimension, Matrix4x4 inverseMatrix)
    {
        if (!tunnelsOnNode.ContainsKey(keyNode))
        {
            tunnelsOnNode.Add(keyNode, new TunnelParts(
                new List<PathJoint> { tube },
                new List<Vector4> { dimension },
                new List<Matrix4x4> { inverseMatrix }
            ));
        }
        else
        {
            var curr = tunnelsOnNode[keyNode];
            curr.tunnel.Add(tube);
            curr.dimension.Add(dimension);
            curr.inverseTransformMatrix.Add(inverseMatrix);
        }
    }
}
