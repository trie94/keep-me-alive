using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Heart : MonoBehaviour
{
    [SerializeField]
    private float uiRevealDist;
    private float uiRevealDistSqrt;

    private InteractableObject interactable;

    private void Start()
    {
        interactable = GetComponent<InteractableObject>();
        interactable.RegisterCallback(EventTriggerType.PointerDown, PointerDown);
        uiRevealDistSqrt = uiRevealDist * uiRevealDist;
    }

    private void Update()
    {
        float distSqrt = Vector3.SqrMagnitude(PlayerBehavior.Instance.transform.position - transform.position);
        Vector3 direction = (PlayerBehavior.Instance.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(direction, PlayerBehavior.Instance.transform.forward);

        if (PlayerBehavior.Instance.carrier.CanReleaseOxygen()
            && distSqrt < uiRevealDistSqrt && dot < 0)
        {
            interactable.IsInteractable = true;
        }
        else
        {
            interactable.IsInteractable = false;
        }
    }

    public void PointerDown(PointerEventData data)
    {
        PlayerBehavior.Instance.carrier.ReleaseOxygen();
    }
}
