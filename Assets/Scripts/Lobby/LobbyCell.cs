using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCell : MonoBehaviour
{
    [SerializeField]
    private CellType celltype;
    [SerializeField]
    private GameObject limbPrefab;

    [SerializeField]
    private int maxLimbNum = 10;
    private List<GameObject> limbs;
    private float speed;
    private Vector3 endPosition;
    private SmearEffect smearEffect;

    private void Awake()
    {
        smearEffect = GetComponentInChildren<SmearEffect>();
        if (celltype == CellType.Platelet)
        {
            limbs = new List<GameObject>();
            InitLimbs();
        }
        Reset();
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, endPosition, Time.deltaTime * speed);

        if ((LobbyGameController.Instance.pathEnd.position - transform.position).sqrMagnitude < 0.5f)
        {
            Reset();
        }
    }

    private void Reset()
    {
        endPosition = LobbyGameController.Instance.GetEndPosition();
        // remove weird artifact
        transform.position = smearEffect.PrevPosition = LobbyGameController.Instance.GetStartPosition();
        speed = Random.Range(0.05f, 0.2f);
    }

    private void InitLimbs()
    {
        int limbNum = Random.Range(5, maxLimbNum);
        Quaternion rotation = Quaternion.identity;
        int length = Shader.PropertyToID("_Length");
        int thickness = Shader.PropertyToID("_Thickness");
        int limbSpeed = Shader.PropertyToID("_Speed");

        for (int i = 0; i < limbNum; i++)
        {
            rotation = Random.rotationUniform;
            GameObject limb = Instantiate(limbPrefab, transform.position, rotation, transform);
            Material limbMat = limb.GetComponent<Renderer>().material;
            limbMat.SetFloat(length, Random.Range(2f, 2.3f));
            limbMat.SetFloat(thickness, Random.Range(1.0f, 1.2f));
            limbMat.SetFloat(limbSpeed, Random.Range(1.4f, 1.8f));
            limbs.Add(limb);
        }
    }
}
