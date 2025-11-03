// 파일 이름: CitizenHighlighter.cs (전면 개정)
using UnityEngine.Serialization;
using System.Collections;
using UnityEngine;

public class CitizenHighlighter : MonoBehaviour
{
    [Header("Settings")] 
    public Color selectedHighlightColor = Color.yellow;
    [Tooltip("골드 수집 단계별로 표시할 하이라이트 색상 (최대 5단계)")]
    [Header("Dynamic Step Highlight (Gradient)")]
    [SerializeField] private bool useGradientStepHighlight = true;
    [SerializeField] private Color startHighlightColor = new Color(1f, 1f, 0.5f, 1f);
    [SerializeField] private Color endHighlightColor = Color.yellow;
    
    [SerializeField] private float hoverResumeWindow = 0.6f; // 호버 끊겨도 이 시간 안에 복귀하면 진행 유지
    
    // ★★★ 핵심 개정: Animator 병사가 아닌, EmotionController 장군을 직접 섬기도록 변경 ★★★
    [Header("Emotion Settings")] [Tooltip("감정 표현을 총괄하는 EmotionController 스크립트입니다.")]
    public EmotionController emotionController;

    [Header("Channels to Subscribe")] public PeopleActorEventChannelSO OnPeopleSelectedChannel;
    public VoidEventChannelSO OnDeselectedChannel;

    [Header("Gold Reward Settings")] [Tooltip("선택하였을 때 떨어뜨릴 황금 주머니 오브젝트입니다.")]
    public GameObject dropObject;

    [Tooltip("게임의 설정을 담는 GameConfig ScriptableObject입니다.")]
    public GameConfig gameConfig;
    [Header("Whip Effect Settings")]
    [FormerlySerializedAs("whipEffectOffset")]
    [Tooltip("World-space offset for whip swing visual effect")]
    [SerializeField] private Vector3 whipSwingEffectOffset = new Vector3(0f, 1.25f, 0f);
    [Tooltip("World-space offset for whip impact visual effect")]
    [SerializeField] private Vector3 whipImpactEffectOffset = new Vector3(0f, 1f, 0f);

    [Tooltip("선택 시 상승할 충성도의 양입니다.")] public int loyaltyBoostAmount = 5;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isSelected = false;
    private bool isMouseOver = false;
    private bool isBeingDragged = false;
    private bool isGoldDropOnCooldown = false;
    private PeopleActor selfActor;
    [Tooltip("채찍에 맞았을 때 번쩍이는 색상입니다.")] public Color flashColor = Color.red;
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

    // 구역 정보를 가진 컴포넌트
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
        // if (Input.GetMouseButtonDown(1))
        // {
        //     if (IsMouseCurrentlyOver())
        //     {
        //         isBeingDragged = true;
        //         isMouseOver = false;
        //         UpdateHighlight();
        //     }
        // }

        // if (Input.GetMouseButtonUp(1))
        // {
        //     if (isBeingDragged)
        //     {
        //         isBeingDragged = false;
        //         isMouseOver = IsMouseCurrentlyOver();
        //         UpdateHighlight();
        //     }
        // }
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
    //             // 3. 충성도 고취
    //             if (selfActor != null)
    //             {
    //                 selfActor.ChangeLoyalty(loyaltyBoostAmount);
    //                 Debug.Log($"{selfActor.DisplayName}의 충성도가 {loyaltyBoostAmount}만큼 상승했습니다!");
    //             }
    //
    //             // [추가]: '처음 우클릭 보상' 발생 시 1회만 로그
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
        if (isBeingDragged) return;  
        if (gameConfig == null || GameManager.instance == null) return;
        if (dropObject == null) return; // 안전망

