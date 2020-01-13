﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum PlayerZoneState
{
    Vein, OxygenArea, HeartArea
}

public class PlayerBehavior : MonoBehaviour
{
    private bool isUsingKeyboard;
    private Vector3 currRotationRate;
    private Vector3 prevRotationRate;

    private Vector3 prevPosition;
    private Vector3 direction;
    public Vector3 Direction { get { return direction; } }
    private Vector3 currVelocity;

    [SerializeField]
    private float velocityMultiplier = 0.5f;
    [SerializeField]
    private float maxSpeed = 3f;
    [SerializeField]
    private float moveSpeed = 1f;
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
    [SerializeField]
    private float pitchRotationSpeed = 1f;
    [SerializeField]
    private float rollRotationSpeed = 1f;
    private float pitch;
    private float yaw;

    #region Debug

    [SerializeField]
    private GameObject debugSphere;
    private GameObject debugIndicatorOnLine;
    #endregion

    private void Awake()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        isUsingKeyboard = true;
        moveSpeed = 1f;
#else
        isUsingKeyboard = false;
        Input.gyro.enabled = true;

        currRotationRate = Input.gyro.rotationRate;
#endif
        speed = moveSpeed;
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        transform.position = Path.Instance.GetPoint(currSeg, progress);
        transform.forward = (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized;
        direction = transform.forward;
        pitch = Vector3.Angle(direction, Vector3.ProjectOnPlane(direction, Vector3.up));
        if (direction.y > 0) pitch *= -1f;
        yaw = Vector3.Angle(Vector3.forward, Vector3.ProjectOnPlane(direction, Vector3.up));
        if (direction.x > 0) yaw *= -1f;

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

        if (isUsingKeyboard)
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
                direction += transform.up * pressTime;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                direction += transform.up * -pressTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                direction += transform.right * -pressTime;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                direction += transform.right * pressTime;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                direction += transform.forward * pressTime;
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

            if (currZoneState == PlayerZoneState.Vein)
            {
                Vector3 pointOnLine = GetClosestPointOnLine(currSeg);
                Vector3 playerToPoint = pointOnLine - transform.position;
                if (playerToPoint.sqrMagnitude >= maxDistFromCenterSqrt)
                {
                    // maybe i need to prevent this position to be updated
                    transform.position = Vector3.Lerp(pointOnLine, transform.position, 0.85f);
                    speed = 0f;
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
                float maxDistSqrt = (currZone.Radius - 0.6f) * (currZone.Radius - 0.6f);
                Vector3 playerToCenter = currZone.transform.position - transform.position;

                if (playerToCenter.sqrMagnitude >= maxDistSqrt)
                {
                    transform.position = Vector3.Lerp(currZone.transform.position, transform.position, 0.95f);
                    speed = 0f;
                }
                else
                {
                    Move();
                }
            }
        }
        else
        {
            currRotationRate = Input.gyro.rotationRate;
            Quaternion gyroQuaternion = GyroToUnity(Input.gyro.attitude);
            Quaternion calculatedRotation = correctionQuaternion * gyroQuaternion;
            Vector3 right = calculatedRotation * Vector3.right;
            float dRoll = Vector3.Angle(right, Vector3.ProjectOnPlane(right, Vector3.up));
            if (right.y > 0) dRoll *= -1f;

            Vector3 forward = calculatedRotation * Vector3.forward;
            float dPitch = Vector3.Angle(forward, Vector3.ProjectOnPlane(forward, Vector3.up));
            if (forward.y > 0) dPitch *= -1f;

            // Quaternion dRotation = Quaternion.Euler(dPitch * pitchRotationSpeed * Time.deltaTime, dRoll * pitchRotationSpeed * Time.deltaTime, 0f);
            // direction = dRotation * direction;
            if (currZoneState == PlayerZoneState.Vein)
            {
                Vector3 pointOnLine = GetClosestPointOnLine(currSeg);
                Vector3 playerToPoint = pointOnLine - transform.position;
                if (playerToPoint.sqrMagnitude >= maxDistFromCenterSqrt)
                {
                    // maybe i need to prevent this position to be updated
                    transform.position = Vector3.Lerp(pointOnLine, transform.position, 0.85f);
                    speed = 0f;
                }
                else
                {
                    Move(dRoll, dPitch);
                }
            }
            else
            {
                // zone
                Debug.Assert(currZone != null);
                float maxDistSqrt = (currZone.Radius - 0.6f) * (currZone.Radius - 0.6f);
                Vector3 playerToCenter = currZone.transform.position - transform.position;

                if (playerToCenter.sqrMagnitude >= maxDistSqrt)
                {
                    transform.position = Vector3.Lerp(currZone.transform.position, transform.position, 0.95f);
                    speed = 0f;
                }
                else
                {
                    Move(dRoll, dPitch);
                }
            }
        }
    }

    private void Move(float dRoll, float dPitch)
    {
        pitch += dPitch * Time.deltaTime * pitchRotationSpeed;
        yaw += dRoll * Time.deltaTime * rollRotationSpeed;

        pitch = Mathf.Clamp(pitch, -89f, 89f);
        if (yaw > 180f) yaw -= 360f;
        if (yaw < -180f) yaw += 360f;

        UIController.Instance.SetDebugText("pitch: " + pitch + " | yaw: " + yaw);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        direction = rot * Vector3.forward;
        speed = moveSpeed;
        transform.position += direction * Time.deltaTime * speed;

        transform.rotation = rot;
        // if (direction != Vector3.zero) transform.forward = direction;
    }

    private void Move()
    {
        speed = moveSpeed;
        direction *= velocityMultiplier;
        Vector3 velocity = Vector3.ClampMagnitude(direction, maxSpeed);
        transform.position += velocity * Time.deltaTime * speed;
        transform.forward = Vector3.SmoothDamp(transform.forward, velocity, ref currVelocity, speed);
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
        if (currSeg.n0.type == NodeType.OxygenEntrance || currSeg.n0.type == NodeType.Oxygen)
        {
            currZoneState = PlayerZoneState.OxygenArea;
            currZone = Path.Instance.OxygenZone;
        }
        else if (currSeg.n0.type == NodeType.Heart || currSeg.n0.type == NodeType.Heart)
        {
            currZoneState = PlayerZoneState.HeartArea;
            currZone = Path.Instance.HeartZone;
        }

        for (int i = 0; i < Path.Instance.zones.Count; i++)
        {
            var curr = Path.Instance.zones[i];
            float maxDistSqrt = (curr.Radius) * (curr.Radius);
            if ((curr.transform.position - transform.position).sqrMagnitude < maxDistSqrt)
            {
                // vein seg is close enough and the direction is pretty much the same
                if (Vector3.Dot(transform.forward, (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized) > 0.3f
                && (GetClosestPointOnLine(currSeg) - transform.position).sqrMagnitude < maxDistFromCenterSqrt && !isSegmentInTheZone(currSeg))
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

    private bool isSegmentInTheZone(Segment segment)
    {
        var startNode = segment.n0;
        var endNode = segment.n1;
        if (startNode.type == NodeType.OxygenEntrance || endNode.type == NodeType.Oxygen || startNode.type == NodeType.HeartEntrance || endNode.type == NodeType.Heart) return true;
        return false;
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
        Gizmos.DrawLine(transform.position, transform.position + direction);
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
