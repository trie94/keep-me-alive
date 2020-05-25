using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyTransition : MonoBehaviour
{
    private Renderer transitionQuadRend;
    private int alphaId;

    private void Awake()
    {
        transitionQuadRend = GetComponent<Renderer>();
        alphaId = Shader.PropertyToID("_Alpha");
    }

    public void ConcealLobby()
    {
        StartCoroutine(ConcealLobbyView());
    }

    private IEnumerator ConcealLobbyView()
    {
        float a = 1;
        float duration = 1f;
        float tick = 0;
        yield return new WaitForEndOfFrame();

        while (tick < duration)
        {
            transitionQuadRend.material.SetFloat(alphaId, a);
            tick += Time.deltaTime;
            a = tick / duration;
            yield return null;
        }
    }
}
