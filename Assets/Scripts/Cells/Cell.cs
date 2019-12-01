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
    public Vector3 currVelocity;
    protected Renderer rend;
    private int faceID;
    [SerializeField]
    private float timeInterval;
    private float tick = 0f;
    private int frameIndex = 0;
    private Texture2D[] currEmotion;
    public float emotionPickInterval;

    public float progress = 0f;
    public float speed;
    public Segment currSeg;

    public virtual void Awake()
    {
        cellCollider = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        emotionPickInterval = Random.Range(5f, 10f);
        speed = Random.Range(0.5f, 2f);
    }

    public virtual void Start()
    {
        // set initial position
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        transform.position = Path.Instance.GetPoint(currSeg, Random.Range(0f, 1f));
    }

    public virtual void Move(Vector3 velocity)
    {
        if (velocity != Vector3.zero) currVelocity = velocity;
        transform.position += currVelocity * Time.deltaTime;
        transform.up = currVelocity;
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

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawLine(transform.position, transform.position + vel);
    //}
}
