// 파일 이름: CitizenHighlighter.cs (전면 개정안)
using UnityEngine;
using System.Collections;

public class CitizenHighlighter : MonoBehaviour
{
    [Header("Settings")]
    public Color mouseOverHighlightColor = new Color(1f, 1f, 0.5f, 1f);
    public Color selectedHighlightColor = Color.yellow;

    // ★★★ 핵심 개정: Animator 병사가 아닌, EmotionController 장군을 직접 섬기도록 변경 ★★★
    [Header("Emotion Settings")]
    [Tooltip("감정 표현을 총괄하는 EmotionController 스크립트입니다.")]
    public EmotionController emotionController;

    [Header("Channels to Subscribe")]
    public PeopleActorEventChannelSO OnPeopleSelectedChannel;
    public VoidEventChannelSO OnDeselectedChannel;

    [Header("Gold Reward Settings")]
    [Tooltip("선택되었을 때 떨어뜨릴 황금 주머니 오브젝트입니다.")]
    public GameObject dropObject;
    [Tooltip("황금과 충성심을 하사한 뒤, 다음 하사까지의 재사용 대기시간(초)입니다.")]
    public float rewardCooldown = 5.0f;
    [Tooltip("선택 시 상승할 충성심의 양입니다.")]
    public int loyaltyBoostAmount = 5;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isSelected = false;
    private bool isMouseOver = false;
    private bool isBeingDragged = false;
    private bool isGoldDropOnCooldown = false;
    private PeopleActor selfActor;
    [Tooltip("채찍에 맞았을 때 번쩍일 색상입니다.")]
    public Color flashColor = Color.red;
    [Tooltip("섬광이 지속될 시간(초)입니다.")]
    public float flashDuration = 0.2f;
    private Coroutine flashCoroutine; // 섬광 코루틴을 제어하기 위함
    private bool isFlashing = false;
    private static bool s_firstRClickLogged = false;
    private bool isRewardOnCooldownHighlight = false; // 쿨타임 중 하이라이트 유지 플래그
    void Awake()
    {
        selfActor = GetComponent<PeopleActor>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) { originalColor = spriteRenderer.color; }
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
            // 1. 감정 표현 (변경 없음)
            if (emotionController != null)
            {
                emotionController.ExpressEmotion("Emotion_Love");
            }

            // ★★★ 핵심 개정: 황금과 충성심을 하나의 어명으로 묶습니다. ★★★
            // '보상 쿨타임'이 아닐 때만 아래를 실행합니다.
            if (dropObject != null && !isGoldDropOnCooldown)
            {
                // 2. 황금 하사
                GameManager.instance.DropGoldEasterEgg(dropObject, selfActor);

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
                isRewardOnCooldownHighlight = true; // 쿨타임 시작 시 하이라이트 유지
                UpdateHighlight(); // 하이라이트 즉시 업데이트
                StartCoroutine(RewardCooldownCoroutine());
            }
        }
    }

    public void SetHovered(bool hovered)
    {
        if (!isBeingDragged)
        {
            isMouseOver = hovered;
            UpdateHighlight();
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
    // ★★★ 이름 변경 및 통합: GoldDropCooldownCoroutine -> RewardCooldownCoroutine ★★★
    private IEnumerator RewardCooldownCoroutine()
    {
        // 설정된 '보상 쿨타임' 시간만큼 기다립니다.
        yield return new WaitForSeconds(rewardCooldown);

        // 시간이 지나면, 다시 황금과 충성심을 하사할 수 있도록 쿨타임 깃발을 내립니다.
        isGoldDropOnCooldown = false;
        isRewardOnCooldownHighlight = false; // 쿨타임 종료 시 하이라이트 해제
        UpdateHighlight(); // 하이라이트 즉시 업데이트
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
        if (isSelected) { spriteRenderer.color = selectedHighlightColor; }
        else if (isRewardOnCooldownHighlight) { spriteRenderer.color = mouseOverHighlightColor; } // 쿨타임 중 하이라이트 유지
        else if (isMouseOver) { spriteRenderer.color = mouseOverHighlightColor; }
        else { spriteRenderer.color = originalColor; }
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
        if (flashCoroutine != null) { StopCoroutine(flashCoroutine); }
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