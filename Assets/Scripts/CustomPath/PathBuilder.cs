using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Tunnels
{
    public List<PathJoint> tunnel;
    public List<Vector4> center;

    public List<Vector4> dimension;
    public List<Matrix4x4> inverseTransformMatrix;

    public Tunnels(List<PathJoint> tunnel, List<Vector4> center, List<Vector4> dimension, List<Matrix4x4> matrix)
    {
        this.tunnel = tunnel;
        this.center = center;
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
    private float radius = 4f;
    private float scale;
    private Dictionary<Node, Tunnels> tunnelsOnNode;
    private Dictionary<Node, PathJoint> jointOnNode;

    private void Awake()
    {
        scale = radius * 2;
        tunnelsOnNode = new Dictionary<Node, Tunnels>();
        jointOnNode = new Dictionary<Node, PathJoint>();
        InitPathObjects();
        BuildPath();
    }

    private void InitPathObjects()
    {
        for (int i = 0; i < Path.Instance.segments.Count; i++)
        {
            // put sphere on the node
            Segment currSeg = Path.Instance.segments[i];
            Node startNode = currSeg.n0;
            Node endNode = currSeg.n1;

            PathJoint joint = Instantiate(spherePrefab, startNode.transform.position, Quaternion.identity);
            joint.transform.localScale = new Vector3(scale, scale, scale);

            Vector3 direction = endNode.transform.position - startNode.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction.normalized);

            float length = direction.magnitude;
            Vector3 center = (startNode.transform.position + endNode.transform.position) / 2;
            Matrix4x4 inverseMatrix = Matrix4x4.TRS(center, rotation, Vector3.one).inverse;

            PathJoint tube = Instantiate(tubePrefab, startNode.transform.position,
            rotation);
            tube.transform.localScale = new Vector3(scale, scale, length);

            if (!jointOnNode.ContainsKey(startNode)) jointOnNode.Add(startNode, joint);
            addToDictionary(startNode, tube, center, length, inverseMatrix);
            addToDictionary(endNode, tube, center, length, inverseMatrix);
        }
    }

    private void BuildPath()
    {
        var nodes = Path.Instance.nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            var tunnels = tunnelsOnNode[node];
            var joint = jointOnNode[node];
            joint.Material.SetInt(joint.cylinderNum, tunnels.center.Count);
            joint.Material.SetVectorArray(joint.cylinderPosition, tunnels.center);
            joint.Material.SetVectorArray(joint.cylinderDimension, tunnels.dimension);
            joint.Material.SetMatrixArray(joint.cylinderInverseTransform, tunnels.inverseTransformMatrix);
        }
    }

    private void addToDictionary(Node keyNode, PathJoint tube, Vector4 center, float length, Matrix4x4 inverseMatrix)
    {
        if (!tunnelsOnNode.ContainsKey(keyNode))
        {
            tunnelsOnNode.Add(keyNode, new Tunnels(
                new List<PathJoint> { tube },
                new List<Vector4> { center },
                new List<Vector4> { new Vector3(radius, radius, length / 2f) },
                new List<Matrix4x4> { inverseMatrix }
            ));
        }
        else
        {
            var curr = tunnelsOnNode[keyNode];
            curr.tunnel.Add(tube);
            curr.center.Add(center);
            curr.dimension.Add(new Vector3(radius, radius, length / 2f));
            curr.inverseTransformMatrix.Add(inverseMatrix);
        }
    }
}
