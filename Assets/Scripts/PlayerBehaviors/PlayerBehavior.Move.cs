using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerBehavior : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private Segment currSeg;
    private Zone currZone;

    [SerializeField]
    private float maxDistFromCenter = 3.9f;
    private float maxDistFromCenterSqr;
    private float bufferInZoneWhenCollide = 0.2f;

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
    [SerializeField]
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
        debugIndicatorOnLine = Instantiate(debugSphere);
    }

    private void UpdateMovement()
    {
        UpdateCurrSeg();
        UpdateZoneState();
        isInsideTheBody = IsInsideTheBody();

        Vector2 turn = InputManager.Instance.Turn;
        speed = InputManager.Instance.Speed;

        if (currZoneState == PlayerZoneState.Vein)
        {
            if (keepInTunnel)
            {
                if (isInsideTheBody)
                {
                    Move(turn.x, -turn.y);
                }
                else
                {
                    Vector3 pointOnLine = GetClosestPointOnLine(currSeg);
                    Vector3 playerToPoint = pointOnLine - transform.position;
                    transform.position = pointOnLine - playerToPoint.normalized * (maxDistFromCenter - 0.001f);
                    speed = 0f;
                }
            }
            else
            {
                Move(turn.x, -turn.y);
            }
        }
        else
        {
            Debug.Assert(currZone != null);
            if (keepInTunnel)
            {
                if (isInsideTheBody)
                {
                    Move(turn.x, -turn.y);
                }
                else
                {
                    Vector3 playerToCenter = currZone.transform.position - transform.position;
                    transform.position = currZone.transform.position - playerToCenter.normalized * (currZone.Radius - bufferInZoneWhenCollide - 0.001f);
                    speed = 0f;
                }
            }
            else
            {
                Move(turn.x, -turn.y);
            }
        }

        MoveDebugIndicator();
    }

    private void Move(float dRoll, float dPitch)
    {
        pitch += dPitch * Time.deltaTime * pitchRotationSpeed;
        yaw += dRoll * Time.deltaTime * rollRotationSpeed;

        pitch = Mathf.Clamp(pitch, -45f, 45f);
        if (yaw > 180f) yaw -= 360f;
        if (yaw < -180f) yaw += 360f;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        direction = rot * Vector3.forward;
        transform.position += direction * Time.deltaTime * speed;
        transform.rotation = rot;
    }

    private void UpdateCurrSeg()
    {
        Segment potential = currSeg;
        float min = (GetClosestPointOnLine(potential) - transform.position).sqrMagnitude;
        var currStartNode = currSeg.n0;
        var currEndNode = currSeg.n1;

        for (int i = 0; i < currStartNode.prevSegments.Count; i++)
        {
            var seg = currStartNode.prevSegments[i];
            float distSqr = (GetClosestPointOnLine(seg) - transform.position).sqrMagnitude;
            if (distSqr < min)
            {
                potential = seg;
                min = distSqr;
            }
        }

        for (int i = 0; i < currEndNode.nextSegments.Count; i++)
        {
            var seg = currEndNode.nextSegments[i];
            float sqrDist = (GetClosestPointOnLine(seg) - transform.position).sqrMagnitude;
            if (sqrDist < min)
            {
                potential = seg;
                min = sqrDist;
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

    // collision detection
    private bool IsInsideTheBody()
    {
        float sqrDistBetweenPlayerAndStartNode = (transform.position - currSeg.n0.transform.position).sqrMagnitude;
        float sqrDistBetweenPlayerAndEndNode = (transform.position - currSeg.n1.transform.position).sqrMagnitude;
        return sqrDistBetweenPlayerAndStartNode <= sqrDistBetweenPlayerAndEndNode ? IsInside(currSeg.n0) : IsInside(currSeg.n1);
    }

    private bool IsInside(Node node)
    {
        bool isInside = false;

        if (currZone)
        {
            float maxDistSqr = (currZone.Radius - bufferInZoneWhenCollide) * (currZone.Radius - bufferInZoneWhenCollide);
            isInside = isInside || isInSphere(currZone.transform.position, maxDistSqr);
        }

        isInside = isInside || isInSphere(node.transform.position, maxDistFromCenterSqr);
        for (int i=0; i < node.nextSegments.Count; i++)
        {
            isInside = isInside || IsInSegment(node.nextSegments[i]);
        }

        for (int i = 0; i < node.prevSegments.Count; i++)
        {
            isInside = isInside || IsInSegment(node.prevSegments[i]);
        }

        return isInside;
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

    // debug
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
