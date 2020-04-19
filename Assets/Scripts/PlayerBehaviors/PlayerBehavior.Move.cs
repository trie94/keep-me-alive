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
    private float zoneCollisionRadiusFactor = 0.1f;

    private float pitchRotationSpeed = 0.3f;
    private float rollRotationSpeed = 0.2f;
    private float pitch;
    private float yaw;
    private float currentFactor = 1f;

    private void InitMovement()
    {
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        transform.position = Path.Instance.GetPoint(currSeg, Random.Range(0f, 1f));
        transform.forward = direction = current = currSeg.Direction;
        yaw = transform.rotation.eulerAngles.y;
        pitch = transform.rotation.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        if (pitch < -180f) pitch += 360f;
        maxDistFromCenterSqr = maxDistFromCenter * maxDistFromCenter;
        UpdateZoneState();
    }

    private void UpdateMovement()
    {
        UpdateSegmentAndCurrent();
        UpdateZoneState();

        Vector2 turn = InputManager.Instance.Turn;
        speed = InputManager.Instance.Speed;
        Move(turn.x, -turn.y);
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
        velocity = direction * speed + current * currentFactor;
        Vector3 position = transform.position + velocity * Time.deltaTime;
        transform.rotation = rot;
        if (!IsInsideTheBody(position))
        {
            position = ConstrainPosition(position);
        }
        transform.position = position;
    }

    private Vector3 ConstrainPosition(Vector3 position)
    {
        Vector3 centerPosition = (currZoneState == PlayerZoneState.Vein) ?
                CurrentManager.Instance.GetClosestPointOnLine(currSeg, position) : currZone.transform.position;
        Vector3 playerToWall = position - centerPosition;
        playerToWall.Normalize();

        float thresholdDist = (currZoneState == PlayerZoneState.Vein) ? maxDistFromCenter : currZone.Radius - zoneCollisionRadiusFactor;
        position = centerPosition + playerToWall * thresholdDist;
        return position;
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
    private bool IsInsideTheBody(Vector3 position)
    {
        float sqrDistBetweenPlayerAndStartNode = (position - currSeg.start.transform.position).sqrMagnitude;
        float sqrDistBetweenPlayerAndEndNode = (position - currSeg.end.transform.position).sqrMagnitude;
        return sqrDistBetweenPlayerAndStartNode <= sqrDistBetweenPlayerAndEndNode ? IsInside(currSeg.start, position) : IsInside(currSeg.end, position);
    }

    private bool IsInside(Node node, Vector3 position)
    {
        // zone
        if (currZone)
        {
            float maxDistSqr = (currZone.Radius - zoneCollisionRadiusFactor) * (currZone.Radius - zoneCollisionRadiusFactor);
            if (isInSphere(currZone.transform.position, maxDistSqr, position)) return true;
        }
        else
        {
            // joint
            if (isInSphere(node.transform.position, maxDistFromCenterSqr, position)) return true;
        }

        // next tubes
        for (int i=0; i < node.nextSegments.Count; i++)
        {
            if (IsInSegment(node.nextSegments[i], position)) return true;
        }

        // prev tubes
        for (int i = 0; i < node.prevSegments.Count; i++)
        {
            if (IsInSegment(node.prevSegments[i], position)) return true;
        }

        return false;
    }

    private bool IsInSegment(Segment seg, Vector3 position)
    {
        Vector3 pointOnLine = CurrentManager.Instance.GetClosestPointOnLine(seg, position);
        Vector3 playerToPoint = pointOnLine - position;
        if (playerToPoint.sqrMagnitude <= maxDistFromCenterSqr)
        {
            return true;
        }
        return false;
    }

    private bool isInSphere(Vector3 center, float maxDistSqr, Vector3 position)
    {
        Vector3 playerToCenter = center - position;
        if (playerToCenter.sqrMagnitude <= maxDistSqr)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region debug
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
