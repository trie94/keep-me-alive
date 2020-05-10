using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTissueGroup : MonoBehaviour
{
    private List<BodyTissue> bodyTissues;
    public List<BodyTissue> BodyTissues { get { return bodyTissues; } }
    private float radius;
    private int bodyTissueNum;

    private void Awake()
    {
        bodyTissues = new List<BodyTissue>();
        radius = Random.Range(1f, 3f);
        bodyTissueNum = Random.Range(3, 7);
        SpawnTissues();
    }

    private void SpawnTissues()
    {
        for (int i = 0; i < 1; i++)
        {
            BodyTissue tissue = Instantiate(BodyTissueGenerator.Instance.bodyTissuePrefab);
            tissue.transform.position = GetRandomPositionWithinRadius();
            CheckCollision(tissue, bodyTissues, 0);
            bodyTissues.Add(tissue);

            Vector3 forward = BodyTissueGenerator.Instance.center.position
                - tissue.transform.position;
            if (forward != Vector3.zero)
            {
                tissue.transform.forward = forward;
            }
        }
    }

    private void CheckCollision(BodyTissue tissue, List<BodyTissue> tissues, int iteration)
    {
        // avoid too many recursive calls
        if (iteration > 10) return;
        float avoidRad = 1f;
        for (int i = 0; i < tissues.Count; i++)
        {
            BodyTissue comp = bodyTissues[i];
            if ((tissue.transform.position - comp.transform.position).sqrMagnitude < avoidRad)
            {
                tissue.transform.position = GetRandomPositionWithinRadius();
                CheckCollision(tissue, tissues, iteration + 1);
            }
        }
    }

    private Vector3 GetRandomPositionWithinRadius()
    {
        float offset = 0.5f;
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * radius;
        Vector3 roomCenterToPoint = randomPoint - BodyTissueGenerator.Instance.center.position;
        return roomCenterToPoint.normalized * (BodyTissueGenerator.Instance.radius + offset)
            + BodyTissueGenerator.Instance.center.position;
    }
}