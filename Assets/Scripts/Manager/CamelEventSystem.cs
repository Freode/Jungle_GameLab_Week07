using System.Collections;
using UnityEngine;

/// <summary>
/// 낙타 특별 이벤트를 관리하는 시스템입니다.
/// </summary>
public class CamelEventSystem : MonoBehaviour
{
    public static CamelEventSystem instance;

    [Header("Settings")]
    [SerializeField] private GameObject camelPrefab;                    // 스폰할 낙타 프리팹
    [SerializeField] private float spawnChance = 0.004f;                // 매초 스폰될 확률 (0.4%)
    [SerializeField] private float bonusDuration = 15f;                 // 보너스 지속 시간
    [SerializeField] private int bonusMultiplier = 20;                  // 보너스 배율 (클릭 골드 * 20)
    [SerializeField] private RectTransform canvasRectTransform;         // UI를 표시할 메인 캔버스
    [SerializeField] private RectTransform camelSpawnAreaRectTransform; // 낙타가 스폰될 영역을 정의하는 RectTransform
    [SerializeField] private float operateTimer = 360f;                 // 아이템이 출현할 시간
    [SerializeField] private BonusEffectController bonusEffectController; // 보너스 시각 효과 컨트롤러

    private bool isBonusActive = false;             // 현재 보너스가 활성화되어 있는지 여부
    private GameObject currentCamelInstance;        // 현재 스폰된 낙타 인스턴스
    private int clicksDuringBonus = 0;              // 보너스 지속 시간 동안 클릭 횟수
    private long goldGainedDuringBonus = 0;         // 보너스 지속 시간 동안 얻은 금
    private float _bonusSpawnChance = 0f;           // 상인 보너스 스폰 확률

    public bool IsBonusActive => isBonusActive;
    public int BonusMultiplier => bonusMultiplier;
    private Camera mainCamera; // 메인 카메라 참조

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(SpawnTimerCoroutine());

