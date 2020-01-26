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
    private bool isEditor;

    private void Awake()
    {
        player = Instantiate(playerCell);
        mainCam = Camera.main;
        offset = player.transform.forward * zOffset + player.transform.up * yOffset;
        mainCam.transform.position = player.transform.position + offset;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        isEditor = true;
#else
        isEditor = false;
#endif
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
    }

    private void LateUpdate()
    {
        CameraFollowPlayer();
    }

    private void CameraFollowPlayer()
    {
        offset = player.transform.forward * zOffset + player.transform.up * yOffset;
        mainCam.transform.position = player.transform.position + offset;
        //mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, player.transform.position + offset, 0.1f);

        Vector3 target = player.transform.position + player.transform.up * yOffset / 2f;
        Quaternion look = Quaternion.LookRotation(target - mainCam.transform.position, player.transform.up);
        //mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, look, 0.1f);
        mainCam.transform.rotation = look;
    }
}
