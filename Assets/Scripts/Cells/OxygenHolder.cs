using System.Collections;
using UnityEngine;

public class OxygenHolder : MonoBehaviour
{
    public bool isOccupied;
    public Vector3 attachPoint;
    private float maxHeight = 0.15f;
    private float tick = 0f;
    private float speed = 0.5f;

    public Transform cell;
    private float offset = -0.49f;
    private float noiseFreq = 0.612f;
    private float noiseScale = 1.18f;
    private float noiseHeight = 0.087f;
    private Vector3 position;
    private Vector3 cellPrevPosition;

    private void Awake()
    {
        position = transform.localPosition;
        cellPrevPosition = cell.position;
        if (GetComponentInParent<PlayerBehavior>() != null)
        {
            offset = 1f;
            noiseFreq = 0.72f;
            noiseScale = 0.96f;
            noiseHeight = 0.041f;
        }
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

    public void OnOccupied()
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
