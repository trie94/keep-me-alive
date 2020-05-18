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
    private float bodyThickness;
    private float speed;
    private float wobble;
    #endregion

    private Transform head;
    public Transform Head { get { return head; } }

    private Renderer rend;
    private Material mat;

    private int lengthId;
    private int thicknessId;
    private int eatingProgressId;
    private int framesId;

    private float attractRadius = 4f;
    private float grabOxygenRadius = 1f;

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

        bodyThickness = Random.Range(1f, 2f);
        speed = Random.Range(4f, 7f);
        wobble = Random.Range(0.1f, 0.5f);

        mat.SetFloat(thicknessId, bodyThickness);
        mat.SetFloat(eatingProgressId, 0f);

        Mesh mesh = GetComponentInChildren<MeshFilter>().mesh;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);

        frameMatrices = new Matrix4x4[numFrame];
    }

    private void Start()
    {
        frames = InitBodyFramesAndTarget(numFrame);
    }

    private void Update()
    {
        UpdateBodyFrames();
        FindClosestOxygen();
    }

    private void FindClosestOxygen()
    {
        if (NeedOxygen())
        {
            Oxygen closest = null;
            float min = float.MaxValue;
            for (int i = 0; i < OxygenController.Instance.oxygenList.Count; i++)
            {
                Oxygen curr = OxygenController.Instance.oxygenList[i];
                Vector3 headToOxygen = curr.transform.position - head.position;
                if (curr.state == MoleculeState.Released || curr.state == MoleculeState.OxygenArea) continue;

                if (headToOxygen.sqrMagnitude < min)
                {
                    closest = curr;
                    min = headToOxygen.sqrMagnitude;
                }
            }

            if (closest != null)
            {
                if (min < grabOxygenRadius * grabOxygenRadius)
                {
                    // check if it is already grabbed
                    GrabOxygen(closest);
                }

                if (min < attractRadius * attractRadius)
                {
                    SetTarget(closest.transform);
                }
                else if (target.targetToFollow != null)
                {
                    SetTarget(null);
                }
            }
        }
    }

    private BodyFrame[] InitBodyFramesAndTarget(int numFrame)
    {
        BodyFrame[] frames = new BodyFrame[numFrame];
        for (int i = 0; i < numFrame; i++)
        {
            Vector3 position = transform.position + transform.forward * (float)i * BodyFrameSpring.restDistance;

            BodyFrame frameObject = Instantiate(framePrefab, position, Quaternion.identity);
            frameObject.transform.forward = this.transform.forward;
            frameObject.transform.parent = this.transform;
            frameObject.GetComponent<MeshRenderer>().enabled = debugSpringPhysics;
            frames[i] = frameObject;
        }
        head = frames[frames.Length - 1].transform;

        Vector3 targetInitPos = head.position + transform.forward * BodyFrameSpring.restDistance;
        target = Instantiate(targetPrefab, targetInitPos, Quaternion.identity);
        target.transform.forward = transform.forward;
        target.GetComponent<MeshRenderer>().enabled = debugSpringPhysics;

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
        float dist = (target.transform.position - head.position).magnitude;
        if (dist < followThreshold)
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
        head = frames[frames.Length - 1].transform;

        // set matrices
        for (int i = 0; i < frames.Length; i++)
        {
            frameMatrices[i] = frames[i].transform.localToWorldMatrix;
        }
        mat.SetMatrixArray(framesId, frameMatrices);
    }

    public void SetTarget(Transform targetToFollow)
    {
        target.targetToFollow = targetToFollow;
    }

    public bool NeedOxygen()
    {
        return oxygenNumber < oxygenCapacity;
    }

    public void GrabOxygen(Oxygen oxygen)
    {
        oxygenNumber++;
        oxygen.carrier.ReleaseOxygen(oxygen, this);
        Debug.Assert(oxygenNumber <= oxygenCapacity);
    }

    public void ConsumeOxygen()
    {
        SetTarget(null);
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
    }

    private void OnDrawGizmos()
    {
        if (target.targetToFollow)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(head.position, target.targetToFollow.position);
        }
    }
}
