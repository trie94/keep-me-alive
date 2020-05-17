using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MoleculeState
{
    OxygenArea, HopOnCell, BeingCarried, Released, HitBodyTissue, FallFromCell, Abandoned
}

[System.Serializable]
public class Oxygen : Molecule
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
    public BodyTissue targetBodyTissue;

    private Vector3 direction;
    private Vector2 velocityVZ;
    private float velocityY;

    private float speed;
    private float fallSpeed = 0.3f;
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
    private float detachDist = 0.28f;
    private float squareDetachDist;
    private float abandonDist = 3.0f;
    private float squareAbandonDist;
    public float velocitySensitivity = 0.5f;

    public float squareAvoidanceRadius;
    [SerializeField]
    private float squareNeighborRadius;
    #endregion

    #region State
    private float joinOxygenGroupThreshold = 4f;
    private float sqrJoinOxygenGroupThreshold;
    private float sqrRad;
    #endregion

    public CreatureTypes type = CreatureTypes.Oxygen;
    private Dictionary<CreatureTypes, List<Transform>> oxygenGroup;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        speed = Random.Range(0.5f, 0.7f);
        emotionPickInterval = Random.Range(5f, 10f);
        state = MoleculeState.OxygenArea;
        oxygenGroup = new Dictionary<CreatureTypes, List<Transform>>();
        oxygenGroup.Add(type, null);

        squareAvoidanceRadius = avoidanceRadius * avoidanceRadius;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareDetachDist = detachDist * detachDist;
        squareAbandonDist = abandonDist * abandonDist;
        sqrRad = Path.Instance.OxygenZone.Radius * Path.Instance.OxygenZone.Radius;
        sqrJoinOxygenGroupThreshold = joinOxygenGroupThreshold * joinOxygenGroupThreshold;
    }

    private void Update()
    {
        UpdateState();
        Move();
    }

    private void UpdateState()
    {
        List<Transform> neighbors = GetOxygenNeighbors();

        if (state == MoleculeState.HopOnCell)
        {
            float sqrDist = Vector3.SqrMagnitude(hopOnHolder.transform.position
                - transform.position);
            if (sqrDist < 0.01f)
            {
                state = MoleculeState.BeingCarried;
            }
        }
        else if (state == MoleculeState.BeingCarried)
        {
            oxygenGroup[type] = neighbors;
            if (carrier.IsPlayer)
            {
                float squareDistBetweenHolderAndOxygen = (hopOnHolder.attachPoint - transform.position).sqrMagnitude;
                if (squareDistBetweenHolderAndOxygen > squareDetachDist)
                {
                    carrier.AbandonOxygen(this);
                }
            }
        }
        else if (state == MoleculeState.FallFromCell)
        {
            if ((hopOnHolder.transform.position - transform.position).sqrMagnitude
                > squareAbandonDist)
            {
                state = MoleculeState.Abandoned;
                // need a reference point to calculate distance
                hopOnHolder = null;
            }
        }
        else if (state == MoleculeState.Abandoned)
        {
            float sqrDistToOxygenArea = (Path.Instance.OxygenZone.transform.position
                - transform.position).sqrMagnitude;
            if (sqrDistToOxygenArea <= sqrRad)
            {
                if (sqrDistToOxygenArea < sqrJoinOxygenGroupThreshold)
                {
                    state = MoleculeState.OxygenArea;
                }
                else
                {
                    direction = (Path.Instance.OxygenZone.transform.position
                    - transform.position).normalized;
                }
            }
            else
            {
                direction = Vector3.zero;
            }
        }
        else if (state == MoleculeState.OxygenArea)
        {
            oxygenGroup[type] = neighbors;
            direction = oxygenBehavior.CalculateVelocity(
                this, oxygenGroup, Path.Instance.OxygenZone.transform.position)
                .normalized;
        }
        else if (state == MoleculeState.Released)
        {
            Debug.Log("oxygen released");
            state = MoleculeState.HitBodyTissue;
        }
        else if (state == MoleculeState.HitBodyTissue)
        {
            targetBodyTissue.ConsumeOxygen();
            Reset();
        }
    }

    private void Move()
    {
        if (state == MoleculeState.OxygenArea || state == MoleculeState.Abandoned)
        {
            transform.position += direction * Time.deltaTime * speed;
        }
        else if (state == MoleculeState.HopOnCell)
        {
            transform.position = CustomSmoothDamp(hopOnHolder.transform,
                springDampWhenGrabbed, springDampWhenGrabbed / 2f);
        }
        else if (state == MoleculeState.BeingCarried)
        {
            transform.position = CustomSmoothDamp(hopOnHolder.attachPoint,
                springDamp, springDamp / 2f);
        }
        else if (state == MoleculeState.Released)
        {
            transform.position = CustomSmoothDamp(targetBodyTissue.Head,
                springDampWhenReleased, springDampWhenReleased);
        }
        else if (state == MoleculeState.FallFromCell)
        {
            transform.position -= direction * Time.deltaTime * fallSpeed;
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
            if (curr == this || curr.carrier != null) continue;
            if (Vector3.SqrMagnitude(curr.transform.position - transform.position) <= squareNeighborRadius)
            {
                neighbors.Add(curr.transform);
            }
        }
        return neighbors;
    }

    private void Reset()
    {
        Debug.Log("reset oxygen");
        speed = Random.Range(0.5f, 0.7f);
        emotionPickInterval = Random.Range(5f, 10f);
        state = MoleculeState.OxygenArea;
        transform.position = OxygenController.Instance.GetRandomPositionInOxygenArea();
        transform.rotation = Random.rotation;
        OxygenController.Instance.oxygens.Push(this);
        targetBodyTissue.SetTarget(null);
        targetBodyTissue = null;
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
