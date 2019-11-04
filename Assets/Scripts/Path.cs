using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Path : MonoBehaviour
{
    private Vector3[] vertices;
    private Mesh mesh;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        Debug.Log(mesh);

        // for (int i = 0; i < vertices.Length; i+=100)
        // {
        //     BoxCollider col = this.gameObject.AddComponent<BoxCollider>();
        //     col.center = vertices[i];
        //     col.size = new Vector3(0.5f, 0.5f, 0.5f);
        // }
    }
}
