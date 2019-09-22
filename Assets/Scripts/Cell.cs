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

    private void Awake()
    {
        cellCollider = GetComponent<Collider>();
    }

    public void Move(Vector3 velocity)
    {
        if (velocity != Vector3.zero) transform.forward += velocity;
        transform.position += velocity * Time.deltaTime;
    }
}
