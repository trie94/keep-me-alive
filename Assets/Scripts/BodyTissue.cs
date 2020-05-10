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
    private int eatingProgressId;
    private int framesId;

    [SerializeField]
    private GameObject debugPrefab;
    private GameObject debugObj;
    [SerializeField]
    private Transform childTransform;

    #region frames
    [SerializeField]
    private GameObject framePrefab;
    private int numFrame = 10;
    // tail to head
    private GameObject[] frames;
    private float headOffset = 1.5f;
    private Matrix4x4[] frameMatrices;
    private Transform target;
    #endregion

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        mat = rend.material;
        lengthId = Shader.PropertyToID("_BodyLength");
        speedId = Shader.PropertyToID("_Speed");
        wobbleId = Shader.PropertyToID("_Wobble");
        flipId = Shader.PropertyToID("_Flip");
        eatingProgressId = Shader.PropertyToID("_EatingProgress");
        thicknessId = Shader.PropertyToID("_BodyThickness");
        framesId = Shader.PropertyToID("_Frames");
        flip = mat.GetFloat(flipId);

        bodyLength = Random.Range(5f, 12f);
        bodyThickness = Random.Range(1f, 2f);
        speed = Random.Range(4f, 7f);
        wobble = Random.Range(0.1f, 0.5f);

        mat.SetFloat(lengthId, bodyLength);
        mat.SetFloat(thicknessId, bodyThickness);
        mat.SetFloat(wobbleId, wobble);
        mat.SetFloat(speedId, speed);
        mat.SetFloat(eatingProgressId, 0f);

        Mesh mesh = GetComponentInChildren<MeshFilter>().mesh;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);

        debugObj = Instantiate(debugPrefab);
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
            frameObject.transform.position = childTransform.TransformPoint(Vector3.zero);
            frames[i] = frameObject;
            frameObject.transform.parent = this.transform;
        }
        return frames;
    }

    private void UpdateBodyFrames()
    {
        // set posiiton
        for (int i = 0; i < frames.Length; i++)
        {
            UpdateBodyFrame(frames[i], i);
        }
        // set rotation

        for (int i = 0; i < frames.Length - 1; i++)
        {
            frames[i].transform.forward = frames[i + 1].transform.position - frames[i].transform.position;
        }
        frames[frames.Length - 1].transform.forward = frames[frames.Length - 2].transform.forward;
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

    private void UpdateBodyFrame(GameObject frame, int index)
    {
        bodyLength = mat.GetFloat("_BodyLength");

        float originalBodyHeight = 3f; // original height is 3
        Vector3 localPos = new Vector3(0, 0, -originalBodyHeight / 2f + ((float)index / numFrame) * originalBodyHeight);

        float z = localPos.z;
        z = (z + 1.5f) / 3f * bodyLength;
        z += originalBodyHeight;
        localPos.z = z;
        float y = Mathf.Sin(z + Time.time * speed) * wobble;
        localPos.y += y * flip;

        frame.transform.position = childTransform.TransformPoint(localPos);
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
