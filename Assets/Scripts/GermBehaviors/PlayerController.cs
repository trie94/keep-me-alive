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
        offset = player.transform.forward * zOffset + player.transform.up * yOffset;
        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, player.transform.position + offset, 0.1f);

        Vector3 target = player.transform.position + player.transform.up * yOffset/2f;
        Quaternion look = Quaternion.LookRotation(target - mainCam.transform.position, player.transform.up);
        mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, look, 0.1f);
    }
}
