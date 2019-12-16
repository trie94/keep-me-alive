using System.Collections;
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

    [SerializeField]
    private Cell[] cellPrefabs;

    #region Behavior
    // the behavior order should match with the cell state.
    // inVein, EnterOxygen, ExitOxygen, EnterHeart, ExitHeart
    [SerializeField]
    private Movement<Cell>[] behaviors;

    [SerializeField]
    private int cellNum = 3;
    public List<Cell> cells;
    public static HashSet<Collider> obstacles;

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
    public CellEmotion emotions;
    #endregion

    #region
    public Transform oxygenExitNode;
    public Transform heardExitNode;
    #endregion

    public bool debugMode = true;

    private void Awake()
    {
        instance = this;
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = avoidanceRadius * avoidanceRadius;
        cells = new List<Cell>();
        obstacles = new HashSet<Collider>();
    }

    private void Start()
    {
        SpawnCells();
    }

    private void SpawnCells()
    {
        int typeIndex = 0;
        for (int i = 0; i < cellNum; i++)
        {
            typeIndex = (i % 5 == 0) ? 0 : 1;
            typeIndex = (i % 7 == 0) ? 2 : typeIndex;

            Cell cell = Instantiate(cellPrefabs[typeIndex]);
            cells.Add(cell);
        }
    }

    public void RegisterObstacles(Collider obstacle)
    {
        obstacles.Add(obstacle);
        Debug.Log("register: " + obstacle);
    }
}