        // 수집 코루틴이 돌고 있으면 재진입 금지
        if (_goldCollectionCoroutine != null) 
        {
            // dropObject가 없으면 골드 관련 로직을 수행하지 않음
            if (dropObject == null) return;

            // 1. 감정 표현 (이제 마지막 골드 드랍 시점에 처리)
            // if (emotionController != null)
            // {
            //     emotionController.ExpressEmotion("Emotion_Love");
            // }

            // [주석] 핵심 개정: 황금과 충성도를 하나의 명세로 묶습니다. [주석]
            // 쿨다운이 활성화되어 있지 않다면 새로 시작
            if (!_isRewardCooldownActive)
            {
                // 현재 AreaType 가져오기
                _currentAreaType = AreaType.Normal; // 기본값
                if (selfActor != null)
                {
                    Mover currentMover = selfActor.GetComponent<Mover>();
                    if (currentMover != null)
                    {
                        AreaZone currentZone = currentMover.GetLockedArea();
                        if (currentZone != null)
                        {
                            _currentAreaType = currentZone.GetAreaType();
                        }
                    }
                }

                // 쿨다운 시작
                _isRewardCooldownActive = true;
                _hasPaidThisCycle = false;            // 아직 이번 주기에는 지급 안 함
                _rewardCooldownCoroutine = StartCoroutine(
                    StartRewardCooldownCoroutine(gameConfig.GetRewardCooldown(_currentAreaType))
                );

                // 골드 수집 상태 초기화
                _currentCollectionStep = 0;

                // 현재 구역 가져오기
                AreaType areaType = _spriteMover.GetCurrentArea().areaType;

                // 총 골드 양 계산 (한 번만 계산) - 가중치 반영
                long firstCollectedBase = GameManager.instance.GetCollectedByAreaType(areaType);
                int firstActorWeight = PeopleManager.Instance.GetActorWeight(selfActor);
                _totalGoldAmountForCurrentCycle = firstCollectedBase * firstActorWeight;

                // 골드 수집 코루틴 시작
                _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());
            }
                        // 쿨다운이 활성화되어 있고, 골드 수집 코루틴이 실행 중이 아니면 새로 시작
                        else if (_goldCollectionCoroutine == null)
                        {
                            _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());
                        }
            // 3. 충성도 고취
            if (selfActor != null)
            {
                selfActor.ChangeLoyalty(loyaltyBoostAmount);
                Debug.Log($"{selfActor.DisplayName}의 충성도가 {loyaltyBoostAmount}만큼 상승했습니다!");
            }

            // [추가]: '처음 우클릭 보상' 발생 시 1회만 로그
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
            isRewardOnCooldownHighlight = true;
            UpdateHighlight();
            return;
        }

        // 이미 쿨타임/사이클 진행 중이면 무시
        if (_isRewardCooldownActive) 
        {
            isRewardOnCooldownHighlight = true;
            UpdateHighlight();
            return;
        }

        // 사이클 시작: 수집만 시작(쿨타임은 지급 후 시작)
        _isRewardCooldownActive = true;
        _hasPaidThisCycle = false;
        _currentCollectionStep = 0;

        // 구역/총골드 계산 (null-세이프)
        _currentAreaType = AreaType.Normal;
        var mover = selfActor != null ? selfActor.GetComponent<Mover>() : null;
        var zone  = mover != null ? mover.GetLockedArea() : null;
        if (zone != null) _currentAreaType = zone.GetAreaType();

        // 총 골드도 동일 소스 기준으로 산출 - 가중치 반영
        long collectedBase = GameManager.instance.GetCollectedByAreaType(_currentAreaType);
        int actorWeight = PeopleManager.Instance.GetActorWeight(selfActor);
        _totalGoldAmountForCurrentCycle = collectedBase * actorWeight;


        _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());

        isRewardOnCooldownHighlight = true;
        UpdateHighlight();
    }


    private IEnumerator CollectGoldInStepsCoroutine()
    {
        float power = (HoverRewardController.Instance != null) ? HoverRewardController.Instance.CurrentPower : 0f;

        // 원의 파워 기반 동적 스텝 적용
        int dynSteps = gameConfig.GetDynamicSteps(_currentAreaType, power);
        _totalStepsForCurrentAttempt = Mathf.Max(1, dynSteps);

        _stepDelay = Mathf.Max(0f, gameConfig.GetGoldCollectionDelay(_currentAreaType));

        // 이미 이번 주기에 지급이 끝났다면 바로 종료
        if (_hasPaidThisCycle)
        {
            _goldCollectionCoroutine = null;
            yield break;
        }

        // 이미 진행된 스텝이 총 스텝 이상이면(오류) 즉시 종료
        if (_currentCollectionStep >= _totalStepsForCurrentAttempt)
        {
            _goldCollectionCoroutine = null;
            yield break;
        }

        while (_currentCollectionStep < _totalStepsForCurrentAttempt)
        {
            // 호버 없으면 ‘유예 대기’ 진입
            if (!isMouseOver)
            {
                float waited = 0f;
                while (!isMouseOver && waited < hoverResumeWindow)
                {
                    waited += Time.deltaTime;
                    yield return null; // 다음 프레임까지 대기
                }

                // 유예시간 내 복귀 못하면 그때 리셋
                if (!isMouseOver)
                {
                    _goldCollectionCoroutine = null;
                    _isRewardCooldownActive = false;
                    _hasPaidThisCycle = false;
                    isRewardOnCooldownHighlight = false;
                    _currentCollectionStep = 0;
                    _totalGoldAmountForCurrentCycle = 0;
                    _lastStepHighlightColor = originalColor;
                    UpdateHighlight();
                    yield break;
                }
                // 여기 도달하면 호버 복귀 → 진행 계속
            }

            _currentCollectionStep++;
            StartCoroutine(SpawnWhipStepEffectsCoroutine());

            // 단계별 하이라이트는 그대로 유지
          if (spriteRenderer != null)
            {
                int currentSteps = _totalStepsForCurrentAttempt;
                Color targetColor = EvaluateStepColor(_currentCollectionStep, currentSteps);
                spriteRenderer.color = targetColor;
                _lastStepHighlightColor = targetColor;
            }

            // ※마지막 스텝 이후에는 더 기다리지 않음 ※steps==1이면 즉시 지급 효과
            if (_currentCollectionStep < _totalStepsForCurrentAttempt)
            {
                // 지연 동안에도 호버가 끊길 수 있으니, 대기 자체를 프레임 단위로 나눠 감시
                float t = 0f;
                while (t < _stepDelay)
                {
                    if (!isMouseOver) break; // 다음 루프에서 유예 대기 처리
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }


        // 모든 단계 완료 → 한 번만 지급
        // ClickMode에 따른 배율 적용
        float multiplier = GetClickModeMultiplier();
        long finalGoldAmount = Mathf.RoundToInt(_totalGoldAmountForCurrentCycle * multiplier);
        
        GameManager.instance.DropGoldEasterEgg(dropObject, selfActor, finalGoldAmount);
        _hasPaidThisCycle = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.IncreaseAuthorityExp(finalGoldAmount);
        }
        if (emotionController != null)
        {
            emotionController.ExpressEmotion("Emotion_Loud");
        }

        _goldCollectionCoroutine = null;

        // 지급 직후에 '그때의 쿨타임' 시작 (중복 지급 차단의 핵심)
        if (_rewardCooldownCoroutine != null) StopCoroutine(_rewardCooldownCoroutine);
        _rewardCooldownCoroutine = StartCoroutine(
            StartRewardCooldownCoroutine(gameConfig.GetRewardCooldown(_currentAreaType))
        );
    }

    
    /// <summary>
    /// 현재 단계와 총 단계 수에 따라 그라데이션 색을 계산한다.
    /// 규칙:
    ///  - steps == 1  : 항상 끝 컬러(즉시 완료)
    ///  - steps == 2  : step1=0.5(중간), step2=1.0(끝)
    ///  - steps >= 3  : t = (currentStep-1)/(steps-1) → 0.0(시작) ~ 1.0(끝)
    /// </summary>
    private Color EvaluateStepColor(int currentStep, int steps)
    {
        Color start = startHighlightColor;
        Color end = endHighlightColor;

        if (!useGradientStepHighlight) return start;
        if (steps <= 0) return start;

        if (steps == 1) return end;
        if (steps == 2) return Color.Lerp(start, end, currentStep == 1 ? 0.5f : 1f);

        float t = Mathf.Clamp01((currentStep - 1f) / (steps - 1f));
        return Color.Lerp(start, end, t);
    }


    public void SetHovered(bool hovered)
    {
        if (!isBeingDragged)
        {
            isMouseOver = hovered;
            UpdateHighlight();

            // 호버 상태가 변경될 때 골드 수집 코루틴의 상태 관리
            if (isMouseOver) // true가 자주 들어옴
            {
                if (!_isRewardCooldownActive)
                {
                    TriggerReward();
                }
                else if (_goldCollectionCoroutine == null && !_hasPaidThisCycle) // ※이미 지급했다면 재시도 금지
                {
                    _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());
                }
            }

            else
            {
                // 코루틴을 멈추지 말고, Collect 코루틴 내부의 '유예 대기' 로직에 맡깁니다.
                // 하이라이트만 원복
                //isRewardOnCooldownHighlight = false;
            }


        }
    }

    private IEnumerator SpawnWhipStepEffectsCoroutine()
    {
        if (ObjectPooler.Instance == null || ClickModeManager.Instance == null)
        {
            yield break;
        }

        // 마우스 월드 위치 계산
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // 고양이 신인지 확인 (CatGodMover 또는 CatGodController 컴포넌트가 있으면 고양이 신)
        bool isCatGod = GetComponent<CatGodMover>() != null || GetComponent<CatGodController>() != null;

        // 고양이 신은 항상 Whip 이펙트만 사용
        if (isCatGod)
        {
            Vector3 swingPosition = transform.position + whipSwingEffectOffset;
            SpawnEffect(ObjectType.WhipSwingEffect, swingPosition);

            Vector3 impactPosition = transform.position + whipImpactEffectOffset;
            SpawnEffect(ObjectType.WhipImpactEffect, impactPosition);
            yield break;
        }

        // Whip 모드일 때는 기존처럼 두 개의 이펙트 (Swing + Impact) - 시민 위치 기준
        if (ClickModeManager.Instance.CurrentMode == ClickMode.Whip)
        {
            Vector3 swingPosition = transform.position + whipSwingEffectOffset;
            SpawnEffect(ObjectType.WhipSwingEffect, swingPosition);

            Vector3 impactPosition = transform.position + whipImpactEffectOffset;
            SpawnEffect(ObjectType.WhipImpactEffect, impactPosition);
        }
        else if (ClickModeManager.Instance.CurrentMode == ClickMode.Thunder)
        {
            // Thunder 모드일 때는 호버 원 크기에 따라 여러 개의 이펙트를 시간차로 생성
            int effectCount = CalculateEffectCount();
            float delayBetweenEffects = 0.05f; // 이펙트 간 지연 시간 (50ms)
            
            for (int i = 0; i < effectCount; i++)
            {
                Vector3 randomPos = GetRandomPositionInHoverCircle(mouseWorldPos);
                SpawnEffect(ObjectType.ThunderEffect, randomPos);
                SpawnEffect(ObjectType.ThunderImpactEffect, randomPos);
                
                // 마지막 이펙트가 아니면 대기
                if (i < effectCount - 1)
                {
                    yield return new WaitForSeconds(delayBetweenEffects);
                }
            }
        }
        else if (ClickModeManager.Instance.CurrentMode == ClickMode.Explode)
        {
            // Explode 모드일 때는 호버 원 크기에 따라 여러 개의 이펙트를 시간차로 생성
            int effectCount = CalculateEffectCount();
            float delayBetweenEffects = 0.08f; // 이펙트 간 지연 시간 (80ms)
            
            for (int i = 0; i < effectCount; i++)
            {
                Vector3 randomPos = GetRandomPositionInHoverCircle(mouseWorldPos);
                SpawnEffect(ObjectType.ExplodeEffect, randomPos);
                
                // 마지막 이펙트가 아니면 대기
                if (i < effectCount - 1)
                {
                    yield return new WaitForSeconds(delayBetweenEffects);
                }
            }
        }
    }

    private void SpawnWhipStepEffects()
    {
        if (ObjectPooler.Instance == null || ClickModeManager.Instance == null)
        {
            return;
        }

        // 마우스 월드 위치 계산
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // 고양이 신인지 확인 (CatGodMover 또는 CatGodController 컴포넌트가 있으면 고양이 신)
        bool isCatGod = GetComponent<CatGodMover>() != null || GetComponent<CatGodController>() != null;

        // 고양이 신은 항상 Whip 이펙트만 사용
        if (isCatGod)
        {
            Vector3 swingPosition = transform.position + whipSwingEffectOffset;
            SpawnEffect(ObjectType.WhipSwingEffect, swingPosition);

            Vector3 impactPosition = transform.position + whipImpactEffectOffset;
            SpawnEffect(ObjectType.WhipImpactEffect, impactPosition);
            return;
        }

        // 일반 시민은 현재 ClickMode에 따라 다른 이펙트 타입 선택
        ObjectType effectType = ObjectType.None;
        
        switch (ClickModeManager.Instance.CurrentMode)
        {
            case ClickMode.Whip:
                effectType = ObjectType.WhipSwingEffect;
                break;
            case ClickMode.Explode:
                effectType = ObjectType.ExplodeEffect;
                break;
            case ClickMode.Thunder:
                effectType = ObjectType.ThunderEffect;
                break;
        }

        if (effectType == ObjectType.None)
        {
            return;
        }

        // Whip 모드일 때는 기존처럼 두 개의 이펙트 (Swing + Impact) - 시민 위치 기준
        if (ClickModeManager.Instance.CurrentMode == ClickMode.Whip)
        {
            Vector3 swingPosition = transform.position + whipSwingEffectOffset;
            SpawnEffect(ObjectType.WhipSwingEffect, swingPosition);

            Vector3 impactPosition = transform.position + whipImpactEffectOffset;
            SpawnEffect(ObjectType.WhipImpactEffect, impactPosition);
        }
        else if (ClickModeManager.Instance.CurrentMode == ClickMode.Thunder)
        {
            // Thunder 모드일 때는 호버 원 크기에 따라 여러 개의 이펙트 생성
            int effectCount = CalculateEffectCount();
            for (int i = 0; i < effectCount; i++)
            {
                Vector3 randomPos = GetRandomPositionInHoverCircle(mouseWorldPos);
                SpawnEffect(ObjectType.ThunderEffect, randomPos);
                SpawnEffect(ObjectType.ThunderImpactEffect, randomPos);
            }
        }
        else if (ClickModeManager.Instance.CurrentMode == ClickMode.Explode)
        {
            // Explode 모드일 때는 호버 원 크기에 따라 여러 개의 이펙트 생성
            int effectCount = CalculateEffectCount();
            for (int i = 0; i < effectCount; i++)
            {
                Vector3 randomPos = GetRandomPositionInHoverCircle(mouseWorldPos);
                SpawnEffect(ObjectType.ExplodeEffect, randomPos);
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        var cam = Camera.main;
        if (cam == null) return Vector3.zero;

        Vector3 mousePos = Input.mousePosition;
        
        // 월드에서 쓰고 싶은 기준 평면의 z. 보통 2D는 0(스프라이트 z=0)로 사용
        const float worldPlaneZ = 0f;
        
        // 카메라에서 그 평면까지의 거리 = 평면z - 카메라z
        mousePos.z = worldPlaneZ - cam.transform.position.z;
        
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        return new Vector3(worldPos.x, worldPos.y, 0f);
    }

    /// <summary>
    /// 호버 원 안의 랜덤한 위치를 반환합니다.
    /// </summary>
    private Vector3 GetRandomPositionInHoverCircle(Vector3 center)
    {
        if (HoverRewardController.Instance == null)
        {
            return center;
        }

        // 호버 반경 가져오기
        float radius = HoverRewardController.Instance.hoverRadius;
        
        // 원 안의 랜덤한 점 생성 (균일 분포)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0f, radius);
        
        // 균일한 분포를 위해 거리에 제곱근 적용
        distance = Mathf.Sqrt(Random.Range(0f, 1f)) * radius;
        
        Vector3 randomOffset = new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            0f
        );
        
        return center + randomOffset;
    }

    /// <summary>
    /// 호버 원의 크기에 따라 생성할 이펙트 개수를 계산합니다.
    /// 반경 1.0당 약 1~2개의 이펙트가 생성됩니다.
    /// </summary>
    private int CalculateEffectCount()
    {
        if (HoverRewardController.Instance == null)
        {
            return 1;
        }

        float radius = HoverRewardController.Instance.hoverRadius;
        
        // 반경에 비례하여 이펙트 개수 계산
        // 반경 1.0 = 1~2개, 반경 2.0 = 2~3개, 반경 3.0 = 3~5개
        int baseCount = Mathf.Max(1, Mathf.FloorToInt(radius));
        int bonusCount = Mathf.FloorToInt(radius * 0.5f);
        
        return baseCount + bonusCount;
    }

    private static void SpawnEffect(ObjectType effectType, Vector3 spawnPosition)
    {
        if (ObjectPooler.Instance == null)
        {
            return;
        }

        if (!ObjectPooler.Instance.HasPool(effectType))
        {
            return;
        }

        ObjectPooler.Instance.SpawnObject(effectType, spawnPosition, Quaternion.identity);
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

            // [주석] 핵심 개정: 황금과 충성도를 하나의 명세로 묶습니다. [주석]
            // '보상 쿨타임'이 아닐 때만 아래를 실행합니다.
            if (dropObject != null && !isGoldDropOnCooldown)
            {
                // 2. 황금 하사
                GameManager.instance.DropGoldEasterEgg(dropObject);

                // 3. 충성도 고취
                if (selfActor != null)
                {
                    selfActor.ChangeLoyalty(loyaltyBoostAmount);
                    Debug.Log($"{selfActor.DisplayName}의 충성도가 {loyaltyBoostAmount}만큼 상승했습니다!");
                }

                // [추가]: '처음 우클릭 보상' 발생 시 1회만 로그
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
        _hasPaidThisCycle = false;           // ※다음 주기를 위해 지급 플래그 리셋

        isRewardOnCooldownHighlight = false;
        UpdateHighlight();
    }



    private void OnDeselected()
    {
        isSelected = false;
        UpdateHighlight();
    }

    /// <summary>
    /// 평상시의 색을 입히는 잡무
    /// </summary>
    private void UpdateHighlight()
    {
        // [주석] 핵심 개정: 긴급 명령(isFlashing)이 발령 중일 때는, 모든 평시 잡무를 중단한다! [주석]
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
        var cam = Camera.main;
        if (cam == null) return false;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }


    /// <summary>
    /// 외부 집행관이 호출할 '붉은 섬광' 명령
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
    /// 붉은 섬광을 잠시 보여주고 원래 색으로 되돌리는 잡무
    /// </summary>
    private IEnumerator FlashCoroutine()
    {
        // 1. 긴급 명령 깃발을 올린다.
        isFlashing = true;

        // 2. 몸을 붉게 물들인다.
        spriteRenderer.color = flashColor;

        // 3. 정해진 시간만큼 기다린다.
        yield return new WaitForSeconds(flashDuration);

        // 4. 긴급 명령 깃발을 내린다.
        isFlashing = false;
        flashCoroutine = null;

        // 5. 긴급 명령이 끝났으니, 다시 평상시의 색으로 돌아가도록 명한다.
        UpdateHighlight();
    }

    /// <summary>
    /// 현재 ClickMode에 따른 보상 배율을 반환합니다.
    /// Whip: 1배, Explode: 2배, Thunder: 3배
    /// </summary>
    private float GetClickModeMultiplier()
    {
        if (ClickModeManager.Instance == null)
            return 1f;

        // ClickModeManager에서 설정된 배수를 가져옵니다
        return ClickModeManager.Instance.GetCurrentMultiplier();
    }
}
