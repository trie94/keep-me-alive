using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platelet : Cell
{
    [SerializeField]
    private GameObject limbPrefab;

    [SerializeField]
    private int maxLimbNum = 10;

    private List<GameObject> limbs;

    public override void Awake()
    {
        base.Awake();
        limbs = new List<GameObject>();
        InitLimbs();
    }

    private void InitLimbs()
    {
        int limbNum = Random.Range(5, maxLimbNum);
        float zAngle = 360f / limbNum;
        Quaternion rotation = Quaternion.identity;
        int length = Shader.PropertyToID("_Length");
        int thickness = Shader.PropertyToID("_Thickness");
        int speed = Shader.PropertyToID("_Speed");

        for (int i = 0; i < limbNum; i++)
        {
            rotation = Random.rotationUniform;
            GameObject limb = Instantiate(limbPrefab, this.transform.position, rotation, this.transform);
            Material limbMat = limb.GetComponent<Renderer>().material;
            limbMat.SetFloat(length, Random.Range(2f, 2.3f));
            limbMat.SetFloat(thickness, Random.Range(1.0f, 1.2f));
            limbMat.SetFloat(speed, Random.Range(1.4f, 1.8f));
            limbs.Add(limb);
        }
    }
}