        // GameManager의 골드 획득 이벤트에 구독
        GameManager.instance.OnClickIncreaseGoldAmount += HandleGoldIncrease;
    }

    private void OnDestroy()
    {
        // GameManager의 골드 획득 이벤트 구독 해지
        if (GameManager.instance != null)
        {
            GameManager.instance.OnClickIncreaseGoldAmount -= HandleGoldIncrease;
        }
    }

    /// <summary>
    /// GameManager에서 골드 획득 이벤트 발생 시 호출됩니다.
    /// </summary>
    private void HandleGoldIncrease(long amount, Color color)
    {
        if (isBonusActive)
        {
            clicksDuringBonus++;
            goldGainedDuringBonus += amount;
        }
    }

    /// <summary>
    /// 디버그용: 'c' 키를 누르면 낙타를 소환합니다.
    /// </summary>
    // private void Update()
    // {
    //    // 보너스가 활성화되어 있지 않을 때 'c' 키를 누르면 낙타를 수동으로 소환합니다.
    //    if (!isBonusActive && Input.GetKeyDown(KeyCode.C))
    //    {
    //        SpawnCamel();
    //    }
    // }

    /// <summary>
    /// 주기적으로 낙타 스폰을 시도하는 코루틴입니다.
    /// </summary>
    private IEnumerator SpawnTimerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (GameManager.instance.GetElapsedGameTime() < operateTimer)
            {
                continue;
            }

            // 보너스가 활성화되어 있지 않고, 확률에 당첨되면 낙타 스폰
            if (!isBonusActive && Random.Range(0f, 1f) < spawnChance + _bonusSpawnChance)
            {
                SpawnCamel();
            }
        }
    }

    /// <summary>
    /// 화면의 랜덤한 위치에 낙타를 스폰합니다.
    /// </summary>
    private void SpawnCamel()
    {
        if (camelPrefab == null || canvasRectTransform == null || camelSpawnAreaRectTransform == null) return;

        // 지정된 스폰 영역 RectTransform 내에서 랜덤 위치를 계산합니다.
        // RectTransformUtility.ScreenPointToLocalPointInRectangle을 사용하여 월드 좌표를 캔버스 로컬 좌표로 변환
        Vector2 randomWorldPoint = GetRandomPointInRect(camelSpawnAreaRectTransform);
        Debug.Log($"[CamelEventSystem] Random World Point in Spawn Area: {randomWorldPoint}");

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, randomWorldPoint, null, out localPoint))
        {
            // 캔버스 내에 프리팹을 생성합니다.
            currentCamelInstance = Instantiate(camelPrefab, canvasRectTransform);
            currentCamelInstance.GetComponent<RectTransform>().anchoredPosition = localPoint;
            Debug.Log($"[CamelEventSystem] Camel spawned at local position: {localPoint}");
        }
        else
        {
            // 변환 실패 시, 기본값 또는 경고
            currentCamelInstance = Instantiate(camelPrefab, canvasRectTransform);
            currentCamelInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Fallback to center
            Debug.LogWarning("[CamelEventSystem] Failed to convert screen point to local point for camel spawn. Spawning at center.");
        }

        GameLogger.Instance.camelStats.LogSpawn();

        // 메시지 표시
        if (MessageDisplayManager.instance != null)
        {
            string message = $"최상급 맥주를 클릭하면 {bonusDuration:F0}초간 금 징수량이 {bonusMultiplier * 100}% 증가합니다!";
            MessageDisplayManager.instance.ShowMessageUntilDestroyed(message, Color.green, currentCamelInstance);
        }
    }

    /// <summary>
    /// RectTransform 내에서 랜덤한 월드 좌표를 반환합니다.
    /// </summary>
    private Vector2 GetRandomPointInRect(RectTransform rectTransform)
    {
        Rect rect = rectTransform.rect;
        float randomX = Random.Range(rect.xMin, rect.xMax);
        float randomY = Random.Range(rect.yMin, rect.yMax);

        // RectTransform의 로컬 좌표를 월드 좌표로 변환
        Vector2 localPointInRect = new Vector2(randomX, randomY);
        Vector2 worldPoint = rectTransform.TransformPoint(localPointInRect);
        Debug.Log($"[CamelEventSystem] GetRandomPointInRect - Rect: {rect}, Local Point: {localPointInRect}, World Point: {worldPoint}");
        return worldPoint;
    }

    /// <summary>
    /// 낙타 보너스를 활성화합니다. (CamelController에서 호출)
    /// </summary>
    public void ActivateCamelBonus()
    {
        if (isBonusActive) return;

        clicksDuringBonus = 0;
        goldGainedDuringBonus = 0;
        
        StartCoroutine(BonusCoroutine());
    }

    /// <summary>
    /// 지정된 시간 동안 보너스를 적용하는 코루틴입니다.
    /// </summary>
    private IEnumerator BonusCoroutine()
    {
        isBonusActive = true;
        bonusEffectController?.StartFadeIn();

        yield return new WaitForSeconds(bonusDuration);

        // 보너스 종료 - 통합 로그 기록 (배수 포함)
        isBonusActive = false;
        bonusEffectController?.StartFadeOut();
        GameLogger.Instance.camelStats.LogDefeated(clicksDuringBonus, goldGainedDuringBonus, bonusMultiplier);

        if (currentCamelInstance != null)
        {
            Destroy(currentCamelInstance);
            currentCamelInstance = null;
        }
    }

    /// <summary>
    /// 낙타 등장 확률을 설정합니다. (TechEffect에서 사용)
    /// </summary>
    public void SetSpawnChance(float newChance)
    {
        spawnChance = Mathf.Clamp(newChance, 0.0001f, 1f);
        Debug.Log($"[CamelEventSystem] 등장 확률이 {spawnChance * 100f:F2}%로 설정되었습니다.");
    }

    /// <summary>
    /// 낙타 등장 확률을 증가시킵니다. (TechEffect에서 사용)
    /// </summary>
    public void AddSpawnChance(float amount)
    {
        spawnChance = Mathf.Clamp(spawnChance + amount, 0.0001f, 1f);
        Debug.Log($"[CamelEventSystem] 등장 확률이 {amount * 100f:F2}% 증가하여 현재 {spawnChance * 100f:F2}%입니다.");
    }

    /// <summary>
    /// 현재 등장 확률을 반환합니다.
    /// </summary>
    public float GetSpawnChance()
    {
        return spawnChance;
    }

    /// <summary>
    /// 보너스 배수를 선형적으로 증가시킵니다. (TechEffect에서 사용)
    /// </summary>
    public void AddBonusMultiplier(int amount)
    {
        bonusMultiplier += amount;
        Debug.Log($"[CamelEventSystem] 보너스 배수가 {amount} 증가하여 현재 {bonusMultiplier}배입니다.");
    }

    /// <summary>
    /// 보너스 배수를 비율로 증가시킵니다. (TechEffect에서 사용)
    /// </summary>
    public void AddBonusMultiplierRate(int rate)
    {
        int addAmount = bonusMultiplier * rate / 100;
        bonusMultiplier += addAmount;
        Debug.Log($"[CamelEventSystem] 보너스 배수가 {rate}% 증가하여 현재 {bonusMultiplier}배입니다.");
    }

    /// <summary>
    /// 보너스 지속시간을 선형적으로 증가시킵니다. (TechEffect에서 사용)
    /// </summary>
    public void AddBonusDuration(float amount)
    {
        bonusDuration += amount;
        Debug.Log($"[CamelEventSystem] 보너스 지속시간이 {amount}초 증가하여 현재 {bonusDuration}초입니다.");
    }

    /// <summary>
    /// 보너스 지속시간을 비율로 증가시킵니다. (TechEffect에서 사용)
    /// </summary>
    public void AddBonusDurationRate(int rate)
    {
        float addAmount = bonusDuration * rate / 100f;
        bonusDuration += addAmount;
        Debug.Log($"[CamelEventSystem] 보너스 지속시간이 {rate}% 증가하여 현재 {bonusDuration:F1}초입니다.");
    }

    /// <summary>
    /// 현재 보너스 배수를 반환합니다.
    /// </summary>
    public int GetBonusMultiplier()
    {
        return bonusMultiplier;
    }

    /// <summary>
    /// 현재 보너스 지속시간을 반환합니다.
    /// </summary>
    public float GetBonusDuration()
    {
        return bonusDuration;
    }

    // 스폰 확률 더하기
    public void AddSpawnPerc(float amount)
    {
        _bonusSpawnChance += amount;
    }


    // 스폰 확률 반환
    public float GetSpawnPerc(float amount)
    {
        return (spawnChance + _bonusSpawnChance + amount) * 100f;
    }
}