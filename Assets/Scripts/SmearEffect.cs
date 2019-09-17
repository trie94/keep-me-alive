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
    public Material smearMat
    {
        get
        {
            if (!_smearMat)
                _smearMat = rend.material;

            if (!_smearMat.HasProperty("_PrevPosition"))
                _smearMat.shader = Shader.Find("Custom/Smear");

            return _smearMat;
        }
    }

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (_recentPositions.Count > _frameLag)
            smearMat.SetVector("_PrevPosition", _recentPositions.Dequeue());

        smearMat.SetVector("_Position", transform.position);
        _recentPositions.Enqueue(transform.position);
    }
}
