using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmearEffect : MonoBehaviour
{
    Queue<Vector3> _recentPositions = new Queue<Vector3>();

    [SerializeField]
    int _frameLag = 0;

    Material _smearMat = null;
    Renderer rend;
    Vector3 _prevPosition;

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

    void Awake()
    {
        rend = GetComponent<Renderer>();
        _prevPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 velocity = (transform.position - _prevPosition) / Time.deltaTime;
        smearMat.SetVector("_Velocity", velocity);

        if (_recentPositions.Count > _frameLag)
            smearMat.SetVector("_PrevPosition", _recentPositions.Dequeue());

        smearMat.SetVector("_Position", transform.position);
        _recentPositions.Enqueue(transform.position);
        _prevPosition = transform.position;
    }
}
