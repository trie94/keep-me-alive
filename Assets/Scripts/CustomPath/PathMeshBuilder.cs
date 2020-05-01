using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMeshBuilder : MonoBehaviour
{
    private int tubularSegments = 20;
    private int radialSegments = 8;
    private float radius = 5;
    [SerializeField]
    private GameObject tunnelPrefab;
    private List<GameObject> tunnels = new List<GameObject>();
    private List<Vector3> debugVertices = new List<Vector3>();

    private void Awake()
    {
        for (int i=0; i<Path.Instance.segments.Count; i++)
        {
            tunnels.Add(Instantiate(tunnelPrefab));
        }
    }

    private void Update()
    {
        for (int i = 0; i < Path.Instance.segments.Count; i++)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var frames = ComputeFrenetFrames(Path.Instance.segments[i], tubularSegments+1);

            for (int j = 0; j <= tubularSegments; j++)
            {
                GenerateSubSegments(Path.Instance.segments[i], frames, vertices, normals, tangents, j);
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

            tunnels[i].GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.tangents = tangents.ToArray();
            mesh.uv = uvs.ToArray();
            // we need back rendering
            triangles.Reverse();
            mesh.triangles = triangles.ToArray();
        }
    }

    private void GenerateSubSegments(
        Segment segment, FrenetFrame[] frames, List<Vector3> vertices, List<Vector3> normals, List<Vector4> tangents, int index)
    {
        // 0~1
        var u = 1f * index / tubularSegments;
        var p = Path.Instance.GetPoint(segment, u);
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

    private FrenetFrame[] ComputeFrenetFrames(Segment segment, int num)
    {
        var frames = new FrenetFrame[num];
        for (int i = 1; i < num + 1; i++)
        {
            float prevStep = 1f * (i - 1) / num;
            float currentStep = 1f * i / num;

            Vector3 prevCenter = Path.Instance.GetPoint(segment, prevStep);
            Vector3 currentCenter = Path.Instance.GetPoint(segment, currentStep);
            Vector3 tangent = (prevCenter - currentCenter).normalized;
            FrenetFrame frame = new FrenetFrame(currentCenter, tangent, Vector3.up, Vector3.Cross(Vector3.up, tangent));
            frames[i - 1] = frame;
        }
        return frames;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < debugVertices.Count; i++)
        {
            DrawGizmo(debugVertices[i]);
        }
    }

    private void DrawGizmo(FrenetFrame frame)
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(frame.center, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(frame.center, frame.center + frame.binormal);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(frame.center, frame.center + frame.tangent);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(frame.center, frame.center + frame.normal);
    }

    private void DrawGizmo(Vector3 point)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(point, 0.1f);
    }
}

public class FrenetFrame
{
    public Vector3 center;
    public Vector3 tangent;
    public Vector3 normal;
    public Vector3 binormal;

    public FrenetFrame(Vector3 center, Vector3 tangent, Vector3 normal, Vector3 binormal)
    {
        this.center = center;
        this.tangent = tangent;
        this.normal = normal;
        this.binormal = binormal;
    }

    public override string ToString()
    {
        return "FrenetFrame>   center: " + center + "   tangent: " + tangent + "   normal: " + normal + "   binormal: " + binormal;
    }
}