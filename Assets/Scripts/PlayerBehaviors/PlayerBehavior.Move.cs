﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerBehavior : MonoBehaviour
{
    private Vector3 direction;
    public Vector3 Direction { get { return direction; } }
    private float speed;
    private Segment currSeg;
    private Zone currZone;
    private Vector3 velocity;
    public Vector3 Velocity { get { return velocity; } }

    [SerializeField]
    private float maxDistFromCenter = 3.9f;
    private float maxDistFromCenterSqr;
    private float zoneCollisionRadiusFactor = 0f;

    private float pitchRotationSpeed = 0.3f;
    private float rollRotationSpeed = 0.2f;
    private float pitch;
    private float yaw;

    #region Debug
    [SerializeField]
    private GameObject debugSphere;
    private GameObject debugIndicatorOnLine;
    [SerializeField]
    private bool keepInTunnel;
    private bool isInsideTheBody;
    #endregion

    private void InitMovement()
    {
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        transform.position = Path.Instance.GetPoint(currSeg, Random.Range(0f, 1f));
        transform.forward = (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized;
        direction = transform.forward;
        yaw = transform.rotation.eulerAngles.y;
        pitch = transform.rotation.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        if (pitch < -180f) pitch += 360f;
        maxDistFromCenterSqr = maxDistFromCenter * maxDistFromCenter;
        UpdateZoneState();
        //debugIndicatorOnLine = Instantiate(debugSphere);
    }

    private void UpdateMovement()
    {
        UpdateCurrSeg();
        UpdateZoneState();
        isInsideTheBody = IsInsideTheBody();

        Vector2 turn = InputManager.Instance.Turn;
        speed = InputManager.Instance.Speed;

        if (keepInTunnel)
        {
            Move(turn.x, -turn.y, !isInsideTheBody);
        }
        else
        {
            Move(turn.x, -turn.y, false);
        }
        //MoveDebugIndicator();
    }

    private void Move(float dRoll, float dPitch, bool isColliding)
    {
        pitch += dPitch * Time.deltaTime * pitchRotationSpeed;
        yaw += dRoll * Time.deltaTime * rollRotationSpeed;

        pitch = Mathf.Clamp(pitch, -45f, 45f);
        if (yaw > 180f) yaw -= 360f;
        if (yaw < -180f) yaw += 360f;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        direction = rot * Vector3.forward;
        if (isColliding)
        {
            // cancel out the direction by adding the opposite vector
            Vector3 wallToPlayer = (currZoneState == PlayerZoneState.Vein) ?
                    GetClosestPointOnLine(currSeg) - transform.position
                    : currZone.transform.position - transform.position;
            wallToPlayer.Normalize();

            // lerp to avoid jitter
            float dot = Vector3.Dot(wallToPlayer, direction);
            direction = Vector3.Lerp(direction, Vector3.Project(direction, wallToPlayer) * Mathf.Sign(dot), 0.5f);
        }
        velocity = direction * speed;
        transform.position += velocity * Time.deltaTime;
        transform.rotation = rot;
    }

    // we need to check all the possible segments nearby because this does not
    // reflect the direction of the cell. If we don't count each node's prev and
    // next segments, it will cause an issue especially in the zone, where start
    // and end node is mixed up. In order to avoid the same computation,
    // HashSet is used.
    private void UpdateCurrSeg()
    {
        Segment potential = currSeg;
        float min = (GetClosestPointOnLine(potential) - transform.position).sqrMagnitude;

        HashSet<Segment> computedSegments = new HashSet<Segment>();
        computedSegments.Add(potential);

        var currStartNode = currSeg.n0;
        var currEndNode = currSeg.n1;

        for (int i = 0; i < currStartNode.prevSegments.Count; i++)
        {
            var seg = currStartNode.prevSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float distSqr = (GetClosestPointOnLine(seg) - transform.position).sqrMagnitude;
                if (distSqr < min)
                {
                    potential = seg;
                    min = distSqr;
                }
                computedSegments.Add(seg);
            }
        }

        for (int i = 0; i < currStartNode.nextSegments.Count; i++)
        {
            var seg = currStartNode.nextSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float distSqr = (GetClosestPointOnLine(seg) - transform.position).sqrMagnitude;
                if (distSqr < min)
                {
                    potential = seg;
                    min = distSqr;
                }
                computedSegments.Add(seg);
            }
        }

        for (int i = 0; i < currEndNode.prevSegments.Count; i++)
        {
            var seg = currEndNode.prevSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg) - transform.position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
            }
        }

        for (int i = 0; i < currEndNode.nextSegments.Count; i++)
        {
            var seg = currEndNode.nextSegments[i];
            if (!computedSegments.Contains(seg))
            {
                float sqrDist = (GetClosestPointOnLine(seg) - transform.position).sqrMagnitude;
                if (sqrDist < min)
                {
                    potential = seg;
                    min = sqrDist;
                }
                computedSegments.Add(seg);
            }
        }

        currSeg = potential;
    }

    private void UpdateZoneState()
    {
        if (currSeg.n0.type == NodeType.OxygenEntrance || currSeg.n0.type == NodeType.Oxygen)
        {
            currZoneState = PlayerZoneState.OxygenArea;
            currZone = Path.Instance.OxygenZone;
        }
        else if (currSeg.n0.type == NodeType.HeartEntrance || currSeg.n0.type == NodeType.Heart)
        {
            currZoneState = PlayerZoneState.HeartArea;
            currZone = Path.Instance.HeartZone;
        }
        else
        {
            currZoneState = PlayerZoneState.Vein;
            currZone = null;
        }
    }

    private Vector3 GetClosestPointOnLine(Segment seg)
    {
        Vector3 StartPointToPlayer = transform.position - seg.n0.transform.position;
        Vector3 segDir = (seg.n1.transform.position - seg.n0.transform.position);
        float t = Mathf.Clamp01(Vector3.Dot(StartPointToPlayer, segDir) / segDir.sqrMagnitude);
        return seg.n0.transform.position + t * segDir;
    }

    #region collision detection
    private bool IsInsideTheBody()
    {
        float sqrDistBetweenPlayerAndStartNode = (transform.position - currSeg.n0.transform.position).sqrMagnitude;
        float sqrDistBetweenPlayerAndEndNode = (transform.position - currSeg.n1.transform.position).sqrMagnitude;
        return sqrDistBetweenPlayerAndStartNode <= sqrDistBetweenPlayerAndEndNode ? IsInside(currSeg.n0) : IsInside(currSeg.n1);
    }

    private bool IsInside(Node node)
    {
        // zone
        if (currZone)
        {
            float maxDistSqr = (currZone.Radius - zoneCollisionRadiusFactor) * (currZone.Radius - zoneCollisionRadiusFactor);
            if (isInSphere(currZone.transform.position, maxDistSqr)) return true;
        }
        else
        {
            // joint
            if (isInSphere(node.transform.position, maxDistFromCenterSqr)) return true;
        }

        // next tubes
        for (int i=0; i < node.nextSegments.Count; i++)
        {
            if (IsInSegment(node.nextSegments[i])) return true;
        }

        // prev tubes
        for (int i = 0; i < node.prevSegments.Count; i++)
        {
            if (IsInSegment(node.prevSegments[i])) return true;
        }

        return false;
    }

    private bool IsInSegment(Segment seg)
    {
        Vector3 pointOnLine = GetClosestPointOnLine(seg);
        Vector3 playerToPoint = pointOnLine - transform.position;
        if (playerToPoint.sqrMagnitude <= maxDistFromCenterSqr)
        {
            return true;
        }
        return false;
    }

    private bool isInSphere(Vector3 center, float maxDistSqr)
    {
        Vector3 playerToCenter = center - transform.position;
        if (playerToCenter.sqrMagnitude <= maxDistSqr)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region debug
    private void MoveDebugIndicator()
    {
        debugIndicatorOnLine.transform.position = GetClosestPointOnLine(currSeg);
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
        Gizmos.DrawLine(transform.position, GetClosestPointOnLine(currSeg));
    }
    #endregion
}