﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour
{
    private static CellController instance;
    public static CellController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CellController>();
            }
            return instance;
        }
    }

    #region Behavior

    [SerializeField]
    private CellBehavior behavior;

    [SerializeField]
    private Cell cellPrefab;
    [SerializeField]
    private int cellNum = 3;
    public List<Cell> cells;

    [Range(0.01f, 3f)]
    public float density = 0.1f;

    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 5f)]
    public float avoidanceRadius = 0.5f;
    [Range(0.1f, 10f)]
    public float velocityMultiplier = 10f;
    [Range(0.1f, 10f)]
    public float maxSpeed = 5f;
    private float squareMaxSpeed;
    public float squareAvoidanceRadius;
    private float squareNeighborRadius;
    #endregion
    #region Emotion
    [SerializeField]
    private CellEmotion emotion;
    private int emotionIndex;
    #endregion

    void Start()
    {
        instance = this;
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = avoidanceRadius * avoidanceRadius;
        cells = new List<Cell>();
        emotion.InitEmotions();
        InitCells();
    }

    void Update()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = avoidanceRadius * avoidanceRadius;

        for (int i = 0; i < cells.Count; i++)
        {
            Cell cell = cells[i];
            List<Transform> context = GetNeighbors(cell);
            Vector3 velocity = behavior.CalculateMove(cell, context);
            velocity *= velocityMultiplier;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            cell.Move(velocity);
            cell.PlayEmotionAnim(emotion.GetEmotion(cell.emotionPickInterval));
        }
    }

    private void InitCells()
    {
        for (int i = 0; i < cellNum; i++)
        {
            Cell cell = Instantiate(
                cellPrefab,
                Random.insideUnitSphere * cellNum * density,
                Random.rotation);

            cells.Add(cell);
        }
    }

    private List<Transform> GetNeighbors(Cell cell)
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(cell.transform.position, neighborRadius);

        for (int i = 0; i < contextColliders.Length; i++)
        {
            var curr = contextColliders[i];
            if (curr != cell.CellCollider)
            {
                context.Add(curr.transform);
            }
        }
        return context;
    }
}
