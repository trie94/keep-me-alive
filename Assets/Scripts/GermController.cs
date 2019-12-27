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
    private int maxWormNumber = 10;
    private int currWormNumber;
    private Stack<Worm> wormPool;
    public List<Worm> worms;
    [SerializeField]
    private float wormInstantiateInterval = 10f;
    private float intervalTick = 0f;
    #endregion

    private void Awake()
    {
        instance = this;
        wormPool = new Stack<Worm>();
        worms = new List<Worm>();
    }

    private void Start()
    {
        CreateWormPool();
    }

    private void Update()
    {
        SpawnGermsConstantly();
    }

    private void CreateWormPool()
    {
        for (int i = 0; i < maxWormNumber; i++)
        {
            int segIndex = Random.Range(0, Path.Instance.segments.Count);
            Segment currSeg = Path.Instance.segments[segIndex];
            float progress = Random.Range(0f, 1f);

            Germ germ = Instantiate(
                germPrefabs[0],
                Path.Instance.GetPoint(currSeg, progress),
                Random.rotation
            );
            germ.gameObject.SetActive(false);
            wormPool.Push((Worm)germ);
        }
    }

    private void SpawnGermsConstantly()
    {
        if (wormPool.Count <= 0) return;
        if (intervalTick > wormInstantiateInterval)
        {
            Worm worm = wormPool.Pop();
            worm.gameObject.SetActive(true);
            worms.Add(worm);
            currWormNumber++;

            intervalTick = 0f;
        }
        intervalTick += Time.deltaTime;
    }
}
