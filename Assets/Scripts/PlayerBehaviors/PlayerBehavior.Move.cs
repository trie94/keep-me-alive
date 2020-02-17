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
        yaw = transform.rotation.eulerAngles.y;
        pitch = transform.rotation.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        if (pitch < -180f) pitch += 360f;

        maxDistFromCenterSqr = maxDistFromCenter * maxDistFromCenter;
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
                if (playerToPoint.sqrMagnitude > maxDistFromCenterSqr)
                {
                    transform.position = pointOnLine - playerToPoint.normalized * (maxDistFromCenter - 0.001f);
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
            Vector3 playerToCenter = currZone.transform.position - transform.position;
            float maxDistSqr = currZone.Radius * currZone.Radius;

            if (keepInTunnel)
            {
                if (playerToCenter.sqrMagnitude > maxDistSqr)
                {
                    transform.position = currZone.transform.position - playerToCenter.normalized * (currZone.Radius - 0.001f);
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
            float distSqr = (GetClosestPointOnLine(seg) - transform.position).sqrMagnitude;
            if (distSqr < min)
            {
                potential = seg;
                min = distSqr;
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
