using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum WormState
{
    Idle, FindTargetOxygen, EatOxygen
}

[System.Serializable]
public class Worm : Germ
{
    [SerializeField]
    private GameObject worm;
    [SerializeField]
    private float duration;
    private float tick;

    private Renderer rend;
    private int growFactor;

    private float fullBody = 0.94f;
    private float startBody = -1.5f;
    private float currBody = 0f;

    private WormState state;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        growFactor = Shader.PropertyToID("_Deform");
        currBody = rend.material.GetFloat(growFactor);
        fullBody = currBody;
        rend.material.SetFloat(growFactor, startBody);

        state = WormState.Idle;
    }

    private void Start()
    {
        StartCoroutine(Born());
    }

    private IEnumerator Born()
    {
        while(tick < duration)
        {
            tick += Time.deltaTime;
            currBody = rend.material.GetFloat(growFactor);
            currBody += (fullBody - startBody) / duration * Time.deltaTime;
            rend.material.SetFloat(growFactor, currBody);
            yield return null;
        }

        tick = 0f;
    }

    private void InitAnother()
    {
        GameObject another = Instantiate(
                worm,
                transform.position,
                Quaternion.Euler(
                    transform.rotation.x,
                    transform.rotation.y + 180f,
                    transform.rotation.z)
        );
    }
}
