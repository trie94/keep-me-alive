using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Oxygen : Cell
{
    private Collider oxygenCollider;
    public Collider OxygenCollider
    {
        get { return oxygenCollider; }
    }
    public Cell master;

    public override void Awake()
    {
        oxygenCollider = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        emotionPickInterval = Random.Range(5f, 10f);
        speed = Random.Range(0.1f, 0.3f);
    }

    public override void Start()
    {
        PickNextEmotionAndReset();
    }
}
