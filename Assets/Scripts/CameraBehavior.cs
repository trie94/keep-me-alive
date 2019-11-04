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
        RenderSettings.fogColor = Camera.main.backgroundColor;
    }

    private void LateUpdate()
    {
        if (CellController.Instance == null || CellController.Instance.cells == null || CellController.Instance.cells.Count <= 0) return;
        // this.transform.position = targetPos + offset;
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
    }
}
