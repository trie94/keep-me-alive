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

    public List<Cell> entireCells;
    public List<Cell> erythrocytes;
    public List<Cell> leukocytes;
    public List<Cell> platelets;
    // this is internal helper
    private Dictionary<CellType, List<Cell>> cellDictionary;
    private float radius = 3f;

    private void Awake()
    {
        instance = this;
        entireCells = new List<Cell>();
        erythrocytes = new List<Cell>();
        leukocytes = new List<Cell>();
        platelets = new List<Cell>();

        cellDictionary = new Dictionary<CellType, List<Cell>>();
        cellDictionary.Add(CellType.Erythrocyte, erythrocytes);
        cellDictionary.Add(CellType.Leukocyte, leukocytes);
        cellDictionary.Add(CellType.Platelet, platelets);
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
            typeIndex = (i % 5 == 0) ? (int)CellType.Leukocyte : (int)CellType.Erythrocyte;
            typeIndex = (i % 7 == 0 && i % 5 != 0) ? (int)CellType.Platelet : typeIndex;

            Cell cell = Instantiate(cellPrefabs[typeIndex]);
            cellDictionary[(CellType)typeIndex].Add(cell);
            entireCells.Add(cell);
        }
    }

    public Vector3 GetRandomPositionInOxygenArea()
    {
        return Path.Instance.OxygenZone.transform.position
                         + Random.insideUnitSphere * radius;
    }

    public BodyTissue GetTargetBodyTissue()
    {
        List<BodyTissueGroup> groups = BodyTissueGenerator.Instance.bodyTissueGroups;
        // first fit
        for (int i=0; i<groups.Count; i++)
        {
            var currGroup = groups[i];
            for (int j=0; j< currGroup.BodyTissues.Count; j++)
            {
                var currTissue = currGroup.BodyTissues[j];
                if (currTissue.NeedOxygen())
                {
                    return currTissue;
                }
            }
        }

        return null;
    }
}
