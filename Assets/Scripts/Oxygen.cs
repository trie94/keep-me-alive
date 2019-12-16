using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Cell master;

    private void Awake()
    {
        oxygenCollider = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        emotionPickInterval = Random.Range(5f, 10f);
        speed = Random.Range(0.5f, 0.7f);
    }

    private void Update()
    {
        List<Transform> neighbors = GetNeighbors();
        Vector3 velocity = (master == null) ?
            oxygenBehavior.CalculateVelocity(this, neighbors)
          : oxygenBehaviorFollowCell.CalculateVelocity(this, neighbors);
        velocity *= velocityMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        Move(velocity);
    }

    private void Move(Vector3 velocity)
    {
        if (velocity != Vector3.zero) currVelocity = velocity;
        transform.position += currVelocity * Time.deltaTime * speed;
        transform.up = currVelocity;
    }

    private List<Transform> GetNeighbors()
    {
        List<Transform> neighbors = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(transform.position, neighborRadius);

        for (int i = 0; i < contextColliders.Length; i++)
        {
            var curr = contextColliders[i];
            var oxygens = OxygenController.Instance.oxygenMap;
            if (curr == OxygenCollider || !oxygens.ContainsKey(curr.transform)
                || oxygens[curr.transform].master != null) continue;
            neighbors.Add(curr.transform);
        }
        return neighbors;
    }
}
