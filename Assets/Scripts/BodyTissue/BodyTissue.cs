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
    #endregion

    private bool isOccupied;
    public bool IsOccupied { get { return isOccupied; } }
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
    private int eatingProgressId;
    private int framesId;

    [SerializeField]
    private Transform childTransform;
    private float oxygenFollowThreshold = 2f;
    private float originalBodyHeight = 3f; // original height is 3

    #region frames
    [SerializeField]
    private BodyFrame framePrefab;
    private int numFrame = 10;
    // tail to head
    private BodyFrame[] frames;
    private Matrix4x4[] frameMatrices;
    [SerializeField]
    private BodyTissueTarget targetPrefab;
    private BodyTissueTarget target;
    [SerializeField]
    private bool debugSpringPhysics = false;
    private float followThreshold = 3f;
    #endregion

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        mat = rend.material;
        lengthId = Shader.PropertyToID("_BodyLength");
        eatingProgressId = Shader.PropertyToID("_EatingProgress");
        thicknessId = Shader.PropertyToID("_BodyThickness");
        framesId = Shader.PropertyToID("_Frames");

        bodyLength = Random.Range(5f, 12f);
        bodyThickness = Random.Range(1f, 2f);
        speed = Random.Range(4f, 7f);
        wobble = Random.Range(0.1f, 0.5f);

        mat.SetFloat(lengthId, bodyLength);
        mat.SetFloat(thicknessId, bodyThickness);
        mat.SetFloat(eatingProgressId, 0f);

        Mesh mesh = GetComponentInChildren<MeshFilter>().mesh;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
    }

    private void Start()
    {
        interactable = GetComponent<InteractableObject>();
        interactable.RegisterCallback(EventTriggerType.PointerDown, PointerDown);
        uiRevealDistSqrt = uiRevealDist * uiRevealDist;
        // matrix
        frames = InitBodyFrames(numFrame);
        frameMatrices = new Matrix4x4[numFrame];

        target = Instantiate(targetPrefab, childTransform.TransformPoint(new Vector3(0, 0, followThreshold + bodyLength * 2f)), Quaternion.identity);
        target.transform.forward = transform.forward;
        target.GetComponent<MeshRenderer>().enabled = debugSpringPhysics;
    }

    private void Update()
    {
        UpdateBodyFrames();
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

    private BodyFrame[] InitBodyFrames(int numFrame)
    {
        BodyFrame[] frames = new BodyFrame[numFrame];
        for (int i = 0; i < numFrame; i++)
        {
            // compute position
            Vector3 localPos = new Vector3(0, 0, -originalBodyHeight / 2f + ((float)i / numFrame) * originalBodyHeight);
            float z = localPos.z;
            z = (z + 1.5f) / 3f * bodyLength;
            z += originalBodyHeight;
            localPos.z = z;

            BodyFrame frameObject = Instantiate(framePrefab, childTransform.TransformPoint(localPos), Quaternion.identity);
            frameObject.transform.parent = this.transform;
            frameObject.GetComponent<MeshRenderer>().enabled = debugSpringPhysics;
            frames[i] = frameObject;
        }
        head = frames[frames.Length - 1].transform.position;
        return frames;
    }

    private void UpdateBodyFrames()
    {
        // reset acceleration
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i].acceleration = Vector3.zero;
        }

        for (int i = 0; i < frames.Length - 1; i++)
        {
            BodyFrameSpring.ComputeAcceleration(frames[i], frames[i + 1]);
        }

        // update head acceleration
        float dist = (target.transform.position - frames[0].transform.position).magnitude;
        if (dist < followThreshold + bodyLength)
        {
            BodyFrameSpring.ComputeAcceleration(frames[frames.Length - 1], target.transform.position);
        }

        // we do not update the tail
        for (int i = 1; i < frames.Length; i++)
        {
            frames[i].UpdateVelocity();
        }

        // update tail alignment first
        Vector3 tailForward = frames[frames.Length - 1].transform.position - frames[0].transform.position;
        if (tailForward != Vector3.zero) frames[0].transform.forward = tailForward;

        // update head alignment
        Vector3 headForward = target.transform.position - frames[frames.Length - 1].transform.position;
        if (headForward != Vector3.zero) frames[frames.Length - 1].transform.forward = headForward;

        for (int i = 1; i < frames.Length - 1; i++)
        {
            frames[i].ComputeAlignment(frames[i - 1], frames[i + 1]);
        }

        // update head reference
        head = frames[frames.Length - 1].transform.position;

        // set matrices
        for (int i = 0; i < frames.Length; i++)
        {
            frameMatrices[i] = frames[i].transform.localToWorldMatrix;
        }
        mat.SetMatrixArray(framesId, frameMatrices);
    }

    public void PointerDown(PointerEventData data)
    {
        PlayerBehavior.Instance.carrier.ReleaseOxygen(this);
        BodyTissueGenerator.Instance.RemoveBodyTissueToAvailableList(this);
    }

    public void SetTarget(Transform targetToFollow)
    {
        target.targetToFollow = targetToFollow;
    }

    public bool NeedOxygen()
    {
        return oxygenNumber < oxygenCapacity;
    }

    public void Occupy(Cell cell)
    {
        isOccupied = true;
        SetTarget(cell.transform);
    }

    public void UnOccupy()
    {
        isOccupied = false;
        SetTarget(null);
    }

    public void ReceiveOxygen()
    {
        UnOccupy();
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
