using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    private static TransitionManager instance;
    public static TransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TransitionManager>();
            }
            return instance;
        }
    }

    [SerializeField]
    private Renderer transitionQuadRend;
    private int alphaId;
    [SerializeField]
    private GameObject canvas;

    private void Awake()
    {
        alphaId = Shader.PropertyToID("_Alpha");
        transitionQuadRend.gameObject.SetActive(true);
        transitionQuadRend.material.SetFloat(alphaId, 1f);
        canvas.SetActive(false);
    }

    public void RevealGame()
    {
        StartCoroutine(RevealGameView());
    }

    private IEnumerator RevealGameView()
    {
        float a = 1;
        float duration = 2f;
        float tick = 0;
        yield return new WaitForEndOfFrame();

        while (tick < duration)
        {
            transitionQuadRend.material.SetFloat(alphaId, a);
            tick += Time.deltaTime;
            a = 1f - tick / duration;
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        transitionQuadRend.gameObject.SetActive(false);
        canvas.SetActive(true);
    }
}
