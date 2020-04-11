using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum CellType
{
    Erythrocyte, Leukocyte, Platelet
}

[System.Serializable]
public abstract class Cell : MonoBehaviour
{
    #region Emotion
    [SerializeField]
    private CellEmotion cellEmotion;
    protected Renderer rend;
    protected int faceID;
    [SerializeField]
    private float timeInterval;
    private float tick = 0f;
    private int frameIndex = 0;
    private Emotions currEmotion = Emotions.Neutral;
    private Texture2D[] currEmotionTextures;
    protected float emotionPickInterval;
    protected float pickTick = 0f;
    #endregion

    #region Movement
    [SerializeField]
    protected CellMovement[] behaviors;
    protected Vector3 currVelocity;
    public float progress = 0f;
    public float speed;
    public Segment currSeg;

    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 5f)]
    public float avoidanceRadius = 0.5f;
    [Range(1f, 5f)]
    public float germDetectionRadius = 2f;
    [Range(0.1f, 10f)]
    public float velocityMultiplier = 10f;
    [Range(0.1f, 10f)]
    public float maxSpeed = 5f;
    private float squareMaxSpeed;
    public float squareAvoidanceRadius;
    private float squareNeighborRadius;
    private float squareGermDetectionRadius;
    #endregion

    protected CreatureTypes type = CreatureTypes.Cell;
    protected CellType cellType;
    protected Dictionary<CreatureTypes, List<Transform>> creatureGroups;

    public virtual void Awake()
    {
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        emotionPickInterval = Random.Range(5f, 10f);
        speed = Random.Range(0.1f, 0.3f);

        // init neighbor groups
        creatureGroups = new Dictionary<CreatureTypes, List<Transform>>();
        creatureGroups.Add(CreatureTypes.Cell, null);
        creatureGroups.Add(CreatureTypes.Oxygen, null);
        creatureGroups.Add(CreatureTypes.Germ, null);

        squareMaxSpeed = maxSpeed * maxSpeed;
        squareAvoidanceRadius = avoidanceRadius * avoidanceRadius;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareGermDetectionRadius = germDetectionRadius * germDetectionRadius;
    }

    public virtual void Start()
    {
        // set initial position
        int segIndex = Random.Range(0, Path.Instance.segments.Count);
        currSeg = Path.Instance.segments[segIndex];
        progress = Random.Range(0f, 1f);
        transform.position = Path.Instance.GetPoint(currSeg, progress);
        if (currSeg.Direction != Vector3.zero)
        {
            transform.forward = currSeg.Direction;
        }
        PickNextEmotionAndReset();
    }

    public virtual void Update()
    {
        List<Transform> neighbors = GetCellNeighbors();
        creatureGroups[type] = neighbors;
        // this should be fixed when other cells also get their states.
        Vector3 velocity = behaviors[0].CalculateVelocity(this, creatureGroups);
        velocity *= velocityMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        Move(velocity);

        if (pickTick > emotionPickInterval)
        {
            PickNextEmotionAndReset();
        }
        pickTick += Time.deltaTime;
        PlayFaceAnim();
    }

    public virtual void Move(Vector3 velocity)
    {
        currVelocity = velocity;
        transform.position += currVelocity * Time.deltaTime * speed;
        if (currVelocity != Vector3.zero) transform.forward = currVelocity;
    }

    protected void PlayFaceAnim()
    {
        if (tick > timeInterval)
        {
            rend.material.SetTexture(faceID, currEmotionTextures[frameIndex]);
            frameIndex = (frameIndex + 1) % currEmotionTextures.Length;
            tick = 0;
        }
        tick += Time.deltaTime;
    }

    protected void PickNextEmotionAndReset()
    {
        var emotions = System.Enum.GetValues(typeof(Emotions));
        var nextEmotion = (Emotions)emotions.GetValue(Random.Range(0, emotions.Length));

        while (currEmotion == nextEmotion)
        {
            nextEmotion = (Emotions)emotions.GetValue(Random.Range(0, emotions.Length));
        }

        frameIndex = 0;
        pickTick = 0f;
        emotionPickInterval = Random.Range(5f, 10f);
        currEmotion = nextEmotion;
        currEmotionTextures = cellEmotion.MapEnumWithTexture(currEmotion);
    }

    protected List<Transform> GetCellNeighbors()
    {
        List<Transform> neighbors = new List<Transform>();
        var cells = CellController.Instance.entireCells;

        for (int i = 0; i < cells.Count; i++)
        {
            var currCell = cells[i];
            if (currCell == this) continue;

            if (Vector3.SqrMagnitude(currCell.transform.position - transform.position) <= squareNeighborRadius)
            {
                neighbors.Add(currCell.transform);
            }
        }

        return neighbors;
    }

    // for now it's just worms
    protected List<Transform> GetGermNeighbors()
    {
        List<Transform> neighbors = new List<Transform>();
        var worms = GermController.Instance.worms;
        for (int i = 0; i < worms.Count; i++)
        {
            var currWorm = worms[i];
            if (Vector3.SqrMagnitude(currWorm.transform.position - transform.position) <= squareGermDetectionRadius)
            {
                neighbors.Add(currWorm.transform);
            }
        }
        return neighbors;
    }

    public virtual void UpdateCellState() { }

    public Segment GetNextSegment()
    {
        var nextSeg = currSeg.end.nextSegments;
        if (nextSeg.Count > 1)
        {
            float weightSum = 0f;
            for (int i = 0; i < nextSeg.Count; i++)
            {
                weightSum += nextSeg[i].weight;
            }

            float rnd = Random.Range(0, weightSum);
            for (int i = 0; i < nextSeg.Count; i++)
            {
                if (rnd < nextSeg[i].weight)
                    return nextSeg[i];
                rnd -= nextSeg[i].weight;
            }
        }
        else
        {
            return nextSeg[0];
        }

        return null;
    }

    //private void OnDrawGizmos()
    //{
    //    if (!Application.isPlaying) return;
    //    List<Transform> neighbors = GetNeighbors();
    //    behaviors[0].DrawGizmos(this, neighbors);
    //}
}
