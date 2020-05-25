using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LobbyCellState
{
    IDLE, SELECTABLE, SELECTED, START_GAME
}

public class LobbyCell : MonoBehaviour
{
    [SerializeField]
    private CellType celltype;
    [SerializeField]
    private GameObject limbPrefab;

    [SerializeField]
    private int maxLimbNum = 10;
    private List<GameObject> limbs;
    public float speed { get; set; }
    private Vector3 endPosition;
    private SmearEffect smearEffect;
    public LobbyCellState cellState { get; set; }

    #region emotion
    [SerializeField]
    private CellEmotion cellEmotion;
    private Renderer rend;
    private int faceID;
    [SerializeField]
    private float timeInterval;
    private float tick = 0f;
    private int frameIndex = 0;
    private Emotions currEmotion = Emotions.Neutral;
    private Texture2D[] currEmotionTextures;
    private float emotionPickInterval;
    private float pickTick = 0f;
    #endregion

    private Transform mainCam;
    private void Awake()
    {
        cellState = LobbyCellState.IDLE;
        mainCam = Camera.main.transform;
        rend = GetComponent<Renderer>();
        faceID = Shader.PropertyToID("_Face");
        emotionPickInterval = Random.Range(5f, 10f);
        speed = Random.Range(0.2f, 0.45f);

        smearEffect = GetComponentInChildren<SmearEffect>();
        if (celltype == CellType.Platelet)
        {
            limbs = new List<GameObject>();
            InitLimbs();
        }
        Reset();
        PickNextEmotionAndReset();
    }

    private void Update()
    {
        if (cellState == LobbyCellState.IDLE)
        {
            transform.position = Vector3.Lerp(transform.position, endPosition, Time.deltaTime * speed);
            Vector3 forward = endPosition - transform.position;
            if (forward != Vector3.zero) transform.forward = Vector3.Lerp(transform.forward, forward, 0.1f);

            if ((endPosition - transform.position).sqrMagnitude < 0.5f)
            {
                Reset();
            }
        }
        else if (cellState == LobbyCellState.SELECTABLE)
        {
            transform.position = Vector3.Lerp(transform.position, LobbyGameController.Instance.characterSelectionBase.position, Time.deltaTime * 2f);
            Vector3 forward = mainCam.position - transform.position;
            if (forward != Vector3.zero) transform.forward = Vector3.Lerp(transform.forward, forward, 0.1f);
        }
        else if (cellState == LobbyCellState.SELECTED)
        {
            transform.position = Vector3.Lerp(transform.position, LobbyGameController.Instance.gameStartPosition.position, Time.deltaTime * 0.75f);
            LobbyGameController.Instance.MoveCameraFollowCell();

            Vector3 forward = LobbyGameController.Instance.gameStartPosition.position - transform.position;
            if (forward != Vector3.zero) transform.forward = Vector3.Lerp(transform.forward, forward, 0.1f);

            if ((LobbyGameController.Instance.gameStartPosition.position - transform.position).sqrMagnitude < 0.25f)
            {
                LobbyGameController.Instance.LoadGame();
                cellState = LobbyCellState.START_GAME;
            }
        }

        if (pickTick > emotionPickInterval)
        {
            PickNextEmotionAndReset();
        }
        pickTick += Time.deltaTime;
        PlayFaceAnim();
    }

    private void Reset()
    {
        endPosition = LobbyGameController.Instance.GetEndPosition();
        // remove weird artifact
        transform.position = smearEffect.PrevPosition = LobbyGameController.Instance.GetStartPosition();
        speed = Random.Range(0.05f, 0.1f);
    }

    private void PlayFaceAnim()
    {
        if (tick > timeInterval)
        {
            rend.material.SetTexture(faceID, currEmotionTextures[frameIndex]);
            frameIndex = (frameIndex + 1) % currEmotionTextures.Length;
            tick = 0;
        }
        tick += Time.deltaTime;
    }

    private void PickNextEmotionAndReset()
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
