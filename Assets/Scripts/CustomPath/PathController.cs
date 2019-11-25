using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathController : MonoBehaviour
{
    [SerializeField]
    private GameObject cellPrefab;
    private GameObject cell;
    private int lineSteps = 10;
    private float progress = 0f;
    private float duration = 10f;
    private int segIndex = 0;

    private void Awake()
    {
        if (cellPrefab == null) return;
        cell = Instantiate(cellPrefab);
    }

    private void Update()
    {
        if (Path.Instance == null || Path.Instance.nodes == null || Path.Instance.segments == null) return;

        progress += Time.deltaTime / duration;
        if (progress > 1f)
        {
            progress = 0f;
            segIndex = (segIndex + 1) % Path.Instance.segments.Length;
        }

        Segment currSeg = Path.Instance.segments[segIndex];

        for (int j = 1; j <= lineSteps; j++)
        {
            Vector3 target = Path.Instance.GetPoint(currSeg, progress);
            Vector3 lookDir = target - cell.transform.position;

            cell.transform.position = target;
            cell.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
    }
}
