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
    }
    // move player using keyboard input
    // block player when too close to the wall
    // slow down when player hits something (for now, it's just wall)

    private void Update()
    {
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
            // Vector3 target = Path.Instance.GetPoint(currSeg, progress);
            velocity *= velocityMultiplier;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            transform.position += velocity * Time.deltaTime * speed;
            transform.forward = Vector3.SmoothDamp(transform.forward, velocity, ref currVelocity, speed);
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
        // find nearest node
        var nodes = Path.Instance.nodes;
        float shortestDistSqrt = float.MaxValue;
        Node closest = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            Node curr = nodes[i];
            float distSqrt = (transform.position - curr.transform.position).sqrMagnitude;

            if (closest == null || distSqrt < shortestDistSqrt)
            {
                shortestDistSqrt = distSqrt;
                closest = curr;
            }
        }

        // compare against segments that have closest node
        var segments = Path.Instance.segments;
        shortestDistSqrt = float.MaxValue;
        Segment closestSegment = null;
        for (int i = 0; i < segments.Count; i++)
        {
            // find closest segment
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
    }
}
