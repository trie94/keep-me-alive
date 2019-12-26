using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    Vector3 offset;
    [SerializeField]
    private GameObject player;

    private void Start()
    {
        offset = this.transform.position - Vector3.zero;
    }

    private void LateUpdate()
    {
        // this.transform.position = targetPos + offset;
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
    }
}
