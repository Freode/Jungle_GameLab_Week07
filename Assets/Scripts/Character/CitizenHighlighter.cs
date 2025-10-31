// ?뚯씪 ?대쫫: CitizenHighlighter.cs (?꾨㈃ 媛쒖젙??
using Mono.Cecil;
using UnityEngine.Serialization;
using System.Collections;
using UnityEngine;

public class CitizenHighlighter : MonoBehaviour
{
    [Header("Settings")] public Color mouseOverHighlightColor = new Color(1f, 1f, 0.5f, 1f);
    public Color selectedHighlightColor = Color.yellow;
    [Tooltip("怨⑤뱶 ?섏쭛 ?④퀎蹂꾨줈 ?쒖떆???섏씠?쇱씠???됱긽 (理쒕? 5?④퀎)")]
    public Color[] stepHighlightColors = new Color[5];

    // ?끸쁾???듭떖 媛쒖젙: Animator 蹂묒궗媛 ?꾨땶, EmotionController ?κ뎔??吏곸젒 ?ш린?꾨줉 蹂寃??끸쁾??
    [Header("Emotion Settings")] [Tooltip("媛먯젙 ?쒗쁽??珥앷큵?섎뒗 EmotionController ?ㅽ겕由쏀듃?낅땲??")]
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
        if (!isBeingDragged)
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
        }
    }

    private IEnumerator CollectGoldInStepsCoroutine()
    {
        _totalStepsForCurrentAttempt = gameConfig.GetGoldCollectionSteps(_currentAreaType);
        _stepDelay = gameConfig.GetGoldCollectionDelay(_currentAreaType);

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
            if (!isMouseOver)
            {
                _goldCollectionCoroutine = null;
                yield break;
            }

            _currentCollectionStep++;
            SpawnWhipStepEffects();

            // ?④퀎蹂??섏씠?쇱씠?몃뒗 洹몃?濡??좎?
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

            // ??留덉?留??ㅽ뀦 ?댄썑?먮뒗 ??湲곕떎由ъ? ?딆쓬 ??steps==1?대㈃ 利됱떆 吏湲??④낵
            if (_currentCollectionStep < _totalStepsForCurrentAttempt)
            {
                yield return new WaitForSeconds(_stepDelay);
            }
        }

        // Update authority gauge via GameManager so UI reflects the change
        if (GameManager.instance != null)
        {
            GameManager.instance.IncreaseAuthorityExp(_totalGoldAmountForCurrentCycle);
        }

        // Trigger emotion only on the final step
        if (emotionController != null)
        {
            emotionController.ExpressEmotion("Emotion_Loud");
        }
        
        // 肄붾（??醫낅즺
        _goldCollectionCoroutine = null; // 肄붾（??李몄“ ?댁젣
    }

    public void SetHovered(bool hovered)
    {
        if (!isBeingDragged)
        {
            isMouseOver = hovered;
            UpdateHighlight();

            // ?몃쾭 ?곹깭媛 蹂寃쎈맆 ??怨⑤뱶 ?섏쭛 肄붾（???곹깭 愿由?
            if (isMouseOver) // true媛 ?먯＜ ?ㅼ뼱??
            {
                if (!_isRewardCooldownActive)
                {
                    TriggerReward();
                }
                else if (_goldCollectionCoroutine == null && !_hasPaidThisCycle) // ???대? 吏湲됲뻽?쇰㈃ ?ъ떆??湲덉?
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        return (hit.collider != null && hit.collider.gameObject == this.gameObject);
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
