using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platelet : MonoBehaviour
{
    private Renderer rend;
    private Material material;

    [SerializeField]
    private GameObject limbPrefab;

    [SerializeField]
    private int maxLimbNum = 10;

    private List<GameObject> limbs;

    private void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        material = rend.material;
        limbs = new List<GameObject>();
        Init();
    }

    private void Init()
    {
        int limbNum = Random.Range(5, maxLimbNum);
        float zAngle = 360f / limbNum;
        Quaternion rotation = Quaternion.identity;

        for (int i = 0; i < limbNum; i++)
        {
            rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), zAngle * i);
            GameObject limb = Instantiate(limbPrefab, this.transform.position, rotation, this.transform);
            limbs.Add(limb);
        }
    }
}
