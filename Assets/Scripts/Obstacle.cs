using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Collider col;
    public Collider Col { get { return col; } }

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void Start()
    {
        CellController.Instance.RegisterObstacles(col);
    }
}
