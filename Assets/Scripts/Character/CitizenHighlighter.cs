// ?뚯씪 ?대쫫: CitizenHighlighter.cs (?꾨㈃ 媛쒖젙??
using Mono.Cecil;
using UnityEngine.Serialization;
using System.Collections;
using UnityEngine;

public class CitizenHighlighter : MonoBehaviour
{
    [Header("Settings")] 
    public Color selectedHighlightColor = Color.yellow;

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

    [Header("Gold Reward Settings")] [Tooltip("?좏깮?섏뿀?????⑥뼱?⑤┫ ?⑷툑 二쇰㉧???ㅻ툕?앺듃?낅땲??")]
    public GameObject dropObject;

    [Tooltip("寃뚯엫???ㅼ젙???대뒗 GameConfig ScriptableObject?낅땲??")]
    public GameConfig gameConfig;
    [Header("Whip Effect Settings")]
    [FormerlySerializedAs("whipEffectOffset")]
    [Tooltip("World-space offset for whip swing visual effect")]
    [SerializeField] private Vector3 whipSwingEffectOffset = new Vector3(0f, 1.25f, 0f);
    [Tooltip("World-space offset for whip impact visual effect")]
    [SerializeField] private Vector3 whipImpactEffectOffset = new Vector3(0f, 1f, 0f);

    [Tooltip("?좏깮 ???곸듅??異⑹꽦?ъ쓽 ?묒엯?덈떎.")] public int loyaltyBoostAmount = 5;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isSelected = false;
    private bool isMouseOver = false;
    private bool isBeingDragged = false;
    private bool isGoldDropOnCooldown = false;
    private PeopleActor selfActor;
    [Tooltip("梨꾩컢??留욎븯????踰덉찉???됱긽?낅땲??")] public Color flashColor = Color.red;
    [Tooltip("?ш킅??吏?띾맆 ?쒓컙(珥??낅땲??")] public float flashDuration = 0.2f;
    private Coroutine flashCoroutine; // ?ш킅 肄붾（?댁쓣 ?쒖뼱?섍린 ?꾪븿
    private bool isFlashing = false;
    private static bool s_firstRClickLogged = false;
    private bool isRewardOnCooldownHighlight = false; // 荑⑦???以??섏씠?쇱씠???좎? ?뚮옒洹?

    // 怨⑤뱶 ?섏쭛 諛?荑⑤떎???곹깭 蹂??
    private bool _isRewardCooldownActive = false; // 蹂댁긽 荑⑤떎???쒖꽦???щ?
    private Coroutine _rewardCooldownCoroutine = null; // 蹂댁긽 荑⑤떎??肄붾（??李몄“
    private Coroutine _goldCollectionCoroutine = null; // 怨⑤뱶 ?섏쭛 肄붾（??李몄“
    private AreaType _currentAreaType; // ?꾩옱 ?쇨씔???랁븳 AreaType

    // ?꾩옱 怨⑤뱶 ?섏쭛 ?쒕룄 ?곹깭
    private int _currentCollectionStep = 0; // ?꾩옱 ?섏쭛 ?쒕룄?먯꽌 吏꾪뻾???④퀎
    private long _goldPerStep = 0; // ?꾩옱 ?섏쭛 ?쒕룄?먯꽌 ?④퀎蹂?怨⑤뱶 ??
    private int _totalStepsForCurrentAttempt = 0; // ?꾩옱 ?섏쭛 ?쒕룄??珥??④퀎 ??
    private float _stepDelay = 0; // ?꾩옱 ?섏쭛 ?쒕룄???④퀎蹂?吏???쒓컙

    // ?꾩옱 荑⑤떎??二쇨린 ?숈븞??怨⑤뱶 ?섏쭛 ?곹깭
    private long _totalGoldAmountForCurrentCycle = 0; // ?꾩옱 荑⑤떎??二쇨린 ?숈븞 ?살쓣 ???덈뒗 珥?怨⑤뱶
    private Color _lastStepHighlightColor; // 留덉?留됱쑝濡??곸슜???④퀎蹂??섏씠?쇱씠???됱긽
    
