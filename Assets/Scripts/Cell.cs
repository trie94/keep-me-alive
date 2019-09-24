using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Cell : MonoBehaviour
{
    private Collider cellCollider;
    public Collider CellCollider
    {
        get { return cellCollider; }
    }
    private Vector3 vel;
    private Renderer rend;

    private void Awake()
    {
        cellCollider = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }

    public void Move(Vector3 velocity)
    {
        if (velocity != Vector3.zero)
        {
            vel = velocity;
        }
        transform.position += vel * Time.deltaTime;
        transform.up = vel;
    }

    public void AssignFace(Texture2D texture)
    {
        rend.material.SetTexture("_Face", texture);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + vel);
    }
}
