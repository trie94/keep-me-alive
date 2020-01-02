using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject playerCell;
    private GameObject player;
    [SerializeField]
    private float zOffset = 2f;
    private Vector3 offset;
    private Camera mainCam;

    private void Awake()
    {
        player = Instantiate(playerCell);
        mainCam = Camera.main;
        offset = player.transform.forward * zOffset;
        player.transform.position = mainCam.transform.position - offset;
    }

    private void LateUpdate()
    {
        offset = player.transform.forward * zOffset;
        mainCam.transform.position = player.transform.position - offset;
        mainCam.transform.LookAt(player.transform);
    }
}