    // 蹂댁긽 1??吏湲??щ?(?꾩옱 荑⑤떎??二쇨린 ?숈븞)
    private bool _hasPaidThisCycle = false;

    // 援ъ뿭 ?뺣낫瑜??댁? 而댄룷?뚰듃
    private Mover _spriteMover;

    void Awake()
    {
        selfActor = GetComponent<PeopleActor>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteMover = GetComponent<Mover>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            _lastStepHighlightColor = originalColor; // 珥덇린??
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
    //         // 1. 媛먯젙 ?쒗쁽 (蹂寃??놁쓬)
    //         if (emotionController != null)
    //         {
    //             emotionController.ExpressEmotion("Emotion_Love");
    //         }
    //
    //         // ?끸쁾???듭떖 媛쒖젙: ?⑷툑怨?異⑹꽦?ъ쓣 ?섎굹???대챸?쇰줈 臾띠뒿?덈떎. ?끸쁾??
    //         // '蹂댁긽 荑⑦??????꾨땺 ?뚮쭔 ?꾨옒瑜??ㅽ뻾?⑸땲??
    //         if (dropObject != null && !isGoldDropOnCooldown)
    //         {
    //             // 2. ?⑷툑 ?섏궗
    //             GameManager.instance.DropGoldEasterEgg(dropObject);
    //
    //             // 3. 異⑹꽦??怨좎랬
    //             if (selfActor != null)
    //             {
    //                 selfActor.ChangeLoyalty(loyaltyBoostAmount);
    //                 Debug.Log($"{selfActor.DisplayName}??異⑹꽦?꾧? {loyaltyBoostAmount}留뚰겮 ?곸듅?덉뒿?덈떎!");
    //             }
    //
    //             // ??異붽?: '泥섏쓬 ?고겢由?蹂댁긽' 諛쒖깮 ??1?뚮쭔 濡쒓렇
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
    //             // 4. 荑⑦????쒖옉
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
            // dropObject媛 ?놁쑝硫?怨⑤뱶 愿??濡쒖쭅???섑뻾?섏? ?딆쓬
            if (dropObject == null) return;

            // 1. 媛먯젙 ?쒗쁽 (?댁젣 留덉?留?怨⑤뱶 ?쒕∼ ?쒖젏??泥섎━)
            // if (emotionController != null)
            // {
            //     emotionController.ExpressEmotion("Emotion_Love");
            // }

            // ?끸쁾???듭떖 媛쒖젙: ?⑷툑怨?異⑹꽦?ъ쓣 ?섎굹???대챸?쇰줈 臾띠뒿?덈떎. ?끸쁾??
            // 荑⑤떎?댁씠 ?쒖꽦?붾릺???덉? ?딅떎硫??덈줈 ?쒖옉
            if (!_isRewardCooldownActive)
            {
                // ?꾩옱 AreaType 媛?몄삤湲?
                _currentAreaType = AreaType.Normal; // 湲곕낯媛?
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

                // 荑⑤떎???쒖옉
                _isRewardCooldownActive = true;
                _hasPaidThisCycle = false;            // ???대쾲 二쇨린?먮뒗 ?꾩쭅 吏湲???
                _rewardCooldownCoroutine = StartCoroutine(
                    StartRewardCooldownCoroutine(gameConfig.GetRewardCooldown(_currentAreaType))
                );

                // 怨⑤뱶 ?섏쭛 ?곹깭 珥덇린??
                _currentCollectionStep = 0;

                // ?꾩옱 援ъ뿭 媛?몄삤湲?
                AreaType areaType = _spriteMover.GetCurrentArea().areaType;

                // 珥?怨⑤뱶 ??怨꾩궛 (??踰덈쭔 怨꾩궛)
                _totalGoldAmountForCurrentCycle = GameManager.instance.GetCollectedByAreaType(areaType);

                // 怨⑤뱶 ?섏쭛 肄붾（???쒖옉
                _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());
            }
            // 荑⑤떎?댁씠 ?쒖꽦?붾릺???덇퀬, 怨⑤뱶 ?섏쭛 肄붾（?댁씠 ?ㅽ뻾 以묒씠 ?꾨땲?쇰㈃ ?덈줈 ?쒖옉
            else if (_goldCollectionCoroutine == null)
            {
                _goldCollectionCoroutine = StartCoroutine(CollectGoldInStepsCoroutine());
            }

