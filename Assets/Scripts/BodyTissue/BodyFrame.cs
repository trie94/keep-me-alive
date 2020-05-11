using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyFrame : MonoBehaviour
{
    public Vector3 force;

    public void ComputeAlignment(BodyFrame front, BodyFrame back)
    {
        Vector3 forward = Vector3.Lerp(front.transform.forward, back.transform.forward, 0.5f);
        if (forward != Vector3.zero) transform.forward = forward;
    }

    public void ComputeForce(Transform root, Transform target)
    {
        Vector3 force = target.position - root.position;
    }
}
