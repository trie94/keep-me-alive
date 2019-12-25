using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OxygenState
{
    OxygenArea, HopOnCell, BeingCarried, HeartArea, HitHeart
}

[System.Serializable]
[RequireComponent(typeof(Collider))]
public class Oxygen : MonoBehaviour
{
    private Collider oxygenCollider;
    public Collider OxygenCollider
    {
        get { return oxygenCollider; }
    }

    #region Emotion
    private Renderer rend;
    private int faceID;
    [SerializeField]
    private float timeInterval;
    private float tick = 0f;
    private int frameIndex = 0;
    private Emotions currEmotion = Emotions.Neutral;
    private Texture2D[] currEmotionTextures;
    private float emotionPickInterval;
    private float pickTick = 0f;
    #endregion

    #region Movement
    [SerializeField]
    private OxygenMovement oxygenBehavior;
    [SerializeField]
    private OxygenMovement oxygenBehaviorFollowCell;
    [SerializeField]
    private OxygenMovement oxygenBehaviorHeart;

    public Vector3 currVelocity;
    public float progress = 0f;
    public float speed;

    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 5f)]
    public float avoidanceRadius = 0.5f;
    [Range(0.1f, 10f)]
    public float velocityMultiplier = 10f;
    [Range(0.1f, 10f)]
    public float maxSpeed = 5f;
    private float squareMaxSpeed;
    public float squareAvoidanceRadius;
    private float squareNeighborRadius;
    #endregion

    #region Delivery
    public Cell master;
    public OxygenHolder hopOnHolder;
    public OxygenState state;
    private float resetTime = 1f;
    private float resetTick = 0f;
    #endregion

    public CreatureTypes type = CreatureTypes.Oxygen;
    private Dictionary<CreatureTypes, List<Transform>> oxygenGroup;

    private void Awake()
    {
        oxygenCollider = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        speed = Random.Range(0.5f, 0.7f);
        emotionPickInterval = Random.Range(5f, 10f);
        state = OxygenState.OxygenArea;
        oxygenGroup = new Dictionary<CreatureTypes, List<Transform>>();
        oxygenGroup.Add(type, null);
    }

    // refactor this based on the oxygen state
    private void Update()
    {
        List<Transform> neighbors = GetOxygenNeighbors();
        Vector3 velocity = Vector3.zero;
        if (state == OxygenState.HopOnCell)
        {
            float distSqrt = Vector3.SqrMagnitude(hopOnHolder.transform.position - transform.position);
            if (distSqrt < 0.01f)
            {
                hopOnHolder.isOccupied = true;
                state = OxygenState.BeingCarried;
            }
            else
            {
                oxygenGroup[type] = neighbors;
                velocity = oxygenBehaviorFollowCell.CalculateVelocity(this, oxygenGroup);
            }
        }
        else if (state == OxygenState.BeingCarried)
        {
            oxygenGroup[type] = neighbors;
            velocity = oxygenBehaviorFollowCell.CalculateVelocity(this, oxygenGroup);
            return;
        }
        else if (state == OxygenState.OxygenArea)
        {
            oxygenGroup[type] = neighbors;
            velocity = oxygenBehavior.CalculateVelocity(this, oxygenGroup,
                                                        CellController.Instance.oxygenArea.position);
        }
        else if (state == OxygenState.HeartArea)
        {
            oxygenGroup[type] = neighbors;
            velocity = oxygenBehaviorHeart.CalculateVelocity(this, oxygenGroup,
                                                             CellController.Instance.heart.position);
            float dist = Vector3.SqrMagnitude(transform.position
                                              - CellController.Instance.heart.position);
            if (dist < 0.2f)
            {
                state = OxygenState.HitHeart;
            }
        }
        else if (state == OxygenState.HitHeart)
        {
            if (resetTick > resetTime)
            {
                Reset();
            }
            else
            {
                resetTick += Time.deltaTime;
            }
        }

        velocity *= velocityMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        Move(velocity);
    }

    private void Move(Vector3 velocity)
    {
        if (velocity != Vector3.zero) currVelocity = velocity;
        transform.position += currVelocity * Time.deltaTime * speed;
        if (currVelocity != Vector3.zero) transform.forward = currVelocity;
    }

    private List<Transform> GetOxygenNeighbors()
    {
        List<Transform> neighbors = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(transform.position, neighborRadius);

        for (int i = 0; i < contextColliders.Length; i++)
        {
            var curr = contextColliders[i];
            var oxygens = OxygenController.Instance.oxygenMap;
            // 1. not self, 2. only oxygen, 3. exclude the ones are being carried
            if (curr == oxygenCollider || !oxygens.ContainsKey(curr.transform)
                || oxygens[curr.transform].master != null) continue;
            neighbors.Add(curr.transform);
        }
        return neighbors;
    }

    private void Reset()
    {
        speed = Random.Range(0.5f, 0.7f);
        emotionPickInterval = Random.Range(5f, 10f);
        state = OxygenState.OxygenArea;
        transform.position = OxygenController.Instance.GetRandomPositionInOxygenArea();
        transform.rotation = Random.rotation;
        OxygenController.Instance.oxygens.Push(this);
        resetTick = 0f;
    }
}
