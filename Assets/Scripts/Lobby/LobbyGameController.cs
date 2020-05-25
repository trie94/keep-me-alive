using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyGameController : MonoBehaviour
{
    [SerializeField]
    private int cellNums = 10;
    [SerializeField]
    private LobbyCell[] lobbyCellPrefabs;
    private LobbyCell[] threeCellTypes;

    public Transform pathBegin;
    public Transform pathEnd;
    public Transform characterSelectionBase;

    #region camera position
    [SerializeField]
    private List<Transform> cameraPosTransfroms;
    private List<Vector3> cameraPositions;
    private CatmullRomCurve curve;
    private GameObject cam;
    #endregion

    private int currentIndex = 0;

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
        SetUpCameraPositions();
        SpawnSpecialThreeCellsForSelection();
        InitCells();
    }

    public Vector3 GetStartPosition()
    {
        return pathBegin.position + Random.insideUnitSphere * 0.45f;
    }

    public Vector3 GetEndPosition()
    {
        return pathEnd.position + Random.insideUnitSphere * 0.45f;
    }

    public void GetCorrectCellCharacter(int step)
    {
        Debug.Log(currentIndex);
        // need to switch cells fast
        threeCellTypes[currentIndex].idle = true;
        threeCellTypes[currentIndex].speed = 0.5f;

        currentIndex = (currentIndex + step) % threeCellTypes.Length;
        if (currentIndex < 0) currentIndex = threeCellTypes.Length - 1;
        threeCellTypes[currentIndex].idle = false;
        ToggleSelectButton();
    }

    private void ToggleSelectButton()
    {
        LobbyUIController.Instance.ToggleSelectCharacter(currentIndex == 0);
    }

    private void SpawnSpecialThreeCellsForSelection()
    {
        threeCellTypes = new LobbyCell[3];
        for (int i = 0; i < 3; i++)
        {
            threeCellTypes[i] = Instantiate(lobbyCellPrefabs[i], GetStartPosition(), Quaternion.identity);
        }
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

    private void SetUpCameraPositions()
    {
        cameraPositions = new List<Vector3>();
        for (int i = 0; i < cameraPosTransfroms.Count; i++)
        {
            cameraPositions.Add(cameraPosTransfroms[i].position);
        }
        curve = new CatmullRomCurve(cameraPositions);
        cam = Camera.main.gameObject;
    }

    public void MoveCameraBetween(LobbyViews currentView)
    {
        StartCoroutine(moveCamera(currentView));
    }

    private IEnumerator moveCamera(LobbyViews currentView)
    {
        float tick = 0;
        float totalDuration = 1f;
        float speed = 2f;

        if (currentView == LobbyViews.TITLE)
        {
            while (tick < totalDuration)
            {
                cam.transform.position = curve.GetPointAt(tick);
                tick += Time.deltaTime * speed;
                yield return null;
            }
        }
        else if (currentView == LobbyViews.CHARACTER_SELECTION)
        {
            while (tick < totalDuration)
            {
                cam.transform.position = curve.GetPointAt(totalDuration - tick);
                tick += Time.deltaTime * speed;
                yield return null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        if (curve == null) return;
        Gizmos.color = Color.white;
        Vector3 lineStart = curve.GetPointAt(0f);
        int lineSteps = 20;
        for (int i = 1; i <= lineSteps; i++)
        {
            Vector3 lineEnd = curve.GetPointAt(i / (float)lineSteps);
            Gizmos.DrawLine(lineStart, lineEnd);
            lineStart = lineEnd;
        }
    }
}
