using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMeshBuilder : MonoBehaviour
{
    private int tubularSegments = 40;
    private int radialSegments = 20;
    [Range(0.1f, 7f)]
    public float radius = 4f;
    [SerializeField]
    private PathJoint tunnelPrefab;
    [SerializeField]
    private PathJoint jointPrefab;
    [SerializeField]
    private PathJoint zonePrefab;
    private List<Vector3> debugVertices = new List<Vector3>();
    private List<CatmullRomCurve> curves = new List<CatmullRomCurve>();
    private List<PathJoint> tunnelPaths = new List<PathJoint>();
    private List<JointInfo> jointInfo = new List<JointInfo>();
    private List<JointInfo> jointInfoWithoutZone = new List<JointInfo>();
    private List<PathJoint> zones = new List<PathJoint>();

    private static PathMeshBuilder instance;
    public static PathMeshBuilder Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PathMeshBuilder>();
            }
            return instance;
        }
    }

    private void Start()
    {
        instance = this;
        BuildZones();
        BuildJoints();
        for (int i = 0; i < Path.Instance.curves.Count; i++)
        {
            var points = new List<Vector3>();
            var currentCurve = Path.Instance.curves[i];
            for (int j = 0; j < currentCurve.nodes.Count; j++)
            {
                points.Add(currentCurve.nodes[j].transform.position);
            }

            BuildCurve(points, currentCurve.isClosed);
        }
        // BuildPath();
    }

    private void BuildZones()
    {
        for (int i = 0; i < Path.Instance.zones.Count; i++)
        {
            Zone zone = Path.Instance.zones[i];
            PathJoint joint = Instantiate(zonePrefab, zone.transform.position, Quaternion.identity);
            float scale = zone.Radius * 2f;
            joint.transform.localScale = new Vector3(scale, scale, scale);
            Matrix4x4 inverseMatrix = Matrix4x4.TRS(zone.transform.position, zone.transform.rotation, Vector3.one).inverse;
            JointInfo info = new JointInfo(joint, zone.Radius, inverseMatrix);
            jointInfo.Add(info);
            zones.Add(joint);
        }
    }

    private void BuildJoints()
    {
        for (int i = 0; i < Path.Instance.joints.Count; i++)
        {
            var j = Path.Instance.joints[i];
            PathJoint joint = Instantiate(jointPrefab, j.transform.position, Quaternion.identity);
            float scale = radius * 2f;
            joint.transform.localScale = new Vector3(scale, scale, scale);
            Matrix4x4 inverseMatrix = Matrix4x4.TRS(joint.transform.position, joint.transform.rotation, Vector3.one).inverse;
            JointInfo info = new JointInfo(joint, radius, inverseMatrix);
            jointInfo.Add(info);
            jointInfoWithoutZone.Add(info);
            joint.Rend.enabled = false;
        }
    }

    private void BuildCurve(List<Vector3> points, bool isClosed)
    {
        PathJoint tunnel = Instantiate(tunnelPrefab);
        CatmullRomCurve curve = new CatmullRomCurve(points, isClosed);
        curves.Add(curve);
        tunnelPaths.Add(tunnel);

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var tangents = new List<Vector4>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();
        var frames = curve.ComputeFrenetFrames(tubularSegments, isClosed);

        for (int j = 0; j <= tubularSegments; j++)
        {
            GenerateSubSegments(curve, frames, vertices, normals, tangents, j);
        }

        for (int k = 0; k <= tubularSegments; k++)
        {
            for (int j = 0; j <= radialSegments; j++)
            {
                float u = 1f * j / radialSegments;
                float v = 1f * k / tubularSegments;
                uvs.Add(new Vector2(u, v));
            }
        }
        debugVertices = vertices;
        // side
        for (int j = 1; j <= tubularSegments; j++)
        {
            for (int k = 1; k <= radialSegments; k++)
            {
                int a = (radialSegments + 1) * (j - 1) + (k - 1);
                int b = (radialSegments + 1) * j + (k - 1);
                int c = (radialSegments + 1) * j + k;
                int d = (radialSegments + 1) * (j - 1) + k;
                triangles.Add(a); triangles.Add(d); triangles.Add(b);
                triangles.Add(b); triangles.Add(d); triangles.Add(c);
            }
        }
        var mesh = new Mesh();
        tunnel.GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.tangents = tangents.ToArray();
        mesh.uv = uvs.ToArray();
        // we need back rendering
        // triangles.Reverse();
        mesh.triangles = triangles.ToArray();
    }

    private void BuildPath()
    {
        List<Matrix4x4> inverseTransformMatrixs = new List<Matrix4x4>();
        List<float> radius = new List<float>();

        for (int i = 0; i < jointInfo.Count; i++)
        {
            inverseTransformMatrixs.Add(jointInfo[i].inverseTransformMatrix);
            radius.Add(jointInfo[i].radius);
        }

        for (int i = 0; i < tunnelPaths.Count; i++)
        {
            PathJoint curr = tunnelPaths[i];
            curr.Material.SetInt(curr.cylinderNum, 0);
            curr.Material.SetInt(curr.zoneNum, jointInfo.Count);
            curr.Material.SetFloatArray(curr.zoneRadius, radius);
            curr.Material.SetMatrixArray(curr.zoneInverseTransform, inverseTransformMatrixs);
        }

        // reuse this for compute zones
        inverseTransformMatrixs = new List<Matrix4x4>();
        radius = new List<float>();

        for (int i = 0; i < jointInfoWithoutZone.Count; i++)
        {
            inverseTransformMatrixs.Add(jointInfoWithoutZone[i].inverseTransformMatrix);
            radius.Add(jointInfoWithoutZone[i].radius);
        }

        for (int i = 0; i < zones.Count; i++)
        {
            PathJoint curr = zones[i];
            curr.Material.SetInt(curr.cylinderNum, 0);
            curr.Material.SetInt(curr.zoneNum, jointInfoWithoutZone.Count);
            curr.Material.SetFloatArray(curr.zoneRadius, radius);
            curr.Material.SetMatrixArray(curr.zoneInverseTransform, inverseTransformMatrixs);
        }
    }

    private void GenerateSubSegments(
        CurveBase curve, List<FrenetFrame> frames, List<Vector3> vertices, List<Vector3> normals, List<Vector4> tangents, int index)
    {
        // 0~1
        var u = 1f * index / tubularSegments;
        var p = curve.GetPointAt(u);
        var fr = frames[index];
        var N = fr.normal;
        var B = fr.binormal;

        for (int i = 0; i <= radialSegments; i++)
        {
            // 0~2pi
            float rad = 1f * i / radialSegments * Mathf.PI * 2;
            float x = Mathf.Cos(rad);
            float y = Mathf.Sin(rad);

            var v = (x * N + y * B).normalized;
            vertices.Add(p + radius * v);
            normals.Add(v);
            var tangent = fr.tangent;
            tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 0f));
        }
    }

    private void OnDrawGizmos()
    {
    }

    private void DrawGizmo(CatmullRomCurve curve)
    {
        Gizmos.color = Color.white;
        Vector3 lineStart = curve.GetPointAt(0f);
        int lineSteps = 20;
        for (int i = 1; i <= lineSteps; i++)
        {
            Gizmos.DrawSphere(lineStart, 0.2f);
            Vector3 lineEnd = curve.GetPointAt(i / (float)lineSteps);
            Gizmos.DrawLine(lineStart, lineEnd);
            lineStart = lineEnd;
        }
    }
}

public struct JointInfo
{
    public PathJoint joint;
    public float radius;
    public Matrix4x4 inverseTransformMatrix;

    public JointInfo(PathJoint joint, float radius, Matrix4x4 inverseTransformMatrix)
    {
        this.joint = joint;
        this.radius = radius;
        this.inverseTransformMatrix = inverseTransformMatrix;
    }
}