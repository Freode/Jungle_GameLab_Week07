using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Event Channels")]
    public FloatEventChannelSO OnAuthorityChangedChannel;
    public AuthorityLevelChangeEventChannelSO OnAuthorityLevelChangedChannel;   // 권위 레벨과 색상이 변경됨
    public OutFloatEventChannelSO OnGetAdditionLifeRateChannel;               // 추가 생존 확률을 가져오는 채널

    public GameObject canvasObject;                         // 캔버스 객체

    public event Action OnCurrentGoldAmountChanged;                 // 현재 금 소지량 변경 시, 모두 호출
    public event Action OnClickIncreaseTotalAmountChanged;          // 현재 한 번 클릭할 때, 얻는 금의 양 변경 시, 모두 호출
    public event Action OnPeriodIncreaseAmountChanged;              // 주기적으로 얻는 금의 양이 변화했을 때, 모두 호출
    public event Action<long, Color> OnClickIncreaseGoldAmount;      // 현재 클릭으로 금의 양을 새롭게 얻었다고 호출
    public event Action<TechData, int> OnMaxCapacityUpgrade;        // 다른 테크의 최대 수용량 업그레이드 시, 호출
    public event Action<TechData, int> OnCurrentCapacityChanged;    // 다른 테크의 현재 수용량 변동 시, 호출
    public event Action<TechData, int> OnModifyStructureLevel;      // 테크의 레벨이 변경되어 구조체 외형 변경을 호출
    public event Action<float> OnModifyRespawnUselessPeople;        // 테크 레벨에 따라 백수 생성 주기 조정
    public event Action<AreaType> OnUnlockStructure;                // 해당 구조물이 처음으로 열렸는지, 확인
    public event Func<float> OnGetRespawnTime;                      // 잉여 인력 리스폰 시간 가져오기
    public event Func<float, float> OnGetNextRespawnTime;           // 잉여 인력 다음 리스폰 시간 가져오기
    public event Action<int, Color> OnAuthorityMultiplierUpdate;    // 권위 수치가 변경되었으니, 업데이트하는 이벤트                      // 피라미드 완성되었을 때의 이벤트

    [SerializeField] long currentGoldAmount = 0;                // 현재 소지하고 있는 금의 양
    [SerializeField] long clickIncreaseGoldAmountLinear = 1;    // 클릭 한 번 시, 획득하는 금의 선형적인 양
    [SerializeField] long periodIncreaseGoldAmountLinear = 0;   // 주기적으로 얻는 금의 양이 선형적으로 증가
    [SerializeField] long clickIncreaseGoldAmountRate = 0;      // 클릭 한 번 시, 획득하는 금의 비율 증가 양
    [SerializeField] long periodIncreaseGoldAmountRate = 0;     // 주기적으로 얻는 금의 양이 비율적으로 증가
    // Ending
    [SerializeField] GameObject fadeOutImage;

    private bool isGameOver = false;                        // 게임 종료 여부
    private long clickIncreaseTotalAmount = 0;               // 클릭 한 번 시, 획득하는 양
    private float currentAuthority = 1f;
    private long periodIncreaseTotalAmount = 0;              // 주기적으로 획득하는 총 양

    private Dictionary<AreaType, IncreaseInfo> increaseGoldAmounts;
    private Dictionary<AreaType, bool> checkUnlockStructures;       // 이미 처음으로 열린 구조물 효과인지 확인
    private float additionLifeRate = 0f;                             // 추가 생존 확률

    // 게임 시간 측정 관련
    private System.DateTime gameStartTime;                           // 게임 시작 시간
    private bool hasGameStarted = false;                             // 게임 시작 여부

    private void Awake()
    {
        instance = this;
        Screen.SetResolution(1920, 1080, false);
        increaseGoldAmounts = new Dictionary<AreaType, IncreaseInfo>();
        checkUnlockStructures = new Dictionary<AreaType, bool>();
    }
    private void OnEnable()
    {
        if (OnAuthorityChangedChannel != null)
        {
            OnAuthorityChangedChannel.OnEventRaised += UpdateAuthority;
        }

        if (OnAuthorityLevelChangedChannel != null)
            OnAuthorityLevelChangedChannel.OnEventRaised += UpdateAuthorityValueAndColor;

        if (OnGetAdditionLifeRateChannel != null)
            OnGetAdditionLifeRateChannel.OnEventRaised += GetAdditionalLifeRate;
    }

    private void OnDisable()
    {
        if (OnAuthorityChangedChannel != null)
        {
            OnAuthorityChangedChannel.OnEventRaised -= UpdateAuthority;
        }

        if (OnAuthorityLevelChangedChannel != null)
            OnAuthorityLevelChangedChannel.OnEventRaised -= UpdateAuthorityValueAndColor;

        if (OnGetAdditionLifeRateChannel != null)
            OnGetAdditionLifeRateChannel.OnEventRaised -= GetAdditionalLifeRate;
    }

    // ★ 4. '권위 방송'을 받으면 호출될 함수
    private void UpdateAuthority(float newAuthority)
    {
        // 권위는 1 이상이라는 법칙 적용
        currentAuthority = Mathf.Max(1f, newAuthority);

        // 권위가 바뀌었으니, UI에 표시되는 총 클릭 수입이 변경되었음을 알림
        OnClickIncreaseTotalAmountChanged?.Invoke();
    }


    private void Start()
    {
        // 게임 시작 시간 기록
        StartGameTimer();
        
        RecalculateAllIncomes(); // 시작 시 모든 수입을 한 번 계산
        StartCoroutine(UpdateGoldAmount());
    }

    // 게임 시작 시간을 기록하는 함수
    private void StartGameTimer()
    {
        if (!hasGameStarted)
        {
            gameStartTime = System.DateTime.Now;
            hasGameStarted = true;
            
            // 게임 시작 로그
            if (GameLogger.Instance != null)
            {
                GameLogger.Instance.Log("GameTime", "GameStarted");
            }
        }
    }
    public void RecalculateAllIncomes()
    {
        RecalculatePeriodIncreaseGoldAmount();
        AddClickIncreaseTotalAmount();
    }

    // 주기적으로 값이 금이 추가
    IEnumerator UpdateGoldAmount()
    {
        while (isGameOver == false)
        {
            yield return new WaitForSeconds(1f);
            AddCurrentGoldAmount(periodIncreaseTotalAmount);
            GameLogger.Instance.gold.AcquireAutoGoldAmount(periodIncreaseTotalAmount);
        }
    }

    // 한 번 클릭했을 때, 금의 양을 업데이트하라고 호출
    public void IncreaseGoldAmountWhenClicked(long amount, Color color)
    {
        OnClickIncreaseGoldAmount?.Invoke(amount, color);
        AddCurrentGoldAmount(amount);
    }

    // 특정 테크의 수용량(유사 최대 레벨) 업그레이드
    public void ModifyMaxCapacityEffect(TechData targetTechData, int amount)
    {
        OnMaxCapacityUpgrade?.Invoke(targetTechData, amount);
    }

    // 특정 테크의 현재 수용량 변경
    public void ModifyCurrentCapacity(TechData targetTechData, int amount)
    {
        OnCurrentCapacityChanged?.Invoke(targetTechData, amount);
    }

    // 특정 테크의 레벨이 증가함에 따라 구조물 변경
    public void ModifyStructureLevel(TechData targetType, int amount)
    {
        OnModifyStructureLevel?.Invoke(targetType, amount);
    }

    // 리스폰 주기 변경
    public void ModifyRespawnUselessPeople(float amount)
    {
        OnModifyRespawnUselessPeople?.Invoke(amount);
    }

    // 골드 주머니 드랍
    public void DropGoldEasterEgg(GameObject targetObject)
    {
        targetObject.TryGetComponent(out PeopleDropGold dropGoldComp);

        if (dropGoldComp == null) return;

        targetObject.SetActive(true);
        dropGoldComp.StartGoldDrop();
    }

    // 처음으로 구조물이 열릴 때, 발생할 효과
    public void UnlockStructure(AreaType areaType)
    {
        if (checkUnlockStructures.ContainsKey(areaType))
            return;

        checkUnlockStructures.Add(areaType, true);
        OnUnlockStructure?.Invoke(areaType);
    }

    // ==========================================================
    //                     Modify Member Value
    // ==========================================================

    // 현재 소지하고 있는 금의 양 변화
    public void AddCurrentGoldAmount(long amount)
    {
        currentGoldAmount += amount;
        // 현재 소유하고 있는 금의 양이 변경되었다고 알림
        OnCurrentGoldAmountChanged?.Invoke();
    }

    // '클릭 수입 증가' 명령도 동일하게 처리
    public void AddClickIncreaseGoldAmountLinear(AreaType type, long amount)
    {
        if (increaseGoldAmounts.ContainsKey(type) == false)
            increaseGoldAmounts.Add(type, new IncreaseInfo());
        increaseGoldAmounts[type].clickEachLinear = amount; // += 에서 = 으로 변경
        RecalculateAllIncomes();
    }

    public void AddClickIncreaseGoldAmountRate(AreaType type, long amount)
    {
        if (increaseGoldAmounts.ContainsKey(type) == false)
            increaseGoldAmounts.Add(type, new IncreaseInfo());
        increaseGoldAmounts[type].clickRate += amount;
        RecalculateAllIncomes();
    }

    // 주기적으로 얻는 금의 선형적 수 변화
    public void AddPeriodIncreaseGoldAmountLinear(AreaType type, long amount)
    {
        if (increaseGoldAmounts.ContainsKey(type) == false)
            increaseGoldAmounts.Add(type, new IncreaseInfo());
        increaseGoldAmounts[type].periodEachLinear = amount; // += 에서 = 으로 변경
        RecalculateAllIncomes(); // 모든 수입 재계산
    }

    // 주기적으로 얻는 금의 비율 변화
    public void AddPeriodIncreaseGoldAmountRate(AreaType type, long amount)
    {
        if (increaseGoldAmounts.ContainsKey(type) == false)
            increaseGoldAmounts.Add(type, new IncreaseInfo());
        increaseGoldAmounts[type].periodRate += amount;
        RecalculateAllIncomes();
    }

    public void AddPeriodIncreaseGoldAmount()
    {
        periodIncreaseTotalAmount = 0;

        // 왕국의 모든 지역(AreaType)을 순회합니다.
        foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
        {
            // 1. 이 지역에서 일하는 백성이 몇 명인지 호조(PeopleManager)에게 묻습니다.
            int peopleCount = PeopleManager.Instance.Count(area);

            // 2. 이 지역의 기술 효과(기본 수입)가 얼마인지 자신의 장부에서 찾습니다.
            if (increaseGoldAmounts.TryGetValue(area, out IncreaseInfo info))
            {
                // 3. (백성 수 * 기술 효과) 만큼을 총수입에 더합니다.
                increaseGoldAmounts[area].periodTotalLinear = peopleCount * (info.periodEachLinear + info.periodEachLinearrAddition);
                long areaIncome = peopleCount * (increaseGoldAmounts[area].periodTotalLinear * (100 + info.periodRate) / 100);

                periodIncreaseTotalAmount += areaIncome;
            }
        }
        
        // 최종적으로 계산된 총수입이 변경되었음을 왕국 전체에 알립니다.
        OnPeriodIncreaseAmountChanged?.Invoke();
    }

    // 한 번 클릭할 때, 얻는 금의 양에 대한 총 변화
    public void AddClickIncreaseTotalAmount()
    {
        clickIncreaseTotalAmount = 0;

        // 왕국의 모든 지역(AreaType)을 순회합니다.
        foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
        {
            // 1. 이 지역에서 일하는 백성이 몇 명인지 호조(PeopleManager)에게 묻습니다.
            int peopleCount = PeopleManager.Instance.Count(area);

            // 2. 이 지역의 기술 효과(1인당 생산량)가 얼마인지 자신의 장부에서 찾습니다.
            if (increaseGoldAmounts.TryGetValue(area, out IncreaseInfo info))
            {
                // 3. (백성 수 * 1인당 생산량) 만큼을 총 클릭 수입에 더합니다.
                info.clickTotalLinear = peopleCount * (info.clickEachLinear + info.clickEachLinearAddition);
                long areaIncome = info.clickTotalLinear * (100 + info.clickRate) / 100;
                clickIncreaseTotalAmount += areaIncome;
            }
        }
        
        // 최종적으로 계산된 총 클릭 수입이 변경되었음을 왕국 전체에 알립니다.
        OnClickIncreaseTotalAmountChanged?.Invoke();
    }

    public void SetPeriodIncreaseGoldAmountLinear(AreaType type, long amount)
    {
        if (increaseGoldAmounts.ContainsKey(type) == false)
            increaseGoldAmounts.Add(type, new IncreaseInfo());

        // += (누적) 대신 = (덮어쓰기)를 사용하여, 해당 지역의 '기본 생산량'을 설정합니다.
        increaseGoldAmounts[type].periodEachLinear = amount;
        
        // 값이 바뀌었으니 총 수입을 다시 계산합니다.
        RecalculatePeriodIncreaseGoldAmount();
    }
    // 주기적으로 얻는 총 세금 계산
    public void RecalculatePeriodIncreaseGoldAmount()
    {
        periodIncreaseTotalAmount = 0;

        foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
        {
            int peopleCount = PeopleManager.Instance.Count(area);
            if (increaseGoldAmounts.TryGetValue(area, out IncreaseInfo info))
            {
                info.periodTotalLinear = peopleCount * (info.periodEachLinear + info.periodEachLinearrAddition);
                long areaIncome = info.periodTotalLinear * (100 + info.periodRate) / 100;
                periodIncreaseTotalAmount += areaIncome;
            }
        }
        
        OnPeriodIncreaseAmountChanged?.Invoke();
    }
    // 클릭으로 얻는 총 세금 계산
    private void RecalculateClickIncreaseTotalAmount()
    {
        clickIncreaseTotalAmount = 0;
        foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
        {
            int peopleCount = PeopleManager.Instance.Count(area);
            if (increaseGoldAmounts.TryGetValue(area, out IncreaseInfo info))
            {
                info.clickTotalLinear = peopleCount * (info.clickEachLinear + info.clickEachLinearAddition);
                long areaIncome = info.clickTotalLinear * (100 + info.clickRate) / 100;
                clickIncreaseTotalAmount += areaIncome;
            }
        }
        OnClickIncreaseTotalAmountChanged?.Invoke();
    }

    // 비율 설정 함수도 동일하게 만듭니다.
    public void SetPeriodIncreaseGoldAmountRate(AreaType type, long amount)
    {
        if (increaseGoldAmounts.ContainsKey(type) == false)
            increaseGoldAmounts.Add(type, new IncreaseInfo());

        increaseGoldAmounts[type].periodRate = amount;
        RecalculatePeriodIncreaseGoldAmount();
    }

    // 권위 수치와 색깔이 변경되었을 때, 관련 기능 업데이트
    public void UpdateAuthorityValueAndColor(int authority, Color color)
    {
        OnAuthorityMultiplierUpdate?.Invoke(authority, color);
    }

    // 클릭으로 얻는 + 금의 양 추가 증가
    public void IncreaseClickLinearGoldAcquirementAmount(AreaType areaType, long amount)
    {
        increaseGoldAmounts[areaType].clickEachLinearAddition += amount;
        AddClickIncreaseTotalAmount();
    }

    // 클릭으로 얻는 x 금의 양 추가 증가
    public void IncreaseClickRateGoldAcquirementAmount(AreaType areaType, long amount)
    {
        increaseGoldAmounts[areaType].clickRate += amount;
        AddClickIncreaseTotalAmount();
    }

    // 주기적으로 얻는 + 금의 양 추가 증가
    public void IncreasePeriodLinearGoldAcquirementAmount(AreaType areaType, long amount)
    {
        increaseGoldAmounts[areaType].periodEachLinearrAddition += amount;
        RecalculatePeriodIncreaseGoldAmount();
    }

    // 주기적으로 얻는 x 금의 양 추가 증가
    public void IncreasePeriodRateGoldAcquirementAmount(AreaType areaType, long amount)
    {
        increaseGoldAmounts[areaType].periodRate += amount;
        RecalculatePeriodIncreaseGoldAmount();
    }

    // 추가 생존 확률을 증가
    public void IncreaseAdditionalLifeRate(float amount)
    {
        additionLifeRate += amount;
    }

    // ==========================================================
    //                            Setter
    // ==========================================================


    public void SetIsGameOver(bool isGameOver)
    {
        this.isGameOver = isGameOver;
        // event 추가 예정
        if (this.isGameOver)
        {
            // 게임 클리어 시간 로그 기록
            LogGameClearTime();
            StartCoroutine(CoFadeOut()); // 임시
        }
    }

    // 게임 클리어 시간을 로그로 기록하는 함수
    private void LogGameClearTime()
    {
        if (hasGameStarted && GameLogger.Instance != null)
        {
            System.DateTime gameEndTime = System.DateTime.Now;
            System.TimeSpan playTime = gameEndTime - gameStartTime;
            
            // 시간을 HH:MM:SS 형식으로 포맷
            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", 
                (int)playTime.TotalHours, 
                playTime.Minutes, 
                playTime.Seconds);
            
            // 요청된 양식에 맞춰 로그 기록: [00:00:00] [LogType] Message/Message/Message
            GameLogger.Instance.Log("GameTime", $"GameCleared/PlayTime:{formattedTime}/TotalSeconds:{(int)playTime.TotalSeconds}");
        }
    }


    public IEnumerator CoFadeOut(float duration = 2.0f)
    {
        fadeOutImage.SetActive(true);
        Image fadeOut = fadeOutImage.GetComponent<Image>();
        if (fadeOut == null) yield break;
        

        // 시작 알파(현재값)와 목표 알파(1.0)
        Color c = fadeOut.color;
        float startA = c.a;
        float endA = 1f;

        // 필요 시 Raycast 막기 (UI 클릭 차단)
        fadeOut.raycastTarget = true;
        fadeOut.gameObject.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;                // 일시정지 중에도 동작
            float p = Mathf.Clamp01(t / duration);      // 0→1
            c.a = Mathf.Lerp(startA, endA, p);
            fadeOut.color = c;
            yield return null;
        }

        // 마무리 보정
        c.a = endA;
        fadeOut.color = c;
        SceneManager.LoadScene("EndingScene");
    }



    // ==========================================================
    //                            Getter
    // ==========================================================

    public IncreaseInfo GetIncreaseGoldInfo(AreaType areaType) 
    {
        if (increaseGoldAmounts.ContainsKey(areaType) == false)
            increaseGoldAmounts.Add(areaType, new IncreaseInfo());

        return increaseGoldAmounts[areaType]; 
    }

    public float GetRespawnTime() { return OnGetRespawnTime.Invoke(); }

    public float GetNextRespwanTime(float amount) { return OnGetNextRespawnTime.Invoke(amount); }

    public long GetCurrentGoldAmount() { return currentGoldAmount; }

    public long GetClickIncreaseGoldAmountLinear() { return clickIncreaseGoldAmountLinear; }

    public long GetPeriodIncreaseGoldAmountLinear() { return periodIncreaseGoldAmountLinear; }

    public long GetClickIncreaseGoldAmountRate() { return clickIncreaseGoldAmountRate; }

    public long GetPeriodIncreaseGoldAmountRate() { return periodIncreaseGoldAmountRate; }

    /// <summary>
    /// UI에 표시할 '기본' 클릭당 골드 획득량을 반환합니다. (권위 배율 미적용)
    /// </summary>
    public long GetBaseClickIncreaseTotalAmount()
    {
        return clickIncreaseTotalAmount;
    }

    public long GetClickIncreaseTotalAmount()
    {
        return clickIncreaseTotalAmount;
    }

    public long GetCurrentAuthority()
    {
        return (long)currentAuthority;
    }

    public long GetPeriodIncreaseTotalAmount() { return periodIncreaseTotalAmount; }

    public bool GetIsGameOver() { return isGameOver; }

    public Dictionary<AreaType, bool> GetCheckUnlockStructures() { return checkUnlockStructures; }

    public float GetAdditionalLifeRate()
    {
        return additionLifeRate;
    }
}
