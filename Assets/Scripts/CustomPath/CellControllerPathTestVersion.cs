using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellControllerPathTestVersion : MonoBehaviour
{
    [SerializeField]
    private GameObject cellPrefab;
    [SerializeField]
    private int cellNum = 10;
    private GameObject[] cells;

    private static CellControllerPathTestVersion instance;
    public static CellControllerPathTestVersion Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CellControllerPathTestVersion>();
            }
            return instance;
        }
    }

    private void Start()
    {
        if (cellPrefab == null) return;
        InitCells();
    }

    private void InitCells()
    {
        cells = new GameObject[cellNum];
        for (int i = 0; i < cellNum; i++)
        {
            GameObject cell = Instantiate(cellPrefab);
            cells[i] = cell;
        }
    }
}
