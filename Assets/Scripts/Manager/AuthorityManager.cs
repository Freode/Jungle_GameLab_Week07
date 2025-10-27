using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// 파라오의 권위(Authority)를 관리하는 싱글톤 매니저.
/// 권위 게이지는 상한 없이 증가하며, 증가량은 로그 함수처럼 점차 감소합니다.
/// </summary>
public class AuthorityManager : MonoBehaviour
{
    #region Singleton
    public static AuthorityManager instance { get; private set; }
    #endregion

    [Header("Fever Time Settings")]
    [Tooltip("피버 타임의 지속 시간(초)입니다.")]
    public float feverTimeDuration = 10f;
    [Tooltip("피버 타임 동안 적용될 최종 배율입니다.")]
    public float feverTimeMultiplier = 20f; // 예: 10배
    [Tooltip("캐릭터 이동/애니메이션 속도의 최대 배율입니다.")]
    public float maxSpeedMultiplier = 5f;


    private ClickThrottle _clickThrottle;
    private int _originalCriticalPercent;


    [Header("Authority Settings")]
    [Tooltip("현재 권위 게이지. 상한 없이 계속 증가할 수 있습니다.")]
    public float authorityGauge = 0f;

    // ★ 현재 피버 타임 상태인지 알려주는 변수
    private bool isFeverTime = false;
    // ★ 외부에서 현재 피버 타임인지 확인할 수 있는 창구
    public bool IsFeverTime => isFeverTime;

    [Tooltip("권위가 감소하기 시작하는 비활성 시간(초)입니다.")]
    public float decayDelay = 5f;

    // [Original: 5f] 감소 속도를 낮춰서 권위가 천천히 줄어들도록 수정.
    [Tooltip("초당 감소하는 권위의 양입니다.")]
    public float decayRate = 3f;

    [Header("Balancing Settings")]
    [Tooltip("게이지가 0일 때의 기본 권위 증가량입니다.")]
    public float baseIncreaseAmount = 10f;
    [Tooltip("채찍에 직접 맞은 백성 1명당 얻는 권위의 양입니다.")]
    public float directHitAuthorityGain = 10f;
    [Tooltip("채찍 근처에서 놀란 백성 1명당 얻는 권위의 양입니다.")]
    public float nearMissAuthorityGain = 2f;

    // [Original: 0.1f] 스케일링 팩터를 줄여서 권위 증가량의 감소폭을 완만하게 수정.
    [Tooltip("증가량 감소에 영향을 미치는 스케일링 팩터입니다. 값이 클수록 증가량이 더 빠르게 줄어듭니다.")]
    public float scalingFactor = 0.05f;
    
    [Tooltip("게이지 수치에 따라 동적으로 변하는 배율입니다.")]
    public float authorityMultiplier = 1f;

    [Header("Channels to Broadcast")]
    public FloatEventChannelSO onAuthorityChangedChannel;
    [Tooltip("권위 '레벨'(int)과 '색상'(Color)이 바뀔 때만 방송합니다.")]
    public AuthorityLevelChangeEventChannelSO onAuthorityLevelChangedChannel; // 새로 추가된 채널

    public event Action OnUpdateAuthorityInPeriodGold;          // 주기적인 골드 획득량에서 권위 계수를 업데이트

    [Header("Gage Slider")]
    public Slider authorityGaugeSlider;
    public TextMeshProUGUI authorityGaugeText;
    [Tooltip("현재 골드 획득 배율을 표시할 텍스트입니다.")]
    public TextMeshProUGUI authorityMultiplierText;

    [Header("Slider Color Settings")]
    [Tooltip("슬라이더의 채워지는(Fill) 영역의 이미지 컴포넌트입니다.")]
    public Image sliderFillImage;
    [Tooltip("슬라이더의 배경(Background) 영역의 이미지 컴포넌트입니다.")]
    public Image sliderBackgroundImage;
    [Tooltip("0레벨일 때의 기본 배경색입니다.")]
    public Color defaultBackgroundColor = Color.white; // 기본값, 인스펙터에서 수정
    [Tooltip("피버 타임일 때 적용될 불타는 금색입니다.")]
    public Color feverTimeColor = Color.yellow; // 기본값, 인스펙터에서 수정
    [Tooltip("권위 레벨별 채우기 색상 목록입니다. (0레벨부터 순서대로)")]
    public Color[] authorityLevelColors;

