using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject playerCell;
    private GameObject player;
    [SerializeField]
    private float zOffset = -2f;
    [SerializeField]
    private float yOffset = 3f;
    private Vector3 offset;
    private Camera mainCam;

    private void Awake()
    {
        player = Instantiate(playerCell);
        mainCam = Camera.main;
        offset = player.transform.forward * zOffset + player.transform.up * yOffset;
        mainCam.transform.position = player.transform.position + offset;
    }

    private void LateUpdate()
    {
        CameraFollowPlayer();
    }

    private void CameraFollowPlayer()
    {
        offset = player.transform.forward * zOffset + player.transform.up * yOffset;
        mainCam.transform.position = player.transform.position + offset;

        Vector3 target = player.transform.position + player.transform.up * yOffset / 2f;
        Vector3 forward = target - mainCam.transform.position;
        if (forward != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(forward, player.transform.up);
            mainCam.transform.rotation = look;
        }
    }
}
