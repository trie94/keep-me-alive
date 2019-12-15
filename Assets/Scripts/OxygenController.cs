using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenController : MonoBehaviour
{
    private static OxygenController instance;
    public static OxygenController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<OxygenController>();
            }
            return instance;
        }
    }

    [SerializeField]
    private CellBehavior oxygenBehavior;
    [SerializeField]
    private CellBehavior oxygenBehaviorFollowCell;
    [SerializeField]
    private Oxygen oxygenPrefab;
    [SerializeField]
    private int initialOxygenNumber;
    public List<Oxygen> oxygens;
    [SerializeField]
    private Transform oxygenArea;

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

    private void Awake()
    {
        instance = this;
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = avoidanceRadius * avoidanceRadius;
        oxygens = new List<Oxygen>();
    }

    private void Start()
    {
        SpawnOxygen();
    }

    private void Update()
    {
        for (int i = 0; i < oxygens.Count; i++)
        {
            Oxygen oxygen = oxygens[i];
            List<Transform> neighbors = GetNeighbors(oxygen);
            Vector3 velocity = (oxygen.master == null) ?
                oxygenBehavior.CalculateVelocity(oxygen, neighbors)
              : oxygenBehaviorFollowCell.CalculateVelocity(oxygen, neighbors);
            velocity *= velocityMultiplier;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            oxygen.Move(velocity);
        }
    }

    private void SpawnOxygen()
    {
        for (int i = 0; i < initialOxygenNumber; i++)
        {
            Oxygen oxygen = Instantiate(
                oxygenPrefab,
                oxygenArea.position + Random.insideUnitSphere * initialOxygenNumber * 0.5f,
                Random.rotation
            );
            oxygens.Add(oxygen);
        }
    }

    private List<Transform> GetNeighbors(Oxygen oxygen)
    {
        if (oxygen == null) return null;
        List<Transform> neighbors = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(oxygen.transform.position, neighborRadius);

        for (int i = 0; i < contextColliders.Length; i++)
        {
            var curr = contextColliders[i];
            if (curr.tag != "Oxygen" || curr == oxygen.OxygenCollider
                || curr.GetComponent<Oxygen>().master != null) continue;
            neighbors.Add(curr.transform);
        }
        return neighbors;
    }
}