    // 마지막으로 권위가 증가한 시간을 추적합니다.
    private float timeSinceLastIncrease = 0f;
    
    // 게이지 최대치 및 초기화 로직을 위한 변수
    private const float MaxAuthorityGauge = 500f;
    private bool _isGaugeFrozen = false;
    private int _previousAuthorityLevel = 1;           // 이전 레벨을 기억 (-1로 초기화하여 시작 시 무조건 방송)
    private float _feverMultiplierAddition = 0f;        // 피버 타임 추가 계수

    private int _sequence = 1;              // 피버 타임 진행 횟수
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        _clickThrottle = GetComponent<ClickThrottle>();
        UpdateAuthorityMultiplier();
        UpdateAuthorityUI();
    }

    private void Update()
    {
        // 게이지가 정지 상태일 때는 감소 로직을 실행하지 않습니다.
        if (_isGaugeFrozen) return;

        // 마지막 증가 후 경과 시간을 계산합니다.
        timeSinceLastIncrease += Time.deltaTime;

        // 설정된 지연 시간보다 오래 증가하지 않았고, 게이지가 0보다 크면 감소를 시작합니다.
        if (timeSinceLastIncrease > decayDelay && authorityGauge > 0)
        {
            authorityGauge -= decayRate * Time.deltaTime;
            authorityGauge = Mathf.Max(authorityGauge, 0f); // 게이지가 0 밑으로 내려가지 않도록 합니다.
            UpdateAuthorityMultiplier();
            UpdateAuthorityUI();
        }
    }

    /// <summary>
    /// 권위 게이지를 증가시킵니다. 증가량은 현재 게이지 값에 따라 동적으로 계산됩니다.
    /// </summary>
    public void IncreaseAuthority()
    {
        // 게이지가 정지 상태일 때는 증가 로직을 실행하지 않습니다.
        if (_isGaugeFrozen) return;

        // 현재 게이지 값에 따라 증가량을 계산합니다. (로그 함수와 유사한 형태)
        // 분모에 +1을 하여 authorityGauge가 0일 때도 정상적으로 작동하도록 합니다.
        float amountToIncrease = baseIncreaseAmount / (authorityGauge * scalingFactor + 1);

        // 권위 게이지를 증가시킵니다.
        authorityGauge += amountToIncrease;

        // 게이지가 상한선을 넘지 않도록 합니다.
        authorityGauge = Mathf.Min(authorityGauge, MaxAuthorityGauge);

        // 마지막 증가 시간을 초기화하여 감소를 막습니다.
        timeSinceLastIncrease = 0f;

        UpdateAuthorityMultiplier();
        UpdateAuthorityUI();

        Debug.Log($"Authority Increased by {amountToIncrease:F2}! Current Gauge: {authorityGauge:F2}");

        // 게이지가 최대치에 도달하면 초기화 코루틴을 시작합니다.
        if (authorityGauge >= MaxAuthorityGauge)
        {
            StartCoroutine(FeverTimeCoroutine());
        }
    }
    /// <summary>
    /// 권위 게이지를 '지정된 양'만큼, '둔화 법칙을 적용하여' 증가시키는 개정된 어명입니다.
    /// </summary>
    /// <param name="baseAmount">계산의 기준이 될 권위의 총량</param>
    public void IncreaseAuthorityByAmount(float baseAmount)
    {
        if (_isGaugeFrozen) return;

        // ★★★ 핵심 개정: 보고받은 양을 '둔화 법칙'에 따라 재계산합니다! ★★★
        // 기존의 baseIncreaseAmount 대신, 보고받은 baseAmount를 사용하여 최종 상승량을 계산합니다.
        float amountToIncrease = baseAmount / (authorityGauge * scalingFactor + 1);

        // (이하 로직은 기존과 동일하옵니다)
        authorityGauge += amountToIncrease;
        
        authorityGauge = Mathf.Min(authorityGauge, MaxAuthorityGauge);
        timeSinceLastIncrease = 0f;
        
        UpdateAuthorityMultiplier();
        UpdateAuthorityUI();

        Debug.Log($"<color=orange>채찍질의 결과로 권위가 {amountToIncrease:F2}만큼 상승! 현재 게이지: {authorityGauge:F2}</color>");

        if (authorityGauge >= MaxAuthorityGauge)
        {
            StartCoroutine(FeverTimeCoroutine());
        }
    }

    /// <summary>
    /// 게이지가 최대치에 도달하면 피버 타임을 시작하고, 시간이 지나면 종료합니다.
    /// </summary>
    private IEnumerator FeverTimeCoroutine()
    {
        // --- 피버 타임 시작 ---
        GameLogger.Instance.Log("Authority", $"{_sequence}번째 : 피버 타임 시작");
        _isGaugeFrozen = true;
        isFeverTime = true;

        if (_clickThrottle != null)
        {
            _originalCriticalPercent = _clickThrottle.criticalPercent;
            _clickThrottle.criticalPercent = 100;
        }

        Debug.Log($"★★★ 피버 타임 시작! {feverTimeDuration}초 동안 지속됩니다. ★★★");
        
        // "상황이 바뀌었으니(피버 시작), 법률에 따라 배율을 재계산하라"고 명합니다.
        UpdateAuthorityMultiplier();
        // ★★★ 추가: 계산된 배율을 즉시 UI에 그리라고 명령합니다.
        UpdateAuthorityUI();

        // 정해진 축제 시간만큼 기다립니다.
        yield return new WaitForSeconds(feverTimeDuration);

        // --- 피버 타임 종료 ---
        GameLogger.Instance.Log("Authority", $"{_sequence}번째 : 피버 타임 종료");
        isFeverTime = false;
        authorityGauge = 0f;
        timeSinceLastIncrease = 0f;
        _isGaugeFrozen = false;

        if (_clickThrottle != null)
        {
            _clickThrottle.criticalPercent = _originalCriticalPercent;
        }

        Debug.Log("피버 타임 종료. 권위가 0으로 초기화되었습니다.");

        // "상황이 바뀌었으니(피버 종료), 법률에 따라 배율을 재계산하라"고 명합니다.
        UpdateAuthorityMultiplier();
        // ★★★ 추가: 초기화된 배율을 즉시 UI에 그리라고 명령합니다.
        UpdateAuthorityUI();
    }

    /// <summary>
    /// 현재 권위 게이지와 피버 타임 상태에 따라 모든 배율을 계산하고 적용합니다.
    /// </summary>
    private void UpdateAuthorityMultiplier()
    {
        float finalGoldMultiplier; // 최종 골드 배율 (제한 없음)

        // 1. "지금이 피버 타임인가?" 를 가장 먼저 확인합니다.
        if (isFeverTime)
        {
            // 피버 타임이 맞다면, 다른 모든 계산을 무시하고
            // 오직 폐하께서 정하신 피버 타임 전용 배율을 최종 배율로 삼습니다.
            finalGoldMultiplier = feverTimeMultiplier + _feverMultiplierAddition;
        }
        // 2. 피버 타임이 아니라면, 비로소 기존의 레벨별 계산법을 따릅니다.
        else
        {
            int level = Mathf.FloorToInt(authorityGauge / 100);
            finalGoldMultiplier = Mathf.Max(1f, level + 1);
        }
        
        // 3. '절제의 칙령'을 집행합니다.
        // 계산된 최종 배율이 얼마이든, '속도'에 적용될 배율만큼은
        // 폐하께서 정하신 한도(maxSpeedMultiplier)를 절대 넘지 못합니다.
        float finalSpeedMultiplier = Mathf.Min(finalGoldMultiplier, maxSpeedMultiplier);

        // 4. 최종적으로 결정된 배율들을 각 시스템에 적용합니다.
        authorityMultiplier = finalGoldMultiplier; // 골드 계산용 배율
        Mover.moveSpeed = Mover.defaultMoveSpeed * finalSpeedMultiplier; // 속도 계산용 배율
        
        // 5. 최종 '골드 배율'을 왕국 전체에 방송합니다.
        if (onAuthorityChangedChannel != null)
        {
            onAuthorityChangedChannel.RaiseEvent(authorityMultiplier);
        }
    }

    /// <summary>
    /// 현재 권위 게이지와 배율에 따라 모든 관련 UI를 업데이트합니다.
    /// </summary>
    private void UpdateAuthorityUI()
    {
        // --- 기존 게이지 슬라이더 및 레벨 텍스트 업데이트 (변경 없음) ---
        if (authorityGaugeSlider != null && authorityGaugeText != null)
        {
            if (isFeverTime || authorityGauge >= MaxAuthorityGauge) // 피버타임일 때도 MAX 표시
            {
                authorityGaugeText.gameObject.SetActive(true);
                authorityGaugeText.text = "MAX";
                authorityGaugeSlider.value = 100f;
            }
            else
            {
                int hundreds = Mathf.FloorToInt(authorityGauge / 100);
                float sliderValue = authorityGauge % 100f;

                if (authorityGauge > 0 && authorityGauge % 100 == 0)
                {
                    sliderValue = 100f;
                    hundreds -= 1;
                }

                if (hundreds <= 0)
                {
                    authorityGaugeText.gameObject.SetActive(false);
                }
                else
                {
                    authorityGaugeText.gameObject.SetActive(true);
                    authorityGaugeText.text = $"x{hundreds + 1}"; // 레벨을 1부터 표시하도록 수정
                }

                authorityGaugeSlider.value = sliderValue;
            }
        }

        // ★★★ 2. 폐하의 명에 따라 '황금 배율' 보고 절차를 수정합니다. ★★★
        if (authorityMultiplierText != null)
        {
            // 배율이 1배를 초과할 때만 텍스트를 표시합니다.
            if (authorityMultiplier > 1f)
            {
                // 소수점 없이 정수로 표시하도록 수정합니다. (예: x100)
                authorityMultiplierText.text = $"x{authorityMultiplier:F0}";;
            }
            else
            {
                // 배율이 1배 이하면 아무것도 표시하지 않습니다 (빈 공간).
                authorityMultiplierText.text = "";
            }
        }

        
        if (sliderFillImage != null && sliderBackgroundImage != null && authorityLevelColors.Length > 0)
        {
            // 피버 타임일 경우, 특별 색상 적용 후 즉시 종료
            if (isFeverTime)
            {
                sliderFillImage.color = feverTimeColor;
                sliderBackgroundImage.color = feverTimeColor; // 배경도 통일
                // ★★★ 피버타임 방송 로직 추가 ★★★
                const int feverLevel = 6; // 피버타임은 6레벨로 정의
                if (_previousAuthorityLevel != feverLevel)
                {
                    onAuthorityLevelChangedChannel?.RaiseEvent(feverLevel, feverTimeColor);
                    _previousAuthorityLevel = feverLevel;
                }
                return; // 기존 return은 그대로 유지
            }

            // 현재 레벨 계산 (0~99.9 => 0레벨, 100~199.9 => 1레벨 ...)
            int currentLevel = Mathf.FloorToInt(authorityGauge / 100);
            
            // 색상 배열의 크기를 넘지 않도록 레벨 값을 제한
            int clampedLevel = Mathf.Min(currentLevel, authorityLevelColors.Length - 1);

            // 채워지는 색상: 현재 레벨의 색상
            Color fillColor = authorityLevelColors[clampedLevel];

            // 배경 색상: 이전 레벨의 색상
            Color backgroundColor;
            if (clampedLevel == 0)
            {
                // 0레벨일 때는 지정된 기본 배경색 사용
                backgroundColor = defaultBackgroundColor;
            }
            else
            {
                // 1레벨 이상일 때는 (현재 레벨 - 1)의 색상을 배경으로 사용
                backgroundColor = authorityLevelColors[clampedLevel - 1];
            }
            
            // 계산된 색상을 실제 이미지에 적용
            sliderFillImage.color = fillColor;
            sliderBackgroundImage.color = backgroundColor;
            if (_previousAuthorityLevel != currentLevel)
            {
                float multiply = authorityMultiplier == 6f ? feverTimeMultiplier + _feverMultiplierAddition : authorityMultiplier;
                GameLogger.Instance?.Log("Authority", $"피버 계수 : x{multiply:F0}");
                onAuthorityLevelChangedChannel?.RaiseEvent(currentLevel, fillColor);
                _previousAuthorityLevel = currentLevel;
            }
        }
    }

    // 피버 타임 추가 계수 적용
    public void IncreaseFeverMultiplier(float amount)
    {
        _feverMultiplierAddition += amount;
        onAuthorityChangedChannel.RaiseEvent(feverTimeMultiplier + _feverMultiplierAddition);
        OnUpdateAuthorityInPeriodGold?.Invoke();
    }

    public float GetFeverMultiplier() { return _feverMultiplierAddition; }
    
    // 피버 타임 전체 배율(기본 + 추가) 반환
    public float GetTotalFeverMultiplier() { return feverTimeMultiplier + _feverMultiplierAddition; }
}
