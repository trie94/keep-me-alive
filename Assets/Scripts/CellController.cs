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

    [SerializeField]
    private int cellNum = 3;
    public Dictionary<Transform, Cell> cellMap;
    public HashSet<Cell> cells;

    #region targets
    public Transform oxygenExitNode;
    public Transform heartExitNode;
    public Transform heart;
    public Transform oxygenArea;
    #endregion

    private float radius = 3f;

    public bool debugMode = true;

    private void Awake()
    {
        instance = this;
        cells = new HashSet<Cell>();
        cellMap = new Dictionary<Transform, Cell>();
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
            cellMap.Add(cell.transform, cell);
            cells.Add(cell);
        }
    }

    public Vector3 GetRandomPositionInOxygenArea()
    {
        return oxygenArea.position
                         + Random.insideUnitSphere * radius;
    }

    public Vector3 GetRandomPositionInHeartArea()
    {
        return heart.position
                    + Random.insideUnitSphere * radius;
    }
}
