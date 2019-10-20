using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Cell : MonoBehaviour
{
    private Collider cellCollider;
    public Collider CellCollider
    {
        get { return cellCollider; }
    }
    private Vector3 vel;
    protected Renderer rend;
    private int faceID;
    [SerializeField]
    private float timeInterval;
    private float tick = 0f;
    private int frameIndex = 0;
    private Texture2D[] currEmotion;
    public float emotionPickInterval;

    public virtual void Awake()
    {
        cellCollider = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        emotionPickInterval = Random.Range(5f, 10f);
    }

    public virtual void Move(Vector3 velocity)
    {
        if (velocity != Vector3.zero)
        {
            vel = velocity;
        }
        transform.position += vel * Time.deltaTime;
        transform.up = vel;
    }

    public virtual void PlayEmotionAnim(Texture2D[] anim)
    {
        if (anim == null) return;

        if (currEmotion != anim)
        {
            frameIndex = 0;
            emotionPickInterval = Random.Range(5f, 10f);
        }

        // play anim
        if (tick > timeInterval)
        {
            rend.material.SetTexture(faceID, anim[frameIndex]);
            frameIndex = (frameIndex + 1) % anim.Length;
            tick = 0;
        }
        tick += Time.deltaTime;
        currEmotion = anim;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + vel);
    }
}
