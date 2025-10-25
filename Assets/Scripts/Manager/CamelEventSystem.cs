
using System.Collections;
using UnityEngine;

/// <summary>
/// 낙타 특별 이벤트를 관리하는 시스템입니다.
/// </summary>
public class CamelEventSystem : MonoBehaviour
{
    public static CamelEventSystem instance;

    [Header("설정")]
    [SerializeField] private GameObject camelPrefab; // 스폰할 낙타 프리팹
    [SerializeField] private float spawnChance = 0.004f; // 매초 스폰될 확률 (0.4%)
    [SerializeField] private float bonusDuration = 15f; // 보너스 지속 시간
    [SerializeField] private int bonusMultiplier = 20; // 보너스 배율 (클릭 골드 * 20)
    [SerializeField] private RectTransform canvasRectTransform; // UI를 표시할 메인 캔버스

    private bool isBonusActive = false; // 현재 보너스가 활성화되어 있는지 여부
    private GameObject currentCamelInstance; // 현재 스폰된 낙타 인스턴스
    private int clicksDuringBonus = 0; // 보너스 지속 시간 동안 클릭 횟수
    private long goldGainedDuringBonus = 0; // 보너스 지속 시간 동안 얻은 금

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
    private void Update()
    {
        // 보너스가 활성화되어 있지 않을 때 'c' 키를 누르면 낙타를 수동으로 소환합니다.
        if (!isBonusActive && Input.GetKeyDown(KeyCode.C))
        {
            SpawnCamel();
        }
    }

    /// <summary>
    /// 주기적으로 낙타 스폰을 시도하는 코루틴입니다.
    /// </summary>
    private IEnumerator SpawnTimerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // 보너스가 활성화되어 있지 않고, 확률에 당첨되면 낙타 스폰
            if (!isBonusActive && Random.Range(0f, 1f) < spawnChance)
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
        if (camelPrefab == null || canvasRectTransform == null) return;

        // 캔버스 내에 프리팹을 생성합니다.
        currentCamelInstance = Instantiate(camelPrefab, canvasRectTransform);

        // 캔버스의 크기를 기준으로 랜덤 위치를 계산합니다. (중앙 앵커 기준)
        float padding = 100f; // 화면 가장자리로부터의 최소 여백
        float spawnX = Random.Range(-canvasRectTransform.rect.width / 2 + padding, canvasRectTransform.rect.width / 2 - padding);
        float spawnY = Random.Range(-canvasRectTransform.rect.height / 2 + padding, canvasRectTransform.rect.height / 2 - padding);

        // UI 요소의 위치는 anchoredPosition을 사용합니다.
        currentCamelInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(spawnX, spawnY);

        GameLogger.Instance.camelStats.LogSpawn();
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

        yield return new WaitForSeconds(bonusDuration);

        // 보너스 종료
        isBonusActive = false;
        GameLogger.Instance.camelBonus.LogBonusResult(clicksDuringBonus, goldGainedDuringBonus);
        GameLogger.Instance.camelStats.LogDisappeared();

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
}
