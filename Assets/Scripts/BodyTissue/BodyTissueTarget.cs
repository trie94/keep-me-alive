using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTissueTarget : MonoBehaviour
{
    public Vector3 originalPosition;

    private float movementRadius = 5f;
    private float tick = 0f;
    private float randomPickInterval = 5f;
    public Transform targetToFollow;
    private Vector3 randomPoint;

    private void Start()
    {
        originalPosition = transform.position;
        randomPickInterval = Random.Range(1f, 4f);
        randomPoint = originalPosition;
    }

    private void Update()
    {
        if (targetToFollow)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetToFollow.position, 0.1f);
        }
        else
        {
            // random movement
            if (tick > randomPickInterval)
            {
                randomPickInterval = Random.Range(1f, 4f);
                randomPoint = GetRandomPoint();
                tick = 0f;
            }
            transform.position = Vector3.MoveTowards(transform.position, randomPoint, 0.05f);
            tick += Time.deltaTime;
        }
    }

    private Vector3 GetRandomPoint()
    {
        return originalPosition + Random.insideUnitSphere * movementRadius;
    }
}
