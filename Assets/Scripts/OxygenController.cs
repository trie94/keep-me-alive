using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenController : MonoBehaviour
{
    private static OxygenController instance;
    public static OxygenController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<OxygenController>();
            }
            return instance;
        }
    }

    [SerializeField]
    private Oxygen oxygenPrefab;
    public int initialOxygenNumber;
    public Dictionary<Transform, Oxygen> oxygenMap;
    public Stack<Oxygen> oxygens;
    private float density = 0.03f;

    private void Awake()
    {
        instance = this;
        oxygenMap = new Dictionary<Transform, Oxygen>();
        oxygens = new Stack<Oxygen>();
    }

    private void Start()
    {
        SpawnOxygen();
    }

    private void SpawnOxygen()
    {
        for (int i = 0; i < initialOxygenNumber; i++)
        {
            Oxygen oxygen = Instantiate(
                oxygenPrefab,
                CellController.Instance.oxygenArea.position
                + Random.insideUnitSphere * initialOxygenNumber * density,
                Random.rotation
            );
            oxygenMap.Add(oxygen.transform, oxygen);
            oxygens.Push(oxygen);
        }
    }

    public Vector3 GetRandomPositionInOxygenArea()
    {
        return CellController.Instance.oxygenArea.position
                         + Random.insideUnitSphere
                         * initialOxygenNumber * density;
    }
}
