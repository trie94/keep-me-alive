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
    #region buttons
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
    [SerializeField]
    private Button characterSelectLeftArrow;
    [SerializeField]
    private Button characterSelectRightArrow;
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
        InitButtons();
        currentView = LobbyViews.TITLE;
        ToggleView();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    public void ToggleSelectCharacter(bool toggle)
    {
        selectButton.gameObject.SetActive(toggle);
    }

    private void OnClickStart()
    {
        // show character selection scene
        LobbyGameController.Instance.MoveCameraBetween(currentView);
        LobbyGameController.Instance.GetCorrectCellCharacter(0);
        currentView = LobbyViews.CHARACTER_SELECTION;
        ToggleView();
    }

    private void OnClickSettings()
    {
        currentView = LobbyViews.SETTINGS;
        ToggleView();
    }

    private void OnClickCredits()
    {
        currentView = LobbyViews.CREDITS;
        ToggleView();
    }

    private void OnClickBackButton()
    {
        LobbyGameController.Instance.MoveCameraBetween(currentView);
        currentView = LobbyViews.TITLE;
        ToggleView();
    }

    private void OnClickCharacterSelectLeftButton()
    {
        LobbyGameController.Instance.GetCorrectCellCharacter(-1);
    }

    private void OnClickCharacterSelectRightButton()
    {
        LobbyGameController.Instance.GetCorrectCellCharacter(1);
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
        characterSelectLeftArrow.onClick.AddListener(() => OnClickCharacterSelectLeftButton());
        characterSelectRightArrow.onClick.AddListener(() => OnClickCharacterSelectRightButton());
    }

    private void RemoveListeners()
    {
        if (startButton != null) startButton.onClick.RemoveListener(() => OnClickStart());
        if (settingsButton != null) settingsButton.onClick.RemoveListener(() => OnClickSettings());
        if (creditsButton != null) creditsButton.onClick.RemoveListener(() => OnClickCredits());
        if (backButton != null) backButton.onClick.RemoveListener(() => OnClickBackButton());
        if (selectButton != null) selectButton.onClick.RemoveListener(() => OnClickSelectCharacter());
        if (characterSelectLeftArrow != null) characterSelectLeftArrow.onClick.RemoveListener(() => OnClickCharacterSelectLeftButton());
        if (characterSelectRightArrow != null) characterSelectRightArrow.onClick.RemoveListener(() => OnClickCharacterSelectRightButton());
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
                selectButton.gameObject.SetActive(false);
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
}
