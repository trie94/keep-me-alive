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
    private bool isEditor;
    private Quaternion currAttitude;
    private Quaternion prevAttitude;

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
    [SerializeField]
    private float maxDistFromCenter = 3.9f;
    private float maxDistFromCenterSqrt;
    [SerializeField]  // for debugging
    private PlayerZoneState currZoneState;
    private Zone currZone;

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

        currAttitude = Input.gyro.attitude;
        prevAttitude = currAttitude;
#endif
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        transform.position = Path.Instance.GetPoint(currSeg, progress);
        transform.forward = (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized;
        velocity = Vector3.zero;
        debugIndicatorOnLine = Instantiate(debugSphere, transform.position, transform.rotation);
        currZoneState = PlayerZoneState.Vein;

        maxDistFromCenterSqrt = maxDistFromCenter * maxDistFromCenter;
    }

    private void Update()
    {
        // update curr seg and zone state
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
            // UIController.Instance.SetDebugText("attitude: " + Input.gyro.attitude);
            currAttitude = Input.gyro.attitude;
            UIController.Instance.SetDebugText("ATTITUDE : " + Input.gyro.attitude);
            velocity += transform.up * -(currAttitude.x - prevAttitude.x) * 5f;
            velocity += transform.right * (currAttitude.z - prevAttitude.z) * 5f;
            velocity += transform.forward;
        }

        PushBackFromWall();
        // move
        velocity *= velocityMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
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
        switch (currSeg.n0.type)
        {
            case NodeType.HeartEntrance:
            case NodeType.Heart:
                currZoneState = PlayerZoneState.HeartArea;
                currZone = Path.Instance.HeartZone;
                break;
            case NodeType.OxygenEntrance:
            case NodeType.Oxygen:
                currZoneState = PlayerZoneState.OxygenArea;
                currZone = Path.Instance.OxygenZone;
                break;
            case NodeType.OxyenExit:
            case NodeType.HeartExit:
                float maxDistSqrt = (currZone.Radius - 0.5f) * (currZone.Radius - 0.5f);
                Vector3 playerToCenter = currZone.transform.position - transform.position;
                if (playerToCenter.sqrMagnitude > maxDistSqrt)
                {
                    currZoneState = PlayerZoneState.Vein;
                }
                break;
            default:
                currZoneState = PlayerZoneState.Vein;
                currZone = null;
                break;
        }
    }

    private void PushBackFromWall()
    {
        if (currZoneState == PlayerZoneState.Vein)
        {
            Vector3 pointOnLine = GetClosestPointOnLine(currSeg);
            Vector3 playerToPoint = pointOnLine - transform.position;
            if (playerToPoint.sqrMagnitude > maxDistFromCenterSqrt)
            {
                // push back
                velocity = playerToPoint * playerToPoint.magnitude;
            }
        }
        else
        {
            // zone
            Debug.Assert(currZone != null);
            float maxDistSqrt = currZone.Radius * currZone.Radius;
            Vector3 playerToCenter = currZone.transform.position - transform.position;

            if (playerToCenter.sqrMagnitude > maxDistSqrt)
            {
                velocity = playerToCenter * playerToCenter.magnitude;
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
}
