﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTissueGenerator : MonoBehaviour
{
    public float radius = 12;
    [SerializeField]
    private int tissueGroupNum = 5;
    [HideInInspector]
    public List<BodyTissueGroup> bodyTissueGroups;
    [HideInInspector]
    public List<BodyTissue> bodyTissues;
    [SerializeField]
    private BodyTissueGroup bodyTissueGroupPrefab;
    public BodyTissue bodyTissuePrefab;
    [HideInInspector]
    public Transform center;

    // add nodes here if you want to avoid spawning tissues around tunnel
    [SerializeField]
    private Node[] tunnelNodesToAvoid;

    private static BodyTissueGenerator instance;
    public static BodyTissueGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BodyTissueGenerator>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
        center = transform;
        bodyTissueGroups = new List<BodyTissueGroup>();
        bodyTissues = new List<BodyTissue>();
        SpawnTissueGroup();
    }

    private void SpawnTissueGroup()
    {
        for (int i = 0; i < tissueGroupNum; i++)
        {
            Vector3 groupCenter = GetRandomPointOnSphereWithWeight();
            BodyTissueGroup group = Instantiate(bodyTissueGroupPrefab, groupCenter, Quaternion.identity);
            bodyTissueGroups.Add(group);
            for (int j = 0; j < group.BodyTissues.Count; j++)
            {
                bodyTissues.Add(group.BodyTissues[j]);
            }
            Vector3 forward = center.position - groupCenter;
            if (forward != Vector3.zero)
            {
                group.transform.forward = forward;
            }
        }
    }

    private Vector3 GetRandomPointOnSphere()
    {
        return center.position + Random.onUnitSphere * radius;
    }

    private float ApplyWeightOnPoint(Vector3 point)
    {
        // TODO - weight logic goes here
        // Thinking of making a texture to reference weight?
        // for now, there's no weight, and always return 1
        return 1f;
    }

    private Vector3 GetRandomPointOnSphereWithWeight()
    {
        Vector3 point = GetRandomPointOnSphere();
        for (int i = 0; i < tunnelNodesToAvoid.Length; i++)
        {
            var node = tunnelNodesToAvoid[i];
            float avoidRad = PathBuilder.Instance.radius * 2f;
            if ((node.transform.position - point).sqrMagnitude
                < avoidRad * avoidRad)
            {
                point = GetRandomPointOnSphereWithWeight();
            }
        }

        while (ApplyWeightOnPoint(point) < Random.Range(0, 1))
        {
            point = GetRandomPointOnSphereWithWeight();
        }
        return point;
    }

    public Vector3 GetRandomPositionInTheBodyTissueArea()
    {
        return center.position + Random.insideUnitSphere * radius;
    }
}
