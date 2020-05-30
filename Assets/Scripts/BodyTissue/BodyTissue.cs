using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO - tissue type will be needed if planning to add different behaviors
// based on the different tissues.
// TODO - tissue needs state especially for animation..
public enum BodyTissueState
{ IDLE, EAT }

public partial class BodyTissue : MonoBehaviour
{
    private BodyTissueState state = BodyTissueState.IDLE;

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

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        mat = rend.material;
        lengthId = Shader.PropertyToID("_BodyLength");
        eatingProgressId = Shader.PropertyToID("_EatingProgress");
        thicknessId = Shader.PropertyToID("_BodyThickness");
        framesId = Shader.PropertyToID("_Frames");
        headColorId = Shader.PropertyToID("_HeadColor");

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
        InitBodyFramesAndTarget(numFrame);
    }

    private void Update()
    {
        UpdateBodyFrames();
        FindClosestOxygen();
        EatOxygenAndDigest();
    }

    public void SetTarget(Transform targetToFollow)
    {
        target.targetToFollow = targetToFollow;
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