            // 3. 異⑹꽦??怨좎랬
            if (selfActor != null)
            {
                selfActor.ChangeLoyalty(loyaltyBoostAmount);
                Debug.Log($"{selfActor.DisplayName}??異⑹꽦?꾧? {loyaltyBoostAmount}留뚰겮 ?곸듅?덉뒿?덈떎!");
            }

            // ??異붽?: '泥섏쓬 ?고겢由?蹂댁긽' 諛쒖깮 ??1?뚮쭔 濡쒓렇
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

            // 荑⑦???以??섏씠?쇱씠???좎?
            isRewardOnCooldownHighlight = true;
            UpdateHighlight(); // ?섏씠?쇱씠??利됱떆 ?낅뜲?댄듃
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

        // 총 골드도 동일 소스 기준으로 산출
        _totalGoldAmountForCurrentCycle = GameManager.instance.GetCollectedByAreaType(_currentAreaType);


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

        // ?대? ?대쾲 二쇨린??吏湲됱씠 ?앸궗?ㅻ㈃ 諛붾줈 醫낅즺
        if (_hasPaidThisCycle)
        {
            _goldCollectionCoroutine = null;
            yield break;
        }

        // ?대? 吏꾪뻾???ㅽ뀦??珥??ㅽ뀦 ?댁긽?대㈃(?ъ쭊???? 利됱떆 醫낅즺
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
            SpawnWhipStepEffects();

            // ?④퀎蹂??섏씠?쇱씠?몃뒗 洹몃?濡??좎?
            if (spriteRenderer != null && stepHighlightColors != null && stepHighlightColors.Length >= 5)
            {
                int currentSteps = _totalStepsForCurrentAttempt;
                Color targetColor = EvaluateStepColor(_currentCollectionStep, currentSteps);
                spriteRenderer.color = targetColor;
                _lastStepHighlightColor = targetColor;
            }

            // ??留덉?留??ㅽ뀦 ?댄썑?먮뒗 ??湲곕떎由ъ? ?딆쓬 ??steps==1?대㈃ 利됱떆 吏湲??④낵
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
        GameManager.instance.DropGoldEasterEgg(dropObject, selfActor, _totalGoldAmountForCurrentCycle);
        _hasPaidThisCycle = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.IncreaseAuthorityExp(_totalGoldAmountForCurrentCycle);
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

