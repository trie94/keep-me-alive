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
    private float oxygenConsumeInterval = 3f;
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
    private float bodyLength;
    private float bodyThickness;
    private float speed;
    private float wobble;
    private float flip;
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
    private int thicknessId;
    private int speedId;
    private int wobbleId;
    private int flipId;
    private int capId;
    private int eatingProgressId;

    [SerializeField]
    private GameObject debugPrefab;
    private GameObject debugObj;
    [SerializeField]
    private Transform childTransform;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        mat = rend.material;
        lengthId = Shader.PropertyToID("_BodyLength");
        speedId = Shader.PropertyToID("_Speed");
        wobbleId = Shader.PropertyToID("_Wobble");
        flipId = Shader.PropertyToID("_Flip");
        capId = Shader.PropertyToID("_Cap");
        eatingProgressId = Shader.PropertyToID("_EatingProgress");
        thicknessId = Shader.PropertyToID("_BodyThickness");
        flip = mat.GetFloat(flipId);

        bodyLength = Random.Range(2f, 6f);
        bodyThickness = Random.Range(1.2f, 2.5f);
        speed = Random.Range(4f, 7f);
        wobble = Random.Range(0.1f, 0.5f);

        mat.SetFloat(lengthId, bodyLength);
        mat.SetFloat(thicknessId, bodyThickness);
        mat.SetFloat(wobbleId, wobble);
        mat.SetFloat(speedId, speed);
        mat.SetFloat(eatingProgressId, 0f);

        Mesh mesh = GetComponentInChildren<MeshFilter>().mesh;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);

        // debugObj = Instantiate(debugPrefab);
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
            && !IsOccupied && NeedOxygen()
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
        float originalBodyHeight = 3f; // original height is 3
        Vector3 localPos = new Vector3(0, 0, 1f);

        float z = localPos.z;
        float c = mat.GetFloat(capId) - Mathf.Abs(localPos.z);
        if (c >= 0) {
            c = 1f;
            z *= bodyLength;
        }
        z += (1f-c) * Mathf.Sign(localPos.z) * (bodyLength-1f);
        z += (bodyLength-1f) + originalBodyHeight;

        localPos.z = z;
        float y = Mathf.Sin(z + Time.time * speed) * wobble;
        localPos.y += y * flip;

        head = childTransform.TransformPoint(localPos);
        // debugObj.transform.position = head;
    }

    public void PointerDown(PointerEventData data)
    {
        PlayerBehavior.Instance.carrier.ReleaseOxygen(this);
        BodyTissueGenerator.Instance.RemoveBodyTissueToAvailableList(this);
    }

    public bool NeedOxygen()
    {
        return oxygenNumber < oxygenCapacity;
    }

    public void ReceiveOxygen()
    {
        oxygenNumber++;
        Debug.Assert(oxygenNumber <= oxygenCapacity);
    }

    public void ConsumeOxygen()
    {
        StartCoroutine(ConsumeOxygenCoroutine());
    }

    private IEnumerator ConsumeOxygenCoroutine()
    {
        Debug.Assert(oxygenNumber > 0);
        while (oxygenTick < oxygenConsumeInterval)
        {
            oxygenTick += Time.deltaTime;
            mat.SetFloat(eatingProgressId, oxygenTick / oxygenConsumeInterval);
            yield return new WaitForEndOfFrame();
        }
        oxygenNumber--;
        oxygenTick = 0f;
        mat.SetFloat(eatingProgressId, 0f);
        BodyTissueGenerator.Instance.AddBodyTissueToAvailableList(this);
    }
}
