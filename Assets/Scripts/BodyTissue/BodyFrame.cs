using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyFrame : MonoBehaviour
{
    public Vector3 acceleration;
    public Vector3 velocity;
    public float mass = 1f;

    public void ComputeAlignment(BodyFrame front, BodyFrame back)
    {
        Vector3 forward = Vector3.Lerp(front.transform.forward, back.transform.forward, 0.5f);
        if (forward != Vector3.zero) transform.forward = forward;
    }

    public void UpdateVelocity()
    {
        velocity = acceleration * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
    }
}