            // 호버 상태가 변경될 때 골드 수집 코루틴 상태 관리
            if (isMouseOver)
            {
                if (!_isRewardCooldownActive)
                {
                    TriggerReward();
                }
                else if (_goldCollectionCoroutine == null && !_hasPaidThisCycle)
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

    private void SpawnWhipStepEffects()
    {
        if (ObjectPooler.Instance == null)
        {
            return;
        }

        Vector3 swingPosition = transform.position + whipSwingEffectOffset;
        SpawnWhipEffect(ObjectType.WhipSwingEffect, swingPosition);

        Vector3 impactPosition = transform.position + whipImpactEffectOffset;
        SpawnWhipEffect(ObjectType.WhipImpactEffect, impactPosition);
    }

    private static void SpawnWhipEffect(ObjectType effectType, Vector3 spawnPosition)
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

            /*// 1. 媛먯젙 ?쒗쁽 (蹂寃??놁쓬)
            if (emotionController != null)
            {
                emotionController.ExpressEmotion("Emotion_Love");
            }

            // ?끸쁾???듭떖 媛쒖젙: ?⑷툑怨?異⑹꽦?ъ쓣 ?섎굹???대챸?쇰줈 臾띠뒿?덈떎. ?끸쁾??
            // '蹂댁긽 荑⑦??????꾨땺 ?뚮쭔 ?꾨옒瑜??ㅽ뻾?⑸땲??
            if (dropObject != null && !isGoldDropOnCooldown)
            {
                // 2. ?⑷툑 ?섏궗
                GameManager.instance.DropGoldEasterEgg(dropObject);

                // 3. 異⑹꽦??怨좎랬
                if (selfActor != null)
                {
                    selfActor.ChangeLoyalty(loyaltyBoostAmount);
                    Debug.Log($"{selfActor.DisplayName}??異⑹꽦?꾧? {loyaltyBoostAmount}留뚰겮 ?곸듅?덉뒿?덈떎!");
                }

                // ??異붽?: '泥섏쓬 ?고겢由?蹂댁긽' 諛쒖깮 ??1?뚮쭔 濡쒓렇
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

                // 4. 荑⑦????쒖옉
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

    // 蹂댁긽 荑⑤떎?댁쓣 ?쒖옉?섍퀬 愿由ы븯??肄붾（??
    private IEnumerator StartRewardCooldownCoroutine(float cooldownDuration)
    {
        yield return new WaitForSeconds(cooldownDuration);

        _isRewardCooldownActive = false;
        _rewardCooldownCoroutine = null;
        _currentCollectionStep = 0;
        _totalGoldAmountForCurrentCycle = 0;
        _lastStepHighlightColor = originalColor;
        _hasPaidThisCycle = false;           // ???ㅼ쓬 二쇨린瑜??꾪빐 吏湲??뚮옒洹?由ъ뀑

        isRewardOnCooldownHighlight = false;
        UpdateHighlight();
    }



    private void OnDeselected()
    {
        isSelected = false;
        UpdateHighlight();
    }

    /// <summary>
    /// ?됱긽?쒖쓽 ?덉깋???댄뵾???꾨Т
    /// </summary>
    private void UpdateHighlight()
    {
        // ?끸쁾???듭떖 媛쒖젙: 湲닿툒 ?대챸(isFlashing)??諛쒕졊 以묒씪 ?뚮뒗, 紐⑤뱺 ?됱떆 ?꾨Т瑜?以묐떒?쒕떎! ?끸쁾??
        if (isFlashing) return;

        if (spriteRenderer == null) return;
        if (isSelected)
        {
            spriteRenderer.color = selectedHighlightColor;
        }
        else if (isRewardOnCooldownHighlight)
        {
            spriteRenderer.color = _lastStepHighlightColor; // 荑⑦???以묒뿉??留덉?留??ㅽ뀦 ?됱긽 ?좎?
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
    /// ?뺣쾶 吏묓뻾愿???몄텧??'遺됱? ?ш킅' ?대챸
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
    /// 遺됱? ?ш킅???좎떆 蹂댁뿬二쇨퀬 ?먮옒 ?됱쑝濡??섎룎由щ뒗 ?꾨Т
    /// </summary>
    private IEnumerator FlashCoroutine()
    {
        // 1. 湲닿툒 ?대챸 源껊컻???щ┛??
        isFlashing = true;

        // 2. 紐몄쓣 遺됯쾶 臾쇰뱾?몃떎.
        spriteRenderer.color = flashColor;

        // 3. ?뺥빐吏??쒓컙留뚰겮 湲곕떎由곕떎.
        yield return new WaitForSeconds(flashDuration);

        // 4. 湲닿툒 ?대챸 源껊컻???대┛??
        isFlashing = false;
        flashCoroutine = null;

        // 5. 湲닿툒 ?대챸???앸궗?쇰땲, ?ㅼ떆 ?됱긽?쒖쓽 ?덉깋?쇰줈 ?뚯븘媛?꾨줉 紐낇븳??
        UpdateHighlight();
    }
}
