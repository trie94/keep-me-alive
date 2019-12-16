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
    [SerializeField]
    private int initialOxygenNumber;
    public List<Oxygen> oxygens;
    [SerializeField]
    private Transform oxygenArea;

    private void Awake()
    {
        instance = this;
        oxygens = new List<Oxygen>();
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
                oxygenArea.position + Random.insideUnitSphere * initialOxygenNumber * 0.5f,
                Random.rotation
            );
            oxygens.Add(oxygen);
        }
    }
}
