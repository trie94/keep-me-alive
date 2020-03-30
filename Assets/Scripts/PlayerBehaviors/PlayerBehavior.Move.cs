using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerBehavior : MonoBehaviour
{
    private Vector3 direction;
    public Vector3 Direction { get { return direction; } }
    private float speed;
    private Segment currSeg;
    private Vector3 current;
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
    private float currentFactor = 0.01f;

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
        current = (currSeg.end.transform.position - currSeg.start.transform.position).normalized;
        transform.position = Path.Instance.GetPoint(currSeg, Random.Range(0f, 1f));
        transform.forward = (currSeg.end.transform.position - currSeg.start.transform.position).normalized;
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
        UpdateSegmentAndCurrent();
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
                    CurrentManager.Instance.GetClosestPointOnLine(currSeg, transform.position) - transform.position
                    : currZone.transform.position - transform.position;
            wallToPlayer.Normalize();

            // lerp to avoid jitter
            float dot = Vector3.Dot(wallToPlayer, direction);
            direction = Vector3.Lerp(direction, Vector3.Project(direction, wallToPlayer) * Mathf.Sign(dot), 0.5f);
        }

        velocity = direction * speed;
        transform.position += velocity * Time.deltaTime;
        transform.position += current * currentFactor;
        transform.rotation = rot;
    }

    private void UpdateSegmentAndCurrent()
    {
        currSeg = CurrentManager.Instance.UpdateCurrentSegment(currSeg, transform.position);
        current = CurrentManager.Instance.GetCurrent(currZoneState, currSeg, transform.position);
    }

    private void UpdateZoneState()
    {
        if (currSeg.start.type == NodeType.OxygenEntrance || currSeg.start.type == NodeType.Oxygen)
        {
            currZoneState = PlayerZoneState.OxygenArea;
            currZone = Path.Instance.OxygenZone;
        }
        else if (currSeg.start.type == NodeType.HeartEntrance || currSeg.start.type == NodeType.Heart)
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

    #region collision detection
    private bool IsInsideTheBody()
    {
        float sqrDistBetweenPlayerAndStartNode = (transform.position - currSeg.start.transform.position).sqrMagnitude;
        float sqrDistBetweenPlayerAndEndNode = (transform.position - currSeg.end.transform.position).sqrMagnitude;
        return sqrDistBetweenPlayerAndStartNode <= sqrDistBetweenPlayerAndEndNode ? IsInside(currSeg.start) : IsInside(currSeg.end);
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
        Vector3 pointOnLine = CurrentManager.Instance.GetClosestPointOnLine(seg, transform.position);
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
        debugIndicatorOnLine.transform.position = CurrentManager.Instance.GetClosestPointOnLine(currSeg, transform.position);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        // highlight current
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + current);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, CurrentManager.Instance.GetClosestPointOnLine(currSeg, transform.position));
    }
    #endregion
}
