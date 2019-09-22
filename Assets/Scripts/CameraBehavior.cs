using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    Vector3 offset;

    void Start()
    {
        offset = this.transform.position - Vector3.zero;
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
        this.transform.position = CellController.Instance.cells[0].transform.position;
        this.transform.LookAt(targetPos);
    }
}
