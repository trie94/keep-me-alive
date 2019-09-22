using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Cell : MonoBehaviour
{
    private Collider cellCollider;
    public Collider CellCollider
    {
        get { return cellCollider; }
    }
    private Vector3 vel;

    private void Awake()
    {
        cellCollider = GetComponent<Collider>();
    }

    public void Move(Vector3 velocity)
    {
        // vel += force * Time.deltaTime;
        // vel = Vector3.ClampMagnitude(force, CellController.Instance.maxSpeed);
        if (velocity != Vector3.zero)
        {
            // vel = Vector3.Lerp(vel, velocity, 0.2f);
            vel = velocity;
        }
        transform.position += vel * Time.deltaTime;
        transform.up = vel;
    }

    void OnDrawGizmos()
    {
        // Debug.Log("gizmo");
        Gizmos.DrawLine(transform.position, transform.position + vel);
    }
}
