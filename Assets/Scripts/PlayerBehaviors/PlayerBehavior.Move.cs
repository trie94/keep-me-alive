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
    private float maxDistFromCenterSqrt;

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
    #endregion

    private void InitMovement()
    {
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        transform.position = Path.Instance.GetPoint(currSeg, Random.Range(0f, 1f));
        transform.forward = (currSeg.n1.transform.position - currSeg.n0.transform.position).normalized;
        direction = transform.forward;

        //debugIndicatorOnLine = Instantiate(debugSphere, transform.position, transform.rotation);
        maxDistFromCenterSqrt = maxDistFromCenter * maxDistFromCenter;
        UpdateZoneState();
    }

    private void UpdateMovement()
    {
        UpdateZoneState();
        UpdateCurrSeg();

        Vector2 turn = InputManager.Instance.Turn;
        speed = InputManager.Instance.Speed;

        if (currZoneState == PlayerZoneState.Vein)
        {
            if (keepInTunnel)
            {
                Vector3 pointOnLine = GetClosestPointOnLine(currSeg);
                Vector3 playerToPoint = pointOnLine - transform.position;
                if (playerToPoint.sqrMagnitude >= maxDistFromCenterSqrt)
                {
                    transform.position = Vector3.Lerp(pointOnLine, transform.position, 0.85f);
                    speed = 0f;
                }
                else
                {
                    Move(turn.x, -turn.y);
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
            float maxDistSqrt = (currZone.Radius - 0.6f) * (currZone.Radius - 0.6f);
            Vector3 playerToCenter = currZone.transform.position - transform.position;

            if (keepInTunnel)
            {
                if (playerToCenter.sqrMagnitude >= maxDistSqrt)
                {
                    transform.position = Vector3.Lerp(currZone.transform.position, transform.position, 0.95f);
                    speed = 0f;
                }
                else
                {
                    Move(turn.x, -turn.y);
                }
            }
            else
            {
                Move(turn.x, -turn.y);
            }
        }

        //MoveDebugIndicator();
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

    private void MoveDebugIndicator()
    {
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
