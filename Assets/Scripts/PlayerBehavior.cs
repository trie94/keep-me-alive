using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum PlayerZoneState
{
    Vein, OxygenArea, HeartArea
}

public class PlayerBehavior : MonoBehaviour
{
    private bool isEditor;
    private Vector3 currRotationRate;
    private Vector3 prevRotationRate;

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

    private float progress = 0f;
    private Segment currSeg;
    [SerializeField]
    private float maxDistFromCenter = 3.9f;
    private float maxDistFromCenterSqrt;
    [SerializeField]  // for debugging
    private PlayerZoneState currZoneState;
    private Zone currZone;
    private Quaternion correctionQuaternion;

    #region Debug

    [SerializeField]
    private GameObject debugSphere;
    private GameObject debugIndicatorOnLine;
    #endregion

    private void Awake()
    {
#if UNITY_EDITOR
        isEditor = true;
#else
        isEditor = false;
        Input.gyro.enabled = true;

        currRotationRate = Input.gyro.rotationRate;
#endif
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        transform.position = Path.Instance.GetPoint(currSeg, progress);
        transform.forward = (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized;
        velocity = Vector3.zero;
        debugIndicatorOnLine = Instantiate(debugSphere, transform.position, transform.rotation);
        correctionQuaternion = Quaternion.Euler(90f, 0f, 0f);
        maxDistFromCenterSqrt = maxDistFromCenter * maxDistFromCenter;
        UpdateZoneState();
    }

    private void Update()
    {
        // update curr seg and zone state
        UpdateZoneState();
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

            // debug indicator
            if (currZoneState == PlayerZoneState.Vein)
            {
                debugIndicatorOnLine.transform.position = GetClosestPointOnLine(currSeg);
            }
            else if (currZoneState == PlayerZoneState.OxygenArea)
            {
                debugIndicatorOnLine.transform.position = Path.Instance.OxygenZone.transform.position;
            }
            else if (currZoneState == PlayerZoneState.HeartArea)
            {
                debugIndicatorOnLine.transform.position = Path.Instance.HeartZone.transform.position;
            }
        }
        else
        {
            currRotationRate = Input.gyro.rotationRate;
            UIController.Instance.SetDebugText("attitude: " + Input.gyro.attitude);
            // velocity += transform.up * -(Input.gyro.attitude.x) * 5f;
            // velocity += transform.right * -(currRotationRate.z) * 5f;
            // velocity += transform.forward;
            Quaternion gyroQuaternion = GyroToUnity(Input.gyro.attitude);
            Quaternion calculatedRotation = correctionQuaternion * gyroQuaternion;
            velocity = calculatedRotation * Vector3.forward;
        }

        if (currZoneState == PlayerZoneState.Vein)
        {
            Vector3 pointOnLine = GetClosestPointOnLine(currSeg);
            Vector3 playerToPoint = pointOnLine - transform.position;
            if (playerToPoint.sqrMagnitude >= maxDistFromCenterSqrt)
            {
                transform.position = Vector3.Lerp(pointOnLine, transform.position, 0.9f);
            }
            else
            {
                Move();
            }
        }
        else
        {
            // zone
            Debug.Assert(currZone != null);
            float maxDistSqrt = currZone.Radius * currZone.Radius;
            Vector3 playerToCenter = currZone.transform.position - transform.position;

            if (playerToCenter.sqrMagnitude >= maxDistSqrt)
            {
                transform.position = Vector3.Lerp(currZone.transform.position, transform.position, 0.9f);
            }
            else
            {
                Move();
            }
        }
    }

    private void Move()
    {
        velocity *= velocityMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime * speed;
        transform.forward = Vector3.SmoothDamp(transform.forward, velocity, ref currVelocity, speed);
        // if(velocity != Vector3.zero) transform.forward = velocity.normalized;
    }

    private void UpdateCurrSeg()
    {
        Segment potential = currSeg;
        if (currZoneState == PlayerZoneState.Vein)
        {
            var nextStartNode = currSeg.n1;

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
        }
        else if (currZoneState == PlayerZoneState.HeartArea)
        {
            for (int i = 0; i < Path.Instance.HeartExitSegments.Count; i++)
            {
                var seg = Path.Instance.HeartExitSegments[i];
                if ((GetClosestPointOnLine(seg) - transform.position).sqrMagnitude < (GetClosestPointOnLine(potential) - transform.position).sqrMagnitude)
                {
                    potential = seg;
                }
            }
        }
        else if (currZoneState == PlayerZoneState.OxygenArea)
        {
            for (int i = 0; i < Path.Instance.OxygenExitSegments.Count; i++)
            {
                var seg = Path.Instance.OxygenExitSegments[i];
                if ((GetClosestPointOnLine(seg) - transform.position).sqrMagnitude < (GetClosestPointOnLine(potential) - transform.position).sqrMagnitude)
                {
                    potential = seg;
                }
            }
        }

        currSeg = potential;
    }

    private void UpdateZoneState()
    {
        // check if the player is in the zone area
        for (int i = 0; i < Path.Instance.zones.Count; i++)
        {
            var curr = Path.Instance.zones[i];
            float maxDistSqrt = (curr.Radius) * (curr.Radius);
            if ((curr.transform.position - transform.position).sqrMagnitude < maxDistSqrt)
            {
                // vein seg is close enough and the direction is pretty much the same
                if (Vector3.Dot(transform.forward, (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized) > 0
                && (GetClosestPointOnLine(currSeg) - transform.position).sqrMagnitude < maxDistFromCenterSqrt)
                {
                    currZoneState = PlayerZoneState.Vein;
                    currZone = null;
                }
                else
                {
                    currZone = curr;
                    currZoneState = (currZone == Path.Instance.OxygenZone) ? PlayerZoneState.OxygenArea : PlayerZoneState.HeartArea;
                }
            }
        }
    }

    private Vector3 GetClosestPointOnLine(Segment seg)
    {
        Vector3 playerToStartPoint = seg.n0.transform.position - transform.position;
        Vector3 segDir = (seg.n1.transform.position - seg.n0.transform.position).normalized;
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
        if (currZoneState == PlayerZoneState.Vein)
        {
            Gizmos.DrawLine(transform.position, GetClosestPointOnLine(currSeg));
        }
        else
        {
            Vector3 zoneCenter = (currZoneState == PlayerZoneState.OxygenArea) ? Path.Instance.zones[0].transform.position : Path.Instance.zones[1].transform.position;
            Gizmos.DrawLine(transform.position, zoneCenter);
        }
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
