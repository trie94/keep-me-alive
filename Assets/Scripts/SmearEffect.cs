using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmearEffect : MonoBehaviour
{
    private Material smearMat = null;
    private Renderer rend;
    private Vector3 prevPosition;
    public Vector3 PrevPosition { set { prevPosition = value; } get { return prevPosition; } }
    private int velocityId;
    private int positionId;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        smearMat = rend.material;
        prevPosition = transform.position;
        velocityId = Shader.PropertyToID("_Velocity");
        positionId = Shader.PropertyToID("_Position");
    }

    private void LateUpdate()
    {
        Vector3 velocity = (transform.position - prevPosition) / Time.deltaTime;
        smearMat.SetVector(velocityId, velocity);
        smearMat.SetVector(positionId, transform.position);
        prevPosition = transform.position;
    }
}
