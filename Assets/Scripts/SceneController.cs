using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private static SceneController instance;
    public static SceneController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneController>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public void LoadGameScene()
    {
        StartCoroutine(LoadGameSceneAsync());
    }

    IEnumerator LoadGameSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Cell");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        TransitionManager.Instance.RevealGame();
    }
}
