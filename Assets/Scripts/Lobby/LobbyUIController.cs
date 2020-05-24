using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LobbyViews
{
    TITLE, CHARACTER_SELECTION, SETTINGS, CREDITS
}

public class LobbyUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject title;
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button settingsButton;

    [SerializeField]
    private Button creditsButton;
    [SerializeField]
    private Button selectButton;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private GameObject titleMenuButtons;
    [SerializeField]
    private GameObject characterSelectionMenuButtons;

    #region camera position
    [SerializeField]
    private List<Transform> cameraPosTransfroms;
    private List<Vector3> cameraPositions;
    private CatmullRomCurve curve;
    private GameObject cam;
    #endregion

    private LobbyViews currentView = LobbyViews.TITLE;

    private static LobbyUIController instance;
    public static LobbyUIController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LobbyUIController>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
        SetUpCameraPositions();
        InitButtons();
        currentView = LobbyViews.TITLE;
        ToggleView();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    public void OnClickStart()
    {
        // show character selection scene
        MoveCameraBetween();
        currentView = LobbyViews.CHARACTER_SELECTION;
        ToggleView();
    }

    public void OnClickSettings()
    {
        currentView = LobbyViews.SETTINGS;
        ToggleView();
    }

    public void OnClickCredits()
    {
        currentView = LobbyViews.CREDITS;
        ToggleView();
    }

    public void OnClickBackButton()
    {
        MoveCameraBetween();
        currentView = LobbyViews.TITLE;
        ToggleView();
    }

    public void onClickExit()
    {
        Application.Quit();
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

    private void MoveCameraBetween()
    {
        StartCoroutine(moveCamera());
    }

    private IEnumerator moveCamera()
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

    private void OnClickSelectCharacter()
    {
        // for now we just load the scene
        SceneController.Instance.LoadGameScene();
    }

    private void InitButtons()
    {
        startButton.onClick.AddListener(() => OnClickStart());
        settingsButton.onClick.AddListener(() => OnClickSettings());
        creditsButton.onClick.AddListener(() => OnClickCredits());
        backButton.onClick.AddListener(() => OnClickBackButton());
        selectButton.onClick.AddListener(() => OnClickSelectCharacter());
    }

    private void RemoveListeners()
    {
        if (startButton != null) startButton.onClick.RemoveListener(() => OnClickStart());
        if (settingsButton != null) settingsButton.onClick.RemoveListener(() => OnClickSettings());
        if (creditsButton != null) creditsButton.onClick.RemoveListener(() => OnClickCredits());
        if (backButton != null) backButton.onClick.RemoveListener(() => OnClickBackButton());
        if (selectButton != null) selectButton.onClick.RemoveListener(() => OnClickSelectCharacter());
    }

    private void ToggleView()
    {
        switch (currentView)
        {
            case LobbyViews.TITLE:
                title.gameObject.SetActive(true);
                titleMenuButtons.SetActive(true);
                backButton.gameObject.SetActive(false);
                characterSelectionMenuButtons.SetActive(false);
                break;

            case LobbyViews.CHARACTER_SELECTION:
                title.gameObject.SetActive(false);
                titleMenuButtons.SetActive(false);
                backButton.gameObject.SetActive(true);
                characterSelectionMenuButtons.SetActive(true);
                break;

            case LobbyViews.SETTINGS:
                title.gameObject.SetActive(false);
                titleMenuButtons.SetActive(false);
                backButton.gameObject.SetActive(true);
                characterSelectionMenuButtons.SetActive(false);
                break;

            case LobbyViews.CREDITS:
                title.gameObject.SetActive(false);
                titleMenuButtons.SetActive(false);
                backButton.gameObject.SetActive(true);
                characterSelectionMenuButtons.SetActive(false);
                break;
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
