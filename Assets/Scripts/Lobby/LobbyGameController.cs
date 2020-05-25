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

    [SerializeField]
    private Transform pathBegin;
    [SerializeField]
    private Transform pathEnd;
    public Transform gameStartPosition;
    public Transform characterSelectionBase;

    #region camera position
    [SerializeField]
    private List<Transform> cameraPosTransfroms;
    private List<Vector3> cameraPositions;
    private CatmullRomCurve curve;
    [SerializeField]
    private List<Transform> cameraPosTransformsCellSelected;
    private List<Vector3> cameraPositionsCellSelected;
    private CatmullRomCurve curveCellSelected;
    private GameObject cam;
    private Vector3 offset;
    #endregion

    private int currentIndex = 0;
    private LobbyCell currentCell = null;

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
        currentCell = threeCellTypes[currentIndex];
        // need to switch cells fast
        currentCell.cellState = LobbyCellState.IDLE;
        currentCell.speed = 0.25f;

        currentIndex = (currentIndex + step) % threeCellTypes.Length;
        if (currentIndex < 0) currentIndex = threeCellTypes.Length - 1;
        currentCell = threeCellTypes[currentIndex];
        currentCell.cellState = LobbyCellState.SELECTABLE;
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

        cameraPositionsCellSelected = new List<Vector3>();
        for (int i = 0; i < cameraPosTransformsCellSelected.Count; i++)
        {
            cameraPositionsCellSelected.Add(cameraPosTransformsCellSelected[i].position);
        }
        curveCellSelected = new CatmullRomCurve(cameraPositionsCellSelected);

        cam = Camera.main.gameObject;
    }

    public void LoadGame()
    {
        SceneController.Instance.LoadGameScene();
    }

    public void SelectCell()
    {
        StartCoroutine(moveCameraWhenCellSelected());
    }

    public void MoveCameraFollowCell()
    {
        cam.transform.position = Vector3.Lerp(cam.transform.position, currentCell.transform.position - offset, Time.deltaTime * 1.75f);
        Vector3 forward = currentCell.transform.position - cam.transform.position;
        if (forward != Vector3.zero) cam.transform.forward = forward;
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

    private IEnumerator moveCameraWhenCellSelected()
    {
        float tick = 0;
        float totalDuration = 1f;
        float speed = 2f;

        while (tick < totalDuration)
        {
            cam.transform.position = curveCellSelected.GetPointAt(tick);
            cam.transform.LookAt(currentCell.transform.position);
            tick += Time.deltaTime * speed;
            yield return null;
        }

        offset = currentCell.transform.position - cam.transform.position;
        yield return new WaitForSeconds(1f);
        currentCell.cellState = LobbyCellState.SELECTED;
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

        Gizmos.color = Color.gray;
        Vector3 lineStart2 = curveCellSelected.GetPointAt(0f);
        int lineSteps2 = 20;
        for (int i = 1; i <= lineSteps2; i++)
        {
            Vector3 lineEnd = curveCellSelected.GetPointAt(i / (float)lineSteps2);
            Gizmos.DrawLine(lineStart2, lineEnd);
            lineStart2 = lineEnd;
        }
    }
}
