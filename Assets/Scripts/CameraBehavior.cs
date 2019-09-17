using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField]
    Transform target;
    Vector3 offset;

    void Start()
    {
        offset = target.position - this.transform.position;
    }

    void LateUpdate()
    {
        this.transform.position = target.transform.position + offset;
        this.transform.LookAt(target);
    }
}
