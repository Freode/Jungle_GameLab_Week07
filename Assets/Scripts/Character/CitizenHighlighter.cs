// 파일 이름: CitizenHighlighter.cs (전면 개정안)
using Mono.Cecil;
using System.Collections;
using UnityEngine;

public class CitizenHighlighter : MonoBehaviour
{
    [Header("Settings")] public Color mouseOverHighlightColor = new Color(1f, 1f, 0.5f, 1f);
    public Color selectedHighlightColor = Color.yellow;
    [Tooltip("골드 수집 단계별로 표시될 하이라이트 색상 (최대 5단계)")]
    public Color[] stepHighlightColors = new Color[5];

    // ★★★ 핵심 개정: Animator 병사가 아닌, EmotionController 장군을 직접 섬기도록 변경 ★★★
    [Header("Emotion Settings")] [Tooltip("감정 표현을 총괄하는 EmotionController 스크립트입니다.")]
    public EmotionController emotionController;

    [Header("Channels to Subscribe")] public PeopleActorEventChannelSO OnPeopleSelectedChannel;
    public VoidEventChannelSO OnDeselectedChannel;

    [Header("Gold Reward Settings")] [Tooltip("선택되었을 때 떨어뜨릴 황금 주머니 오브젝트입니다.")]
    public GameObject dropObject;

    [Tooltip("게임의 설정을 담는 GameConfig ScriptableObject입니다.")]
    public GameConfig gameConfig;

    [Tooltip("선택 시 상승할 충성심의 양입니다.")] public int loyaltyBoostAmount = 5;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isSelected = false;
    private bool isMouseOver = false;
    private bool isBeingDragged = false;
    private bool isGoldDropOnCooldown = false;
    private PeopleActor selfActor;
    [Tooltip("채찍에 맞았을 때 번쩍일 색상입니다.")] public Color flashColor = Color.red;
    [Tooltip("섬광이 지속될 시간(초)입니다.")] public float flashDuration = 0.2f;
    private Coroutine flashCoroutine; // 섬광 코루틴을 제어하기 위함
    private bool isFlashing = false;
    private static bool s_firstRClickLogged = false;
    private bool isRewardOnCooldownHighlight = false; // 쿨타임 중 하이라이트 유지 플래그

    // 골드 수집 및 쿨다운 상태 변수
    private bool _isRewardCooldownActive = false; // 보상 쿨다운 활성화 여부
    private Coroutine _rewardCooldownCoroutine = null; // 보상 쿨다운 코루틴 참조
    private Coroutine _goldCollectionCoroutine = null; // 골드 수집 코루틴 참조
    private AreaType _currentAreaType; // 현재 일꾼이 속한 AreaType

    // 현재 골드 수집 시도 상태
    private int _currentCollectionStep = 0; // 현재 수집 시도에서 진행된 단계
    private long _goldPerStep = 0; // 현재 수집 시도에서 단계별 골드 양
    private int _totalStepsForCurrentAttempt = 0; // 현재 수집 시도의 총 단계 수
    private float _stepDelay = 0; // 현재 수집 시도의 단계별 지연 시간

    // 현재 쿨다운 주기 동안의 골드 수집 상태
    private long _totalGoldAmountForCurrentCycle = 0; // 현재 쿨다운 주기 동안 얻을 수 있는 총 골드
    private Color _lastStepHighlightColor; // 마지막으로 적용된 단계별 하이라이트 색상
    
    // 보상 1회 지급 여부(현재 쿨다운 주기 동안)
    private bool _hasPaidThisCycle = false;

    // 구역 정보를 담은 컴포넌트
    private Mover _spriteMover;

