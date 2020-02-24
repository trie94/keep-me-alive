using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmearEffect : MonoBehaviour
{
    private Material _smearMat = null;
    private Renderer rend;
    private Vector3 _prevPosition;

    public Material smearMat
    {
        get
        {
            if (!_smearMat)
                _smearMat = rend.sharedMaterial;

            if (!_smearMat.HasProperty("_PrevPosition"))
                _smearMat.shader = Shader.Find("Unlit/Cell");

            return _smearMat;
        }
    }

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void Start()
    {
        _prevPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 velocity = (transform.position - _prevPosition) / Time.deltaTime;
        smearMat.SetVector("_Velocity", velocity);
        smearMat.SetVector("_Position", transform.position);
        _prevPosition = transform.position;
    }
}
