using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum WormState
{
    Idle, Attack, TakeAwayOxygen, RunAway, Hide, Captured, Killed
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

    [SerializeField]
    private float attackRadius = 1.5f;
    private float squareAttackRadius;

    private WormState state;
    private WormState prevState;

    [SerializeField]
    private GermMovement idleBehavior;
    [SerializeField]
    private GermMovement moveToTargetBehavior;

    [SerializeField] //debug
    private Erythrocyte target;

    [SerializeField]
    private GameObject mole;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        growFactor = Shader.PropertyToID("_Deform");
        currBody = rend.material.GetFloat(growFactor);
        fullBody = currBody;
        rend.material.SetFloat(growFactor, startBody);
        squareAttackRadius = attackRadius * attackRadius;

        state = WormState.Idle;
        prevState = state;
    }

    private void Start()
    {
        StartCoroutine(Born());
    }

    private void Update()
    {
        if (state == WormState.Idle)
        {
            if (target == null)
            {
                target = FindTarget();
            }
            else
            {
                state = WormState.Attack;
            }
        }
        else if (state == WormState.Attack)
        {
            float squareDist = Vector3.SqrMagnitude(transform.position - target.transform.position);
            if (squareDist < 0.2f)
            {
                state = WormState.TakeAwayOxygen;
            }
            else
            {
                // move towards the target erythrocyte
            }
        }
        else if (state == WormState.TakeAwayOxygen)
        {
            // eat all the oxygen (or one)
            // if successfully finish eating, go to hide
            // if there's a leukocyte nearby, hide and stash oxygen
        }
        else if (state == WormState.RunAway)
        {
            // if the worm is currently outside of the mole
            // come back to the mole
        }
        else if (state == WormState.Hide)
        {
            // hide inside the mole
            // when there's no leukocyte, come back to idle
            // when there's leukocyte, keep hiding
        }
        else if (state == WormState.Captured)
        {
            // the worm is dragged out from the mole by a leukocyte
            // ONLY WHEN THE LEUKOCYTE FINDS THE WORM BEFORE IT HIDES
        }
        else if (state == WormState.Killed)
        {
            // killed, and back to the worm pool
            // and reinstantiate the worm with idle state
        }
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

    // find target to attack
    private Erythrocyte FindTarget()
    {
        var erythrocytes = CellController.Instance.erythrocytes;
        target = null;
        float closestSquareDist = 0f;

        for (int i = 0; i < erythrocytes.Count; i++)
        {
            var currErythrocyte = erythrocytes[i];
            float squareDistBetween = Vector3.SqrMagnitude(currErythrocyte.transform.position - transform.position);
            if (squareDistBetween <= squareAttackRadius)
            {
                if (target == null || squareDistBetween < closestSquareDist)
                {
                    target = (Erythrocyte)currErythrocyte;
                }
            }
        }
        return target;
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
