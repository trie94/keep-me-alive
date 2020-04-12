using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// TODO - tissue type will be needed if planning to add different behaviors
// based on the different tissues.
// TODO - tissue needs state especially for animation..
public class BodyTissue : MonoBehaviour
{
    #region oxygen
    private int oxygenCapacity = 1;
    private int oxygenNumber;
    private float oxygenConsumeInterval = 5f;
    private float oxygenTick;
    // TODO - might need oxygen reference
    #endregion

    #region carbon dioxide
    private int carbonDioxideCapacity = 2;
    private int carbonDioxideNumber;
    [SerializeField]
    private float carbonDioxideSpawnInterval = 10f;
    private float tick;
    #endregion

    #region visual
    private float length;
    private float speed;
    #endregion

    private bool isOccupied;
    public bool IsOccupied { get { return isOccupied; } set { isOccupied = value; } }
    private Vector3 head;
    public Vector3 Head { get { return head; } }

    [SerializeField]
    private float uiRevealDist;
    private float uiRevealDistSqrt;

    private InteractableObject interactable;

    private Renderer rend;
    private Material mat;

    private int lengthId;
    private int speedId;
    private int wobbleId;
    private int flipId;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        mat = rend.material;
        lengthId = Shader.PropertyToID("_BodyLength");
        speedId = Shader.PropertyToID("_Speed");
        wobbleId = Shader.PropertyToID("_Wobble");
        flipId = Shader.PropertyToID("_Flip");
    }

    private void Start()
    {
        interactable = GetComponent<InteractableObject>();
        interactable.RegisterCallback(EventTriggerType.PointerDown, PointerDown);
        uiRevealDistSqrt = uiRevealDist * uiRevealDist;
        head = transform.position;
    }

    private void Update()
    {
        UpdateHeadPosition();
        interactable.SetDifferentUiRevealPosition(head);

        float distSqrt = Vector3.SqrMagnitude(PlayerBehavior.Instance.transform.position - head);
        Vector3 direction = (PlayerBehavior.Instance.transform.position - head).normalized;
        float dot = Vector3.Dot(direction, PlayerBehavior.Instance.transform.forward);

        if (PlayerBehavior.Instance.carrier.CanReleaseOxygen()
            && NeedOxygen()
            && distSqrt < uiRevealDistSqrt && dot < -0.5f)
        {
            interactable.IsInteractable = true;
        }
        else
        {
            interactable.IsInteractable = false;
        }
    }

    private void UpdateHeadPosition()
    {
        // this can be moved to start if no dynamic change is needed.
        float bodyLength = mat.GetFloat(lengthId);
        float wobble = mat.GetFloat(wobbleId);
        float speed = mat.GetFloat(speedId);
        float flip = mat.GetFloat(flipId);
        Vector3 localPos = transform.InverseTransformPoint(transform.localPosition);

        float z = localPos.z * bodyLength;
        z += bodyLength - 2.5f;
        localPos.z = z;
        float y = Mathf.Sin(z + Time.time * speed) * wobble;
        localPos.y += y * flip;

        head = transform.TransformPoint(localPos);
    }

    public void PointerDown(PointerEventData data)
    {
        PlayerBehavior.Instance.carrier.ReleaseOxygen(this);
    }

    public bool NeedOxygen()
    {
        return oxygenNumber < oxygenCapacity;
    }

    public void ReceiveOxygen()
    {
        oxygenNumber++;
        Debug.Assert(oxygenNumber <= oxygenCapacity);
        StartCoroutine(ConsumeOxygen());
    }

    private IEnumerator ConsumeOxygen()
    {
        Debug.Assert(oxygenNumber > 0);
        while (oxygenTick < oxygenConsumeInterval)
        {
            oxygenTick += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        oxygenNumber--;
        oxygenTick = 0f;
    }
}
