using System.Collections;
using UnityEngine;

public class OxygenHolder : MonoBehaviour
{
    public bool isOccupied;
    public Vector3 attachPoint;
    private float maxHeight = 0.15f;
    private float tick = 0f;
    private float speed = 0.5f;

    private void Update()
    {
        if (isOccupied)
        {
            tick += Time.deltaTime;
            float d = Mathf.Sin(tick * speed);
            attachPoint = transform.position + transform.up * d * Mathf.Sign(d) * maxHeight;
        }
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
