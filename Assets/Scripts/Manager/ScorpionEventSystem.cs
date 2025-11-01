using System.Collections;
using UnityEngine;

/// <summary>
/// 전갈 특별 이벤트를 관리하는 시스템입니다.
/// </summary>
public class ScorpionEventSystem : MonoBehaviour
{
    public static ScorpionEventSystem instance;

    [Header("Settings")]
    [SerializeField] private GameObject scorpionPrefab; // 스폰할 전갈 프리팹
    [SerializeField] private float spawnChance = 0.001f; // 매초 스폰될 확률 (0.1%)
    [SerializeField] private float goldReductionInterval = 1f; // 골드 감소 주기 (초)
    [SerializeField] private float goldReductionMultiplier = 5f; // 초당 획득 골드의 500% 감소
    [SerializeField] private int requiredClicksToDefeat = 20; // 처치에 필요한 클릭 횟수
    [SerializeField] private RectTransform canvasRectTransform; // UI를 표시할 메인 캔버스
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-200, -200); // 스폰 가능 영역 최소 좌표
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(200, 200); // 스폰 가능 영역 최대 좌표

    public bool IsScorpionActive { get; private set; } = false; // 현재 전갈이 활성화되어 있는지 여부

    private GameObject currentScorpionInstance; // 현재 스폰된 전갈 인스턴스
    public int currentClicks { get; private set; } = 0; // 현재 받은 클릭 횟수
    private float spawnTime; // 전갈이 스폰된 시간
    private float firstClickTime; // 전갈이 처음 클릭된 시간
    private bool hasBeenClicked = false; // 전갈이 한 번이라도 클릭되었는지 여부

    public RectTransform CurrentScorpionRectTransform
    {
        get { return currentScorpionInstance != null ? currentScorpionInstance.GetComponent<RectTransform>() : null; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnTimerCoroutine());
    }

    /// <summary>
    /// 주기적으로 전갈 스폰을 시도하는 코루틴입니다.
    /// </summary>
    private IEnumerator SpawnTimerCoroutine()
    {
        while (true)
        { 
            yield return new WaitForSeconds(1f);

            /*if (GameManager.instance.GetElapsedGameTime() < 360f)
            {
                continue;
            }*/

            if (!IsScorpionActive && Random.Range(0f, 1f) < spawnChance)
            {
                SpawnScorpion();
            }
        }
    }

    /// <summary>
    /// 화면의 특정 위치에 전갈을 스폰합니다.
    /// </summary>
    private void SpawnScorpion()
    {
        if (scorpionPrefab == null || canvasRectTransform == null) return;

        IsScorpionActive = true;
        currentClicks = 0;
        spawnTime = Time.time; // 스폰 시간 기록
        firstClickTime = 0; // 초기화
        hasBeenClicked = false; // 초기화

        currentScorpionInstance = Instantiate(scorpionPrefab, canvasRectTransform);

        // 캔버스 내 지정된 영역에서 랜덤 위치를 계산합니다.
        float spawnX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float spawnY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);

        currentScorpionInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(spawnX, spawnY);

        // 전갈 컨트롤러에 이 시스템과 스폰 영역을 연결
        if (currentScorpionInstance.TryGetComponent<ScorpionController>(out var controller))
        {
            controller.Initialize(this, spawnAreaMin, spawnAreaMax);
        }

        GameLogger.Instance.scorpion.LogSpawn();

        // 메시지 표시
        if (MessageDisplayManager.instance != null)
        {
            string message = "황금을 노리는 거대 전갈이 나타났습니다! 서둘러 퇴치하세요!";
            MessageDisplayManager.instance.ShowMessage(message, Color.red, 5f);
        }
        StartCoroutine(GoldReductionCoroutine());
    }

    /// <summary>
    /// 전갈이 활성화된 동안 골드를 감소시키는 코루틴입니다.
    /// </summary>
    private IEnumerator GoldReductionCoroutine()
    {
        while (IsScorpionActive)
        {
            yield return new WaitForSeconds(goldReductionInterval);

            if (IsScorpionActive)
            {
                Debug.Log($"[ScorpionEventSystem] Reducing gold. Multiplier: {goldReductionMultiplier}");
                GameManager.instance.ReduceGoldByScorpion(goldReductionMultiplier);
            }
        }
        float activeTime = Time.time - spawnTime; // 활성화 시간 계산
        GameLogger.Instance.scorpion.LogActiveTime(activeTime);
        Debug.Log("[ScorpionEventSystem] GoldReductionCoroutine ended.");
    }

    /// <summary>
    /// 전갈이 클릭되었을 때 호출됩니다.
    /// </summary>
    public void OnScorpionClicked()
    {
        if (!IsScorpionActive) return;

        if (!hasBeenClicked)
        {
            firstClickTime = Time.time; // 첫 클릭 시간 기록
            hasBeenClicked = true;
        }

        currentClicks++;
        if (currentClicks >= requiredClicksToDefeat)
        {
            DefeatScorpion();
        }
    }

    /// <summary>
    /// 전갈을 처치합니다.
    /// </summary>
    private void DefeatScorpion()
    {
        float timeToDefeat = 0;
        if (hasBeenClicked)
        {
            timeToDefeat = Time.time - firstClickTime; // 처치까지 걸린 시간 계산
            GameLogger.Instance.scorpion.LogDefeatTime(timeToDefeat);
        }
        GameLogger.Instance.scorpion.LogGoldStolen(GameManager.instance.GetStolenGoldAmount()); // 총 빼앗긴 골드 로깅
        GameManager.instance.ReturnStolenGold(1.1f); // 110% 반환 (GameManager에서 빼앗긴 골드 로깅)
        IsScorpionActive = false;
        if (currentScorpionInstance != null)
        {
            Destroy(currentScorpionInstance);
            currentScorpionInstance = null;
        }
        currentClicks = 0;
    }
}