    void Awake()
    {
        selfActor = GetComponent<PeopleActor>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteMover = GetComponent<Mover>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            _lastStepHighlightColor = originalColor; // 초기화
        }
    }

    private void OnEnable()
    {
        OnPeopleSelectedChannel.OnEventRaised += OnPeopleSelected;
        OnDeselectedChannel.OnEventRaised += OnDeselected;
    }

    private void OnDisable()
    {
        OnPeopleSelectedChannel.OnEventRaised -= OnPeopleSelected;
        OnDeselectedChannel.OnEventRaised -= OnDeselected;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (IsMouseCurrentlyOver())
            {
                isBeingDragged = true;
                isMouseOver = false;
                UpdateHighlight();
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (isBeingDragged)
            {
                isBeingDragged = false;
                isMouseOver = IsMouseCurrentlyOver();
                UpdateHighlight();
            }
        }
    }

    // private void OnMouseEnter()
    // {
    //     if (!isBeingDragged)
    //     {
    //         isMouseOver = true;
    //         UpdateHighlight();
    //
    //         // 1. 감정 표현 (변경 없음)
    //         if (emotionController != null)
    //         {
    //             emotionController.ExpressEmotion("Emotion_Love");
    //         }
    //
    //         // ★★★ 핵심 개정: 황금과 충성심을 하나의 어명으로 묶습니다. ★★★
    //         // '보상 쿨타임'이 아닐 때만 아래를 실행합니다.
    //         if (dropObject != null && !isGoldDropOnCooldown)
    //         {
    //             // 2. 황금 하사
    //             GameManager.instance.DropGoldEasterEgg(dropObject);
    //
    //             // 3. 충성심 고취
    //             if (selfActor != null)
    //             {
    //                 selfActor.ChangeLoyalty(loyaltyBoostAmount);
    //                 Debug.Log($"{selfActor.DisplayName}의 충성도가 {loyaltyBoostAmount}만큼 상승했습니다!");
    //             }
    //
    //             // ★ 추가: '처음 우클릭 보상' 발생 시 1회만 로그
    //             if (!s_firstRClickLogged)
    //             {
    //                 s_firstRClickLogged = true;
    //                 Vector3 p = transform.position;
    //
    //                 // [TimeStamp] [Click] FirstRClickReward/...
    //                 GameLogger.Instance?.Log(
    //                     "Click",
    //                     $"FirstRClickReward/id={(selfActor != null ? selfActor.Id : 0)}/name={(selfActor != null ? selfActor.DisplayName : "NPC")}/" +
    //                     $"loyalty+={loyaltyBoostAmount}/pos=({p.x:F2},{p.y:F2})"
    //                 );
    //             }
    //
    //             // 4. 쿨타임 시작
    //             isGoldDropOnCooldown = true;
    //             StartCoroutine(RewardCooldownCoroutine());
    //         }
    //     }
    // }
    //
    // private void OnMouseExit()
    // {
    //     isMouseOver = false;
    //     UpdateHighlight();
    // }

    public void TriggerReward()
    {
        if (!isBeingDragged)
        {
            // dropObject가 없으면 골드 관련 로직을 수행하지 않음
            if (dropObject == null) return;

            // 1. 감정 표현 (변경 없음)
            if (emotionController != null)
            {
                emotionController.ExpressEmotion("Emotion_Love");
            }

            // ★★★ 핵심 개정: 황금과 충성심을 하나의 어명으로 묶습니다. ★★★
            // 쿨다운이 활성화되어 있지 않다면 새로 시작
            if (!_isRewardCooldownActive)
            {
                // 현재 AreaType 가져오기
                _currentAreaType = AreaType.Normal; // 기본값
                if (selfActor != null)
                {
                    Mover mover = selfActor.GetComponent<Mover>();
                    if (mover != null)
                    {
                        AreaZone currentZone = mover.GetLockedArea();
                        if (currentZone != null)
                        {
                            _currentAreaType = currentZone.GetAreaType();
                        }
                    }
                }

                // 쿨다운 시작
                _isRewardCooldownActive = true;
                _hasPaidThisCycle = false;            // ★ 이번 주기에는 아직 지급 전
                _rewardCooldownCoroutine = StartCoroutine(
                    StartRewardCooldownCoroutine(gameConfig.GetRewardCooldown(_currentAreaType))
                );

                // 골드 수집 상태 초기화
                _currentCollectionStep = 0;

                // 현재 구역 가져오기
                AreaType areaType = _spriteMover.GetCurrentArea().areaType;

                // 총 골드 양 계산 (한 번만 계산)
                _totalGoldAmountForCurrentCycle = GameManager.instance.GetCollectedByAreaType(areaType);

                // 골드 수집 코루틴 시작
                _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());
            }
            // 쿨다운이 활성화되어 있고, 골드 수집 코루틴이 실행 중이 아니라면 새로 시작
            else if (_goldCollectionCoroutine == null)
            {
                _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());
            }

            // 3. 충성심 고취
            if (selfActor != null)
            {
                selfActor.ChangeLoyalty(loyaltyBoostAmount);
                Debug.Log($"{selfActor.DisplayName}의 충성도가 {loyaltyBoostAmount}만큼 상승했습니다!");
            }

            // ★ 추가: '처음 우클릭 보상' 발생 시 1회만 로그
            if (!s_firstRClickLogged)
            {
                s_firstRClickLogged = true;
                Vector3 p = transform.position;

                // [TimeStamp] [Click] FirstRClickReward/...
                GameLogger.Instance?.Log(
                    "Click",
                    $"FirstRClickReward/id={(selfActor != null ? selfActor.Id : 0)}/name={(selfActor != null ? selfActor.DisplayName : "NPC")}/" +
                    $"loyalty+={loyaltyBoostAmount}/pos=({p.x:F2},{p.y:F2})"
                );
            }

            // 쿨타임 중 하이라이트 유지
            isRewardOnCooldownHighlight = true;
            UpdateHighlight(); // 하이라이트 즉시 업데이트
        }
    }

    private IEnumerator CollectGoldInStepsCoroutine()
    {
        _totalStepsForCurrentAttempt = gameConfig.GetGoldCollectionSteps(_currentAreaType);
        _stepDelay = gameConfig.GetGoldCollectionDelay(_currentAreaType);

        // 이미 이번 주기에 지급이 끝났다면 바로 종료
        if (_hasPaidThisCycle)
        {
            _goldCollectionCoroutine = null;
            yield break;
        }

        // 이미 진행된 스텝이 총 스텝 이상이면(재진입 등) 즉시 종료
        if (_currentCollectionStep >= _totalStepsForCurrentAttempt)
        {
            _goldCollectionCoroutine = null;
            yield break;
        }

        while (_currentCollectionStep < _totalStepsForCurrentAttempt)
        {
            if (!isMouseOver)
            {
                _goldCollectionCoroutine = null;
                yield break;
            }

            _currentCollectionStep++;

            // 단계별 하이라이트는 그대로 유지
            if (spriteRenderer != null && stepHighlightColors != null && stepHighlightColors.Length >= 5)
            {
                int currentSteps = _totalStepsForCurrentAttempt;
                Color targetColor = mouseOverHighlightColor;

                if (currentSteps == 1) targetColor = stepHighlightColors[4];
                else if (currentSteps == 2) targetColor = (_currentCollectionStep == 1) ? stepHighlightColors[2] : stepHighlightColors[4];
                else if (currentSteps == 3)
                {
                    if      (_currentCollectionStep == 1) targetColor = stepHighlightColors[0];
                    else if (_currentCollectionStep == 2) targetColor = stepHighlightColors[2];
                    else                                  targetColor = stepHighlightColors[4];
                }
                else if (currentSteps == 4)
                {
                    if      (_currentCollectionStep == 1) targetColor = stepHighlightColors[0];
                    else if (_currentCollectionStep == 2) targetColor = stepHighlightColors[1];
                    else if (_currentCollectionStep == 3) targetColor = stepHighlightColors[3];
                    else                                  targetColor = stepHighlightColors[4];
                }
                else if (currentSteps >= 5)
                {
                    int idx = Mathf.Clamp(_currentCollectionStep - 1, 0, 4);
                    targetColor = stepHighlightColors[idx];
                }

                spriteRenderer.color = targetColor;
                _lastStepHighlightColor = targetColor;
            }

            // ★ 마지막 스텝 이후에는 더 기다리지 않음 → steps==1이면 즉시 지급 효과
            if (_currentCollectionStep < _totalStepsForCurrentAttempt)
            {
                yield return new WaitForSeconds(_stepDelay);
            }
        }

        // ★ 여기서 단 한 번만 지급
        if (!_hasPaidThisCycle)
        {
            AreaType areaType = _spriteMover.GetCurrentArea().areaType;
            _totalGoldAmountForCurrentCycle = GameManager.instance.GetCollectedByAreaType(areaType);
            GameManager.instance.DropGoldEasterEgg(dropObject, selfActor, _totalGoldAmountForCurrentCycle);
            _hasPaidThisCycle = true;  // 이번 주기는 지급 완료
        }

        _goldCollectionCoroutine = null;
    }

    public void SetHovered(bool hovered)
    {
        if (!isBeingDragged)
        {
            isMouseOver = hovered;
            UpdateHighlight();

            // 호버 상태가 변경될 때 골드 수집 코루틴 상태 관리
            if (isMouseOver) // true가 자주 들어올 수 있음(프레임마다)
            {
                if (!_isRewardCooldownActive)
                {
                    TriggerReward();
                }
                else if (_goldCollectionCoroutine == null && !_hasPaidThisCycle) // ★ 이미 지급했으면 재시작 금지
                {
                    _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());
                }
            }
            else
            {
                if (_goldCollectionCoroutine != null)
                {
                    StopCoroutine(_goldCollectionCoroutine);
                    _goldCollectionCoroutine = null;
                }
            }

        }
    }

    private void OnPeopleSelected(PeopleActor selectedActor)
    {
        if (selectedActor.gameObject == this.gameObject)
        {
            isSelected = true;

            /*// 1. 감정 표현 (변경 없음)
            if (emotionController != null)
            {
                emotionController.ExpressEmotion("Emotion_Love");
            }

            // ★★★ 핵심 개정: 황금과 충성심을 하나의 어명으로 묶습니다. ★★★
            // '보상 쿨타임'이 아닐 때만 아래를 실행합니다.
            if (dropObject != null && !isGoldDropOnCooldown)
            {
                // 2. 황금 하사
                GameManager.instance.DropGoldEasterEgg(dropObject);

                // 3. 충성심 고취
                if (selfActor != null)
                {
                    selfActor.ChangeLoyalty(loyaltyBoostAmount);
                    Debug.Log($"{selfActor.DisplayName}의 충성도가 {loyaltyBoostAmount}만큼 상승했습니다!");
                }

                // ★ 추가: '처음 우클릭 보상' 발생 시 1회만 로그
                if (!s_firstRClickLogged)
                {
                    s_firstRClickLogged = true;
                    Vector3 p = transform.position;

                    // [TimeStamp] [Click] FirstRClickReward/...
                    GameLogger.Instance?.Log(
                        "Click",
                        $"FirstRClickReward/id={(selfActor != null ? selfActor.Id : 0)}/name={(selfActor != null ? selfActor.DisplayName : "NPC")}/" +
                        $"loyalty+={loyaltyBoostAmount}/pos=({p.x:F2},{p.y:F2})"
                    );
                }

                // 4. 쿨타임 시작
                isGoldDropOnCooldown = true;
                StartCoroutine(RewardCooldownCoroutine());
            }
            */
        }
        else
        {
            isSelected = false;
        }

        UpdateHighlight();
    }

    // 보상 쿨다운을 시작하고 관리하는 코루틴
    private IEnumerator StartRewardCooldownCoroutine(float cooldownDuration)
    {
        yield return new WaitForSeconds(cooldownDuration);

        _isRewardCooldownActive = false;
        _rewardCooldownCoroutine = null;
        _currentCollectionStep = 0;
        _totalGoldAmountForCurrentCycle = 0;
        _lastStepHighlightColor = originalColor;
        _hasPaidThisCycle = false;           // ★ 다음 주기를 위해 지급 플래그 리셋

        isRewardOnCooldownHighlight = false;
        UpdateHighlight();
    }



    private void OnDeselected()
    {
        isSelected = false;
        UpdateHighlight();
    }

    /// <summary>
    /// 평상시의 안색을 살피는 임무
    /// </summary>
    private void UpdateHighlight()
    {
        // ★★★ 핵심 개정: 긴급 어명(isFlashing)이 발령 중일 때는, 모든 평시 임무를 중단한다! ★★★
        if (isFlashing) return;

        if (spriteRenderer == null) return;
        if (isSelected)
        {
            spriteRenderer.color = selectedHighlightColor;
        }
        else if (isRewardOnCooldownHighlight)
        {
            spriteRenderer.color = _lastStepHighlightColor; // 쿨타임 중에는 마지막 스텝 색상 유지
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    private bool IsMouseCurrentlyOver()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        return (hit.collider != null && hit.collider.gameObject == this.gameObject);
    }

    /// <summary>
    /// 형벌 집행관이 호출할 '붉은 섬광' 어명
    /// </summary>
    public void FlashRed()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    /// <summary>
    /// 붉은 섬광을 잠시 보여주고 원래 색으로 되돌리는 임무
    /// </summary>
    private IEnumerator FlashCoroutine()
    {
        // 1. 긴급 어명 깃발을 올린다!
        isFlashing = true;

        // 2. 몸을 붉게 물들인다.
        spriteRenderer.color = flashColor;

        // 3. 정해진 시간만큼 기다린다.
        yield return new WaitForSeconds(flashDuration);

        // 4. 긴급 어명 깃발을 내린다!
        isFlashing = false;
        flashCoroutine = null;

        // 5. 긴급 어명이 끝났으니, 다시 평상시의 안색으로 돌아가도록 명한다.
        UpdateHighlight();
    }
}