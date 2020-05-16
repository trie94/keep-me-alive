using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyFrameSpring : MonoBehaviour
{
    public static float springStrength = 1000f;
    public static float restDistance = 0.5f;
    public static float damp = 1f;
    public static float distanceThreshold = 1.5f;

    public static void ComputeAcceleration(BodyFrame frame1, BodyFrame frame2)
    {
        //    F = -k(|x|-d)(x/|x|) - bv            
        float distanceBetween = (frame1.transform.position - frame2.transform.position).magnitude;

        Vector3 frame1To2 = (frame2.transform.position - frame1.transform.position).normalized;
        Vector3 f1 = springStrength * (distanceBetween - restDistance) * (frame1To2);
        Vector3 f2 = springStrength * (distanceBetween - restDistance) * (-frame1To2);

        frame1.acceleration += f1 / frame1.mass;
        frame2.acceleration += f2 / frame2.mass;
    }

    public static void ComputeAcceleration(BodyFrame frame, Vector3 target)
    {
        //    F = -k(|x|-d)(x/|x|) - bv            
        float distanceBetween = (frame.transform.position - target).magnitude;

        Vector3 frameToTarget = (target - frame.transform.position).normalized;
        Vector3 f1 = springStrength * (distanceBetween - restDistance) * (frameToTarget);
        Vector3 f2 = springStrength * (distanceBetween - restDistance) * (-frameToTarget);

        frame.acceleration += f1 / frame.mass;
    }
}