using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    private bool isEditor;
    private Quaternion currRotation;
    private Quaternion prevRotation;

    private Vector3 prevPosition;
    private Vector3 velocity;
    private Vector3 currVelocity;

    [SerializeField]
    private float velocityMultiplier = 0.5f;
    [SerializeField]
    private float maxSpeed = 3f;
    [SerializeField]
    private float speed = 1f;
    private float pressTime = 0f;

    private Vector3 targetPositionOnSegment;
    private float progress = 0f;
    private float duration = 10f;
    private Segment currSeg;
    private float distSqrtThresholdToUpdateSegment = 0.5f;

    [SerializeField]
    private GameObject debugSphere;
    private GameObject debugIndicatorOnLine;
    private Vector3 closestPointOnCurrSeg;

    private void Awake()
    {
#if UNITY_EDITOR
        isEditor = true;
#else
        isEditor = false;
        Input.gyro.enabled = true;
#endif
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        transform.position = Path.Instance.GetPoint(currSeg, progress);
        transform.forward = velocity = (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized;
        closestPointOnCurrSeg = transform.position;
        debugIndicatorOnLine = Instantiate(debugSphere, transform.position, transform.rotation);
    }

    private void Update()
    {
        closestPointOnCurrSeg = GetClosestPointOnLine(currSeg);
        UpdateCurrSeg();

        if (isEditor)
        {
            if (!Input.anyKey)  // no input
            {
                pressTime = 0f;
            }
            else
            {
                pressTime += Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                velocity += transform.forward * pressTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                velocity += transform.right * -pressTime;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                velocity += transform.right * pressTime;
            }

            if (Input.GetKey(KeyCode.A))
            {
                velocity += transform.up * pressTime;
            }
            if (Input.GetKey(KeyCode.Z))
            {
                velocity += transform.up * -pressTime;
            }

            // move
            velocity *= velocityMultiplier;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            transform.position += velocity * Time.deltaTime * speed;
            transform.forward = Vector3.SmoothDamp(transform.forward, velocity, ref currVelocity, speed);

            debugIndicatorOnLine.transform.position = GetClosestPointOnLine(currSeg);
        }
    }

    private Segment GetNextSegment()
    {
        var nextSeg = currSeg.n1.nextSegments;
        if (nextSeg.Count > 1)
        {
            float weightSum = 0f;
            for (int i = 0; i < nextSeg.Count; i++)
            {
                weightSum += nextSeg[i].weight;
            }

            float rnd = Random.Range(0, weightSum);
            for (int i = 0; i < nextSeg.Count; i++)
            {
                if (rnd < nextSeg[i].weight)
                    return nextSeg[i];
                rnd -= nextSeg[i].weight;
            }
        }
        else
        {
            return nextSeg[0];
        }

        return nextSeg[0];
    }

    private void UpdateCurrSeg()
    {
        Segment potential = currSeg;
        var nextStartNode = currSeg.n1;

        // check in case they ignore curr seg and do sharp turn
        for (int i = 0; i < nextStartNode.nextSegments.Count; i++)
        {
            var seg = nextStartNode.nextSegments[i];
            if ((GetClosestPointOnLine(seg) - transform.position).sqrMagnitude < (GetClosestPointOnLine(potential) - transform.position).sqrMagnitude)
            {
                potential = seg;
            }
        }

        // for branching paths, we need to check curr seg as well
        var currStartNode = currSeg.n0;
        if (currStartNode.nextSegments.Count > 1)
        {
            for (int i = 0; i < currStartNode.nextSegments.Count; i++)
            {
                var seg = currStartNode.nextSegments[i];
                if ((GetClosestPointOnLine(seg) - transform.position).sqrMagnitude < (GetClosestPointOnLine(potential) - transform.position).sqrMagnitude)
                {
                    potential = seg;
                }
            }
        }

        if (potential != currSeg) currSeg = potential;
    }

    private Vector3 GetClosestPointOnLine(Segment currSeg)
    {
        Vector3 playerToStartPoint = currSeg.n0.transform.position - transform.position;
        Vector3 segDir = (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized;
        Vector3 playerToClosestPointOnLine = playerToStartPoint - Vector3.Dot(playerToStartPoint, segDir) * segDir;

        return transform.position + playerToClosestPointOnLine;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
        // highlight currseg
        Gizmos.color = Color.red;
        Gizmos.DrawLine(currSeg.n0.transform.position, currSeg.n1.transform.position);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, closestPointOnCurrSeg);
    }

}
