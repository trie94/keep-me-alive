using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// TODO - tissue type will be needed if planning to add different behaviors
// based on the different tissues.
// TODO - tissue needs state especially for animation..
[System.Serializable]
public enum TissueState
{
    IDLE, FOLLOW_OXYGEN, EAT_OXYGEN, BACK_TO_IDLE
}

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
    private int eatingProgressId;
    private int framesId;

    [SerializeField]
    private GameObject debugPrefab;
    private GameObject debugObj;
    [SerializeField]
    private Transform childTransform;
    [SerializeField]
    private TissueState tissueState;
    private TissueState prevState;
    private float oxygenFollowThreshold = 2f;
    private float originalBodyHeight = 3f; // original height is 3

    #region frames
    [SerializeField]
    private GameObject framePrefab;
    private int numFrame = 10;
    // tail to head
    private GameObject[] frames;
    private float headOffset = 1.5f;
    private Matrix4x4[] frameMatrices;
    [SerializeField]
    private GameObject targetObject;
    private Transform target;
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

        debugObj = Instantiate(debugPrefab);
        prevState = TissueState.IDLE;
        tissueState = TissueState.IDLE;
    }

    private void Start()
    {
        interactable = GetComponent<InteractableObject>();
        interactable.RegisterCallback(EventTriggerType.PointerDown, PointerDown);
        uiRevealDistSqrt = uiRevealDist * uiRevealDist;
        head = transform.position;
        // matrix
        frames = InitBodyFrames(numFrame);
        frameMatrices = new Matrix4x4[numFrame];

        GameObject t = Instantiate(targetObject);
        t.transform.position = childTransform.TransformPoint(new Vector3(0, 0, bodyLength * 3));
        target = t.transform;
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

    private GameObject[] InitBodyFrames(int numFrame)
    {
        GameObject[] frames = new GameObject[numFrame];
        for (int i = 0; i < numFrame; i++)
        {
            GameObject frameObject = Instantiate(framePrefab);

            Vector3 localPos = new Vector3(0, 0, -originalBodyHeight / 2f + ((float)i / numFrame) * originalBodyHeight);
            float z = localPos.z;
            z = (z + 1.5f) / 3f * bodyLength;
            z += originalBodyHeight;
            localPos.z = z;
            frameObject.transform.position = childTransform.TransformPoint(localPos);
            frames[i] = frameObject;
            frameObject.transform.parent = this.transform;
        }
        head = frames[frames.Length - 1].transform.position;
        return frames;
    }

    private void UpdateBodyFrames()
    {
        // set posiiton
        if (tissueState == TissueState.IDLE || tissueState == TissueState.EAT_OXYGEN)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                UpdateBodyFrameIdle(frames[i], i);
            }
        }
        else if (tissueState == TissueState.FOLLOW_OXYGEN)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                UpdateBodyFrameFollowTarget(frames[i], i, target);
            }
        }
        else if (tissueState == TissueState.BACK_TO_IDLE)
        {
            if (prevState != tissueState)
            {
                tick = 0;
                prevState = tissueState;
            }
            else if (tick >= 1f)
            {
                prevState = tissueState;
                tissueState = TissueState.IDLE;
                tick = 0;
            }
            else
            {
                tick += Time.deltaTime;
                for (int i = 0; i < frames.Length; i++)
                {
                    UpdateBodyFrameBackToIdle(frames[i], i, tick);
                }
            }
        }

        // set forward
        for (int i = 0; i < frames.Length - 1; i++)
        {
            UpdateBodyFrameForward(frames[i], frames[i + 1], i, target);
        }

        if (tissueState == TissueState.FOLLOW_OXYGEN)
        {
            frames[frames.Length - 1].transform.forward = target.transform.position - frames[frames.Length - 1].transform.position;
        }
        else
        {
            frames[frames.Length - 1].transform.forward = frames[frames.Length - 2].transform.forward;
        }

        // handle the last one, head
        head = frames[frames.Length - 1].transform.position;
        debugObj.transform.position = head;
        // set matrices
        for (int i = 0; i < frames.Length; i++)
        {
            frameMatrices[i] = frames[i].transform.localToWorldMatrix;
        }
        mat.SetMatrixArray(framesId, frameMatrices);
    }

    private void UpdateBodyFrameIdle(GameObject frame, int index)
    {
        bodyLength = mat.GetFloat("_BodyLength");
        Vector3 localPos = new Vector3(0, 0, -originalBodyHeight / 2f + ((float)index / numFrame) * originalBodyHeight);

        if ((transform.position - target.position).magnitude < oxygenFollowThreshold + bodyLength)
        {
            prevState = tissueState;
            tissueState = TissueState.FOLLOW_OXYGEN;
        }
        else
        {
            float z = localPos.z;
            z = (z + 1.5f) / 3f * bodyLength;
            z += originalBodyHeight;
            float y = Mathf.Sin(z + Time.time * speed) * wobble;

            localPos.z = z;
            localPos.y += y;

            frame.transform.position = childTransform.TransformPoint(localPos);
        }
    }

    private void UpdateBodyFrameFollowTarget(GameObject frame, int index, Transform target)
    {
        bodyLength = mat.GetFloat("_BodyLength");
        Vector3 localPos = new Vector3(0, 0, -originalBodyHeight / 2f + ((float)index / numFrame) * originalBodyHeight);

        float z = localPos.z;
        Vector3 localTarget = childTransform.InverseTransformPoint(target.position);
        float dist = (transform.position - target.position).magnitude;


        if (dist < oxygenFollowThreshold + bodyLength)
        {
            float weight = (float)index / numFrame;
            localPos = Vector3.Lerp(localPos, localTarget, weight);

            float y = Mathf.Sin(z + Time.time * speed) * wobble;
            localPos.y += y;

            frame.transform.position = childTransform.TransformPoint(localPos);
        }
        else
        {
            prevState = tissueState;
            tissueState = TissueState.BACK_TO_IDLE;
        }
    }

    private void UpdateBodyFrameBackToIdle(GameObject frame, int index, float lerpFactor)
    {
        Vector3 localTarget = new Vector3(0, 0, -originalBodyHeight / 2f + ((float)index / numFrame) * originalBodyHeight);
        float z = localTarget.z;
        z = (z + 1.5f) / 3f * bodyLength;
        z += originalBodyHeight;
        float y = Mathf.Sin(z + Time.time * speed) * wobble;

        localTarget.z = z;
        localTarget.y += y;

        Vector3 target = childTransform.TransformPoint(localTarget);
        frame.transform.position = Vector3.Lerp(frame.transform.position, target, lerpFactor);
    }

    private void UpdateBodyFrameForward(GameObject currFrame, GameObject frontFrame, int index, Transform target)
    {
        Vector3 f = (frontFrame.transform.position - currFrame.transform.position).normalized;
        float weight = (float)index / numFrame;
        f = Vector3.Lerp(f, (target.position - currFrame.transform.position).normalized, weight);

        if (f != Vector3.zero)
        {
            currFrame.transform.forward = f;
        }
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
