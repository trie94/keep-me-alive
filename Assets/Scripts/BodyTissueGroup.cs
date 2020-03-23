using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTissueGroup : MonoBehaviour
{
    private List<BodyTissue> bodyTissues;
    private float radius;
    private int bodyTissueNum;

    private void Awake()
    {
        bodyTissues = new List<BodyTissue>();
        radius = Random.Range(1f, 3f);
        bodyTissueNum = Random.Range(3, 7);
        SpawnTissues();
    }

    // when spawn, there should be no collision between tissues
    private void SpawnTissues()
    {
        for (int i = 0; i < bodyTissueNum; i++)
        {
            BodyTissue tissue = Instantiate(
                BodyTissueGenerator.Instance.bodyTissuePrefab);
            bodyTissues.Add(tissue);
            tissue.transform.position = GetRandomPositionWithinRadius();
            Vector3 forward = BodyTissueGenerator.Instance.center.position
                - tissue.transform.position;
            if (forward != Vector3.zero)
            {
                tissue.transform.forward = forward;
            }
        }
    }

    private Vector3 GetRandomPositionWithinRadius()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * radius;
        Vector3 roomCenterToPoint = randomPoint - BodyTissueGenerator.Instance.center.position;
        return roomCenterToPoint.normalized * BodyTissueGenerator.Instance.radius
            + BodyTissueGenerator.Instance.center.position;
    }
}