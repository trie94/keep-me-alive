using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMeshBuilder : MonoBehaviour
{
    private int tubularSegments = 40;
    private int radialSegments = 10;
    [Range(0.1f, 7f)]
    public float radius = 1;
    [SerializeField]
    private GameObject tunnelPrefab;
    private GameObject tunnel;
    private List<Vector3> debugVertices = new List<Vector3>();
    private CatmullRomCurve curve;
    [SerializeField]
    private List<Transform> nodes;

    private void Awake()
    {
        tunnel = Instantiate(tunnelPrefab);
        BuildCurveAndMesh();
    }

    private void Update()
    {
        BuildCurveAndMesh();
    }

    private void BuildCurveAndMesh()
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < nodes.Count; i++)
        {
            points.Add(nodes[i].transform.position);
        }
        curve = new CatmullRomCurve(points, true);
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var tangents = new List<Vector4>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();
        var frames = curve.ComputeFrenetFrames(tubularSegments, true);

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
        tunnel.GetComponentInChildren<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.tangents = tangents.ToArray();
        mesh.uv = uvs.ToArray();
        // we need back rendering
        triangles.Reverse();
        mesh.triangles = triangles.ToArray();
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
        if (curve == null) return;
        DrawGizmo(curve);
    }

    private void DrawGizmo(CatmullRomCurve curve)
    {
        Gizmos.color = Color.white;
        Vector3 lineStart = curve.GetPointAt(0f);
        int lineSteps = 20;
        for (int i = 1; i <= lineSteps; i++)
        {
            // Gizmos.DrawSphere(lineStart, 0.2f);
            Vector3 lineEnd = curve.GetPointAt(i / (float)lineSteps);
            Gizmos.DrawLine(lineStart, lineEnd);
            lineStart = lineEnd;
        }
    }
}

public class FrenetFrame
{
    public Vector3 tangent;
    public Vector3 normal;
    public Vector3 binormal;

    public FrenetFrame(Vector3 tangent, Vector3 normal, Vector3 binormal)
    {
        this.tangent = tangent;
        this.normal = normal;
        this.binormal = binormal;
    }

    public override string ToString()
    {
        return "FrenetFrame>   tangent: " + tangent + "   normal: " + normal + "   binormal: " + binormal;
    }
}
