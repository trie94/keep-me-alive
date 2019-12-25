using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GermController : MonoBehaviour
{
    private static GermController instance;
    public static GermController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GermController>();
            }
            return instance;
        }
    }

    [SerializeField]
    private Germ[] germPrefabs;

    #region worm
    [SerializeField]
    private int wormNumber;
    private int currWormNumber;
    public List<Worm> worms;
    #endregion

    private void Awake()
    {
        instance = this;
        worms = new List<Worm>();
    }

    private void Start()
    {
        SpawnGerms();
    }

    private void SpawnGerms()
    {
        for (int i = 0; i < wormNumber; i++)
        {
            Germ germ = Instantiate(germPrefabs[0]);
            worms.Add((Worm)germ);
        }
    }
}
