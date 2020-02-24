using System.Collections;
using UnityEngine;

public class OxygenHolder : MonoBehaviour
{
    public bool isOccupied;
    public Vector3 attachPoint;
    private float height = 0.5f;

    private void Update()
    {
        attachPoint = transform.position + this.transform.up * height;
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
