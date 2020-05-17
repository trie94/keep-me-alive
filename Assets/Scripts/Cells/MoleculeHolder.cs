using System.Collections;
using UnityEngine;

public class MoleculeHolder : MonoBehaviour
{
    public bool isOccupied;
    public Vector3 attachPoint;
    private float maxHeight = 0.15f;
    private float tick = 0f;
    private float speed = 0.5f;

    [HideInInspector]
    public Transform cell;

    private float offset;
    private float noiseFreq;
    private float noiseScale;
    private float noiseHeight;
    private Vector3 position;
    private Vector3 cellPrevPosition;

    private void Start()
    {
        position = transform.localPosition;
        cellPrevPosition = cell.position;
        // Get renderer and material from the parent
        Renderer[] rends = GetComponentsInParent<Renderer>();
        Renderer rend = null;
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] != GetComponent<Renderer>())
            {
                rend = rends[i];
                break;
            }
        }

        offset = rend.material.GetFloat("_Offset");
        noiseFreq = rend.material.GetFloat("_NoiseFreq");
        noiseScale = rend.material.GetFloat("_NoiseScale");
        noiseHeight = rend.material.GetFloat("_NoiseHeight");
    }

    private void LateUpdate()
    {
        Vector3 worldPosition = cell.TransformPoint(position);
        Vector3 localOffset = cell.position - worldPosition;

        Vector3 velocity = (cell.position - cellPrevPosition) / Time.deltaTime;
        Vector3 normalizedVelocity = (velocity.magnitude == 0) ? Vector3.zero : velocity.normalized;
        float dirDot = Mathf.Abs(Vector3.Dot(normalizedVelocity, localOffset.normalized) + offset);
        Vector3 smearOffset = velocity * (dirDot + SNoise.snoise(worldPosition * noiseFreq) * noiseScale) * noiseHeight;

        worldPosition -= smearOffset;
        transform.position = worldPosition;

        if (isOccupied)
        {
            tick += Time.deltaTime;
            float d = Mathf.Sin(tick * speed);
            attachPoint = transform.position + transform.up * d * Mathf.Sign(d) * maxHeight;
        }

        cellPrevPosition = cell.position;
    }

    public void Occupy()
    {
        isOccupied = true;
        speed = Random.Range(0.5f, 1f);
        attachPoint = transform.position;
    }

    public void Reset()
    {
        isOccupied = false;
    }

    private void OnDrawGizmos()
    {
        if (isOccupied)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, attachPoint);
        }
    }
}
