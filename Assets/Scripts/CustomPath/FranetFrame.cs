using UnityEngine;

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