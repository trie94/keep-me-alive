using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Tunnels
{
    public List<PathJoint> tunnel;

    public List<Vector4> dimension;
    public List<Matrix4x4> inverseTransformMatrix;

    public Tunnels(List<PathJoint> tunnel, List<Vector4> dimension, List<Matrix4x4> matrix)
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
    private float radius = 4f;
    private float scale;
    private Dictionary<Node, Tunnels> tunnelsOnNode;
    private Dictionary<Node, PathJoint> jointOnNode;
    private Tunnels entireTunnels;

    private void Awake()
    {
        scale = radius * 2;
        tunnelsOnNode = new Dictionary<Node, Tunnels>();
        jointOnNode = new Dictionary<Node, PathJoint>();
        entireTunnels = new Tunnels(
            new List<PathJoint>(), new List<Vector4>(), new List<Matrix4x4>()
        );
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
            Vector3 center = (startNode.transform.position + endNode.transform.position) / 2f;
            Matrix4x4 inverseMatrix = Matrix4x4.TRS(center, rotation, Vector3.one).inverse;
            Vector4 dimension = new Vector3(radius, radius, length / 2f);

            PathJoint tube = Instantiate(tubePrefab, startNode.transform.position,
            rotation);
            tube.transform.localScale = new Vector3(scale, scale, length);

            if (!jointOnNode.ContainsKey(startNode)) jointOnNode.Add(startNode, joint);

            addToDictionary(startNode, tube, dimension, inverseMatrix);
            addToDictionary(endNode, tube, dimension, inverseMatrix);

            entireTunnels.tunnel.Add(tube);
            entireTunnels.dimension.Add(dimension);
            entireTunnels.inverseTransformMatrix.Add(inverseMatrix);
        }
    }

    private void BuildPath()
    {
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
            currTube.Material.SetInt(currTube.cylinderNum, entireTunnels.tunnel.Count-1);
            currTube.Material.SetVectorArray(currTube.cylinderDimension, dimensions);
            currTube.Material.SetMatrixArray(currTube.cylinderInverseTransform, matrixes);
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
        }

    }

    private void addToDictionary(Node keyNode, PathJoint tube, Vector4 dimension, Matrix4x4 inverseMatrix)
    {
        if (!tunnelsOnNode.ContainsKey(keyNode))
        {
            tunnelsOnNode.Add(keyNode, new Tunnels(
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
