using UnityEngine;

public partial class BodyTissue : MonoBehaviour
{
    private Transform head;
    public Transform Head { get { return head; } }

    private Renderer rend;
    private Material mat;

    private int lengthId;
    private int thicknessId;
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

    private void InitBodyFramesAndTarget(int numFrame)
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

        this.frames = frames;
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
}