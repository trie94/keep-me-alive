using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OxygenState
{
    OxygenArea, HopOnCell, BeingCarried, HeartArea, HitHeart
}

[System.Serializable]
public class Oxygen : MonoBehaviour
{
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

    private Vector3 direction;
    private Vector3 velocity;
    private Vector2 velocityVZ;
    private float velocityY;

    private float speed;
    private float springDamp = 0.05f;
    private float springDampWhenGrabbed = 0.1f;
    private float springDampWhenReleased = 0.2f;

    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 5f)]
    public float avoidanceRadius = 0.5f;
    // if the oxygen goes beyond this radius, it damps back to the attach point
    [Range(0.1f, 0.7f)]
    public float dampThreshold = 0.3f;
    // if the oxygen goes beyond this radius, it gets detached from the cell
    [Range(0.8f, 1.2f)]
    public float detachThreshold = 0.8f;
    public float velocitySensitivity = 0.5f;

    public float squareAvoidanceRadius;
    [SerializeField]
    private float squareNeighborRadius;
    private float squareMaxRadius;
    #endregion

    #region Delivery
    public Cell master;
    public PlayerBehavior playerMaster;
    public OxygenHolder hopOnHolder;
    public OxygenState state;
    private float resetTime = 1f;
    private float resetTick = 0f;
    #endregion

    public CreatureTypes type = CreatureTypes.Oxygen;
    private Dictionary<CreatureTypes, List<Transform>> oxygenGroup;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        speed = Random.Range(0.7f, 1.0f);
        emotionPickInterval = Random.Range(5f, 10f);
        state = OxygenState.OxygenArea;
        oxygenGroup = new Dictionary<CreatureTypes, List<Transform>>();
        oxygenGroup.Add(type, null);

        squareAvoidanceRadius = avoidanceRadius * avoidanceRadius;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareMaxRadius = dampThreshold * dampThreshold;
    }

    private void Update()
    {
        List<Transform> neighbors = GetOxygenNeighbors();
        Vector3 velocity = Vector3.zero;

        if (state == OxygenState.HopOnCell)
        {
            float sqrDist = Vector3.SqrMagnitude(hopOnHolder.transform.position - transform.position);
            if (sqrDist < 0.01f)
            {
                hopOnHolder.OnOccupied();
                state = OxygenState.BeingCarried;
            }
        }
        else if (state == OxygenState.OxygenArea)
        {
            oxygenGroup[type] = neighbors;
            velocity = oxygenBehavior.CalculateVelocity(this, oxygenGroup,
                                                        Path.Instance.OxygenZone.transform.position);
        }
        else if (state == OxygenState.HeartArea)
        {
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

        Move(velocity.normalized);
    }

    private void Move(Vector3 dir)
    {
        if (dir != Vector3.zero) direction = dir;
        if (state == OxygenState.HopOnCell)
        {
            transform.position = CustomSmoothDamp(hopOnHolder.transform,
                springDampWhenGrabbed, springDampWhenGrabbed / 2f);
        }
        else if (state == OxygenState.BeingCarried)
        {
            transform.position = CustomSmoothDamp(hopOnHolder.attachPoint,
                springDamp, springDamp / 2f);
        }
        else if (state == OxygenState.HeartArea)
        {
            transform.position = CustomSmoothDamp(CellController.Instance.heart,
                springDampWhenReleased, springDampWhenReleased);
        }
        else
        {
            transform.position += direction * Time.deltaTime * speed;
        }

        if (direction != Vector3.zero) transform.forward = direction;
    }

    private List<Transform> GetOxygenNeighbors()
    {
        List<Transform> neighbors = new List<Transform>();
        var oxygens = OxygenController.Instance.oxygenList;

        for (int i = 0; i < oxygens.Count; i++)
        {
            var curr = oxygens[i];
            if (curr == this || curr.master != null || curr.playerMaster != null) continue;
            if (Vector3.SqrMagnitude(curr.transform.position - transform.position) <= squareNeighborRadius)
            {
                neighbors.Add(curr.transform);
            }
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

    private Vector3 CustomSmoothDamp(Transform target, float smoothTimeVz, float smoothTimeY)
    {
        Vector2 vz = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetVz = new Vector2(target.transform.position.x, target.transform.position.z);
        Vector2 xzDamp = Vector2.SmoothDamp(vz, targetVz, ref velocityVZ, smoothTimeVz);
        float yDamp = Mathf.SmoothDamp(transform.position.y, target.transform.position.y, ref velocityY, smoothTimeY);
        return new Vector3(xzDamp.x, yDamp, xzDamp.y);
    }

    private Vector3 CustomSmoothDamp(Vector3 target, float smoothTimeVz, float smoothTimeY)
    {
        Vector2 vz = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetVz = new Vector2(target.x, target.z);
        Vector2 xzDamp = Vector2.SmoothDamp(vz, targetVz, ref velocityVZ, smoothTimeVz);
        float yDamp = Mathf.SmoothDamp(transform.position.y, target.y, ref velocityY, smoothTimeY);
        return new Vector3(xzDamp.x, yDamp, xzDamp.y);
    }
}
      