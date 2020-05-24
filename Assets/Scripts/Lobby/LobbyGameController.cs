using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyGameController : MonoBehaviour
{
    [SerializeField]
    private int cellNums = 10;
    [SerializeField]
    private LobbyCell[] lobbyCellPrefabs;

    public Transform pathBegin;
    public Transform pathEnd;

    private static LobbyGameController instance;
    public static LobbyGameController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LobbyGameController>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
        InitCells();
    }

    private void InitCells()
    {
        StartCoroutine(InitCellsWithInterval());
    }

    private IEnumerator InitCellsWithInterval()
    {
        for (int i = 0; i < cellNums; i++)
        {
            Instantiate(lobbyCellPrefabs[i % lobbyCellPrefabs.Length], GetStartPosition(), Quaternion.identity);
            yield return new WaitForSeconds(i % lobbyCellPrefabs.Length);
        }
    }

    public Vector3 GetStartPosition()
    {
        return pathBegin.position + Random.insideUnitSphere * 0.45f;
    }

    public Vector3 GetEndPosition()
    {
        return pathEnd.position + Random.insideUnitSphere * 0.45f;
    }
}
