using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateletLimb : MonoBehaviour
{
    private float rotationProgress = 0f;
    private Quaternion targetRotation = Quaternion.identity;

    private float speed;

    private void Start()
    {
        speed = Random.Range(0.05f, 0.1f);
        UpdateTargetRotation();
    }

    private void Update()
    {
        if (rotationProgress > 1f)
        {
            UpdateTargetRotation();
        }

        UpdateRotation();
    }

    private void UpdateTargetRotation()
    {
        rotationProgress = 0;
        Vector3 direction = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        targetRotation = Quaternion.Euler(direction);
    }

    private void UpdateRotation()
    {
        rotationProgress += Time.deltaTime * speed;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationProgress);
    }
}
