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
    public Stack<Oxygen> oxygens;
    public List<Oxygen> oxygenList;
    private float density = 0.03f;

    private void Awake()
    {
        instance = this;
        oxygens = new Stack<Oxygen>();
        oxygenList = new List<Oxygen>();
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
                Path.Instance.OxygenZone.transform.position
                + Random.insideUnitSphere * initialOxygenNumber * density,
                Random.rotation
            );
            oxygenList.Add(oxygen);
            oxygens.Push(oxygen);
        }
    }

    public Vector3 GetRandomPositionInOxygenArea()
    {
        return Path.Instance.OxygenZone.transform.position
                         + Random.insideUnitSphere
                         * initialOxygenNumber * density;
    }
}
