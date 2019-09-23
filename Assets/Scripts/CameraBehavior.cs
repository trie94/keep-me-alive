using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    Vector3 offset;

    void Start()
    {
        offset = this.transform.position - Vector3.zero;
        RenderSettings.fogColor = Camera.main.backgroundColor;
    }

    void LateUpdate()
    {
        Vector3 targetPos = Vector3.zero;
        for (int i=0; i<CellController.Instance.cells.Count; i++)
        {
            targetPos += CellController.Instance.cells[i].transform.position;
        }
        targetPos /= CellController.Instance.cells.Count;
        // this.transform.position = targetPos + offset;
        transform.position = CellController.Instance.cells[0].transform.position;
        var targetRotation = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.5f);
    }
}
