using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum MoveState
{
    Returning,
    Wandering,
    Dwelling,
    Carring,
    Sitting
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class Mover : MonoBehaviour
{
    [Header("Movement Settings")] public static float moveSpeed = 0.5f;
    public static float defaultMoveSpeed = 0.5f;
    [SerializeField] private float dwellTimeMin = 1f;
    [SerializeField] private float dwellTimeMax = 3f;

    // ★ 1. '운반자 전용' 휴식 시간 변수를 추가합니다.
    [Header("Carrier Settings")] [Tooltip("운반자가 짐을 싣고 내릴 때의 짧은 체류 시간입니다.")] [SerializeField]
    private float carrierDwellTimeMin = 0.1f;

    [SerializeField] private float carrierDwellTimeMax = 0.5f;
    [SerializeField] private float arrivalDistance = 0.1f;

    [Header("Debug")] [SerializeField] private bool showDebugGizmos = true;

    [Header("Death Settings")] [Tooltip("이동을 시작하기 전 1회 체크되는 즉사 확률(%)")] [Range(0f, 100f)] [SerializeField]
    private float deathChancePercent = 0.1f;

    [Tooltip("즉사 시 현재 위치에 생성할 프리팹(시체/유골 등)")] [SerializeField]
    private GameObject deathPrefab;

    private PeopleActor peopleActor;
    private MoveState currentState = MoveState.Dwelling; // 초기에는 대기 상태로 시작
    private AreaZone currentArea;
    public AreaZone lockedArea; // public으로 변경하여 외부에서 직접 설정 가능
    private Vector2 targetPosition;
    private float dwellTimer;
    private Rigidbody2D rb;
    private Vector2 lastValidPosition;
    private bool wasInsideArea = false;
    private bool isInitialized = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private string currentDwellAnimation;

    public bool isCarring = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        lastValidPosition = transform.position;
        targetPosition = transform.position; // 초기 목표를 현재 위치로 설정

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        peopleActor = GetComponent<PeopleActor>();
    }

    private void Start()
    {
        // Start에서 초기화 - lockedArea가 외부에서 설정된 후
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        AreaZone targetArea = lockedArea != null ? lockedArea : currentArea;
        if (targetArea != null)
        {
            wasInsideArea = targetArea.IsPointInside(transform.position);

            // 영역 내부에 있으면 바로 배회 시작
            if (wasInsideArea)
            {
                StartWandering();
            }
            else
            {
                // 영역 외부에 있으면 복귀
                ReturnToArea(targetArea);
            }
        }
        else
        {
            // 영역이 없으면 현재 위치에서 대기
            targetPosition = transform.position;
            StartDwelling();
        }

        isInitialized = true;
    }


    private void Update()
    {
        // 초기화가 안 됐으면 다시 시도
        if (!isInitialized)
        {
            Initialize();
        }

        // 영역 이탈 체크 (드래그나 임의 위치 변경 감지)
        CheckIfOutsideArea();

        // Dwelling 상태의 타이머만 Update에서 처리
        if (currentState == MoveState.Dwelling)
        {
            dwellTimer -= Time.deltaTime;
            if (dwellTimer <= 0)
            {
                // 만약 운반자면 여기서 로직 실행
                if (peopleActor.Job == JobType.Carrier)
                {
                    StartCarring();
                }

                StartWandering();
            }
        }

        UpdateAnimatorState();
        UpdateAnimatorSpeed();
    }

    private void FixedUpdate()
    {
        // 물리 기반 이동은 FixedUpdate에서 처리
        switch (currentState)
        {
            case MoveState.Returning:
            case MoveState.Wandering:
                MoveToTarget();
                break;
            case MoveState.Sitting:
                // 아무것도 하지 않음
                break;
        }
    }

    private void CheckIfOutsideArea()
    {
        AreaZone targetArea = lockedArea != null ? lockedArea : currentArea;

        if (targetArea != null)
        {
            bool isInside = targetArea.IsPointInside(transform.position);

            // 영역 내부에서 외부로 나간 경우
            if (wasInsideArea && !isInside)
            {
                ReturnToArea(targetArea);
            }

            wasInsideArea = isInside;
        }
    }

    private void MoveToTarget()
    {
        Vector2 currentPos = transform.position;
        Vector2 direction = (targetPosition - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, targetPosition);

        UpdateSpriteDirection(direction);

        if (distance > arrivalDistance)
        {
            // FixedUpdate에서는 Time.fixedDeltaTime 사용
            Vector2 newPosition = currentPos + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);

            // 영역 내부에 있을 때만 유효한 위치로 저장
            if (currentArea != null && currentArea.IsPointInside(newPosition))
            {
                lastValidPosition = newPosition;
            }
        }
        else
        {
            // 목표 도착
            if (currentState == MoveState.Returning)
            {
                // 복귀 완료 후 배회 시작
                StartWandering();
            }
            else if (currentState == MoveState.Wandering)
            {
                // 배회 중 목표 도착 시 대기
                StartDwelling();
            }
        }
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (spriteRenderer == null) return;

        // 수평 이동이 거의 없을 때는 방향을 바꾸지 않음
        if (Mathf.Abs(direction.x) < 0.01f) return;

        // 왼쪽으로 이동하면 true, 오른쪽이면 false로 설정
        spriteRenderer.flipX = direction.x < 0;
    }

    private void ReturnToArea(AreaZone area)
    {
        // null 이면 targetPosition 제자리로
        if (area == null)
        {
            targetPosition = transform.position;
            return;
        }

        currentState = MoveState.Returning;
        targetPosition = area.GetRandomPointInside();
        currentArea = area;
        wasInsideArea = false;
    }
    /// <summary>
    /// 불충으로 인한 죽음을 집행합니다. PeopleActor에 의해 호출됩니다.
    /// </summary>
    public void ExecuteDeathByDisloyalty()
    {
        // 1. 모든 이성과 움직임을 멈춥니다.
        this.enabled = false; // Mover 자신의 모든 활동을 중단!
        
        Draggable draggable = GetComponent<Draggable>();
        if (draggable != null)
        {
            draggable.enabled = false; // 드래그 기능도 정지시킵니다.
        }

        // 2. 폐하의 명대로, 숨겨진 'OnDeath' 동작을 취하게 합니다.
        if (animator != null)
        {
            // 모든 일반 동작을 멈추고, 죽음을 맞이할 준비를 합니다.
            ResetAnimationBools(); 
            animator.SetTrigger("OnDeath");
        }

        // 3. 죽음의 절차를 시작합니다.
        StartCoroutine(DeathProcessCoroutine());
    }

    // ★★★ 추가: 죽음의 절차를 순서대로 진행하는 임무 (코루틴) ★★★
    private IEnumerator DeathProcessCoroutine()
    {
        // 죽음의 동작이 끝날 때까지 잠시 기다립니다. (애니메이션 길이를 1.5초로 가정)
        yield return new WaitForSeconds(1.3f);

        // 죽음 이후의 절차를 진행합니다.
        // 1. 유골 생성 및 '신상 기록'
        if (deathPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, -9f);
            
            // 1-1. 유골을 소환합니다.
            GameObject skullObj = Instantiate(deathPrefab, spawnPosition, Quaternion.identity);
            
            // 1-2. 소환된 유골의 장부(DraggableSkull)를 찾아냅니다.
            DraggableSkull skull = skullObj.GetComponent<DraggableSkull>();
            
            // 1-3. 장부를 찾았다면, '자신(peopleActor)'의 정보를 새겨넣으라 명합니다!
            if (skull != null && peopleActor != null)
            {
                skull.Initialize(peopleActor);
            }
        }

        // 2. 시신 처리 (소멸)
        if (PeopleManager.Instance != null)
        {
            PeopleManager.Instance.DespawnPerson(this.gameObject);

            // 다시 활성화시키고 반환
            enabled = true;

            Draggable draggable = GetComponent<Draggable>();
            if (draggable != null)
            {
                draggable.enabled = true;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 클래스 내부 어딘가에 추가
    //private bool TrySuddenDeath()
    //{
    //    // normal 타입의 area에 있을때는 즉사 없음
    //    if (lockedArea != null && lockedArea.areaType == AreaType.Normal)
    //        return false;

    //    // 1%: Random.value < 0.01f (deathChancePercent 기준)
    //    float p = deathChancePercent / 100f;
    //    if (p <= 0f) return false;

    //    if (Random.value < p)
    //    {
    //        // 프리팹 스폰 (있을 때만)
    //        if (deathPrefab != null)
    //            Instantiate(deathPrefab, transform.position, transform.rotation);


    //        PeopleManager.Instance.DespawnPerson(this.gameObject);

    //        return true; // 죽음 발생
    //    }
    //    return false; // 생존
    //}

    void StartCarring()
    {
        if (isCarring)
        {
            PeopleManager.Instance.SetAreaLock(this.gameObject, AreaType.Carrier);
            isCarring = false;
            return;
        }

        CarrierItem carrierItem = peopleActor.CarrierItem;

        bool isStoneCarve = PeopleManager.Instance.checkUnlockStructures.ContainsKey(AreaType.StoneCarving);

        bool isArchitect = PeopleManager.Instance.checkUnlockStructures.ContainsKey(AreaType.Architect);

        switch (carrierItem)
        {
            case CarrierItem.None:
                PeopleManager.Instance.SetAreaLock(this.gameObject, AreaType.Mine);
                isCarring = true;
                break;
            case CarrierItem.Stone:
                if (isStoneCarve)
                {
                    PeopleManager.Instance.SetAreaLock(this.gameObject, AreaType.StoneCarving);
                    isCarring = true;
                }

                break;
            case CarrierItem.CarvedStone:
                if (isArchitect)
                {
                    PeopleManager.Instance.SetAreaLock(this.gameObject, AreaType.Architect);
                    isCarring = true;
                }

                break;
        }
    }


    public void Stop()
    {
        currentState = MoveState.Dwelling;
        targetPosition = transform.position;
    }

    public void StartWandering()
    {
        currentDwellAnimation = null;

        AreaZone targetArea = lockedArea != null ? lockedArea : currentArea;

        if (targetArea != null)
        {
            //if (TrySuddenDeath()) return; // 일단 안죽음

            currentState = MoveState.Wandering;
            targetPosition = targetArea.GetRandomPointInside();
        }
    }

    private void StartDwelling()
    {
        currentState = MoveState.Dwelling;

        // ★ 2. 신분을 확인하여 다른 법률을 적용합니다.
        // 만약 이 백성이 '운반자'라면,
        if (peopleActor.Job == JobType.Carrier)
        {
            // 운반자 전용의 짧은 휴식 시간을 부여합니다.
            dwellTimer = Random.Range(carrierDwellTimeMin, carrierDwellTimeMax);

            // 도착했으니 아이템 정보를 갱신합니다.
            AreaType destinationArea = lockedArea.areaType;
            switch (destinationArea)
            {
                case (AreaType.Mine):
                    peopleActor.SetCarrierItem(CarrierItem.Stone);
                    break;
                case (AreaType.StoneCarving):
                    peopleActor.SetCarrierItem(CarrierItem.CarvedStone);
                    break;
                case (AreaType.Architect):
                    peopleActor.SetCarrierItem(CarrierItem.None);
                    break;
                case (AreaType.Special):
                    peopleActor.SetCarrierItem(CarrierItem.None); // 특별 구역에서는 특별한 아이템 없음
                    break;
            }
        }
        // 운반자가 아닌 다른 모든 백성이라면,
        else
        {
            // 기존의 긴 휴식 시간을 부여합니다.
            dwellTimer = Random.Range(dwellTimeMin, dwellTimeMax);
        }

        DecideDwellAnimation();
    }

    private void DecideDwellAnimation()
    {
        currentDwellAnimation = null; // 기본값으로 초기화
        AreaZone targetArea = lockedArea != null ? lockedArea : currentArea;
        if (targetArea == null) return;

        // 운송자는 다른 애니메이션으로 변경
        if (peopleActor.Job == JobType.Carrier)
        {
            CarrierItem carrierItem = peopleActor.CarrierItem;

            switch (carrierItem)
            {
                case CarrierItem.None:
                    currentDwellAnimation = "IsCarrying";
                    break;
                case CarrierItem.Stone:
                    currentDwellAnimation = "IsCarryingRock";
                    break;
                case CarrierItem.CarvedStone:
                    currentDwellAnimation = "IsCarryingBlock";
                    break;
            }

            return;
        }

        // 신(God)은 특별한 애니메이션으로 변경
        if (peopleActor.Job == JobType.God)
        {
            currentDwellAnimation = "IsDoing"; // 신적인 행동
            return;
        }

        switch (targetArea.areaType)
        {
            case AreaType.Mine:
                currentDwellAnimation = "IsMining";
                break;
            case AreaType.Architect:
                currentDwellAnimation = Random.value < 0.5f ? "IsDigging" : "IsHammering";
                break;
            case AreaType.StoneCarving:
                currentDwellAnimation = Random.value < 0.5f ? "IsHammering" : "IsDoing";
                break;
            case AreaType.Barrack:
                currentDwellAnimation = "IsAttacking";
                break;
            case AreaType.Brewery:
                break;
            case AreaType.Temple:
                break;
            case AreaType.Special:
                currentDwellAnimation = "IsDoing"; // 특별한 구역에서의 행동
                break;
            // Normal, Carrier 등은 특별한 행동이 없으므로 null 유지
        }
    }

    // 영역 고정/해제
    public void LockToArea(AreaZone area)
    {
        lockedArea = area;

        isInitialized = false; // 재초기화 필요
        ReturnToArea(area);
    }

        public void UnlockArea()
        {
            lockedArea = null;
        }
    
        public void StartSitting(float duration)
        {
            currentState = MoveState.Sitting;
            StartCoroutine(SitForDuration(duration));
        }
    
        private IEnumerator SitForDuration(float duration)
        {
            yield return new WaitForSeconds(duration);
            StartWandering();
        }
    // 외부에서 호출하여 즉시 초기화 (스폰 직후 등)
    public void ForceInitialize()
    {
        isInitialized = false;
        Initialize();
    }

    public bool IsAreaLocked()
    {
        return lockedArea != null;
    }

    public AreaZone GetLockedArea()
    {
        return lockedArea;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AreaZone area = other.GetComponent<AreaZone>();
        if (area != null)
        {
            // 고정된 영역이 있으면 무시
            if (lockedArea != null) return;

            currentArea = area;
            wasInsideArea = true;

            // 처음 진입 시 배회 시작
            if (currentState != MoveState.Returning)
            {
                StartWandering();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        AreaZone area = other.GetComponent<AreaZone>();
        if (area != null && area == currentArea)
        {
            wasInsideArea = false;
            // 고정된 영역이 없을 때만 currentArea 해제
            if (lockedArea == null)
            {
                currentArea = null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // 현재 상태에 따라 색상 변경
        switch (currentState)
        {
            case MoveState.Returning:
                Gizmos.color = Color.red;
                break;
            case MoveState.Wandering:
                Gizmos.color = Color.green;
                break;
            case MoveState.Dwelling:
                Gizmos.color = Color.yellow;
                break;
        }

        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // 목표 지점 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, targetPosition);
        Gizmos.DrawWireSphere(targetPosition, 0.2f);
    }

    /// <summary>
    /// 현재 이동 상태와 지역 타입에 따라 애니메이터의 Bool 파라미터를 업데이트합니다.
    /// </summary>
    private void UpdateAnimatorState()
    {
        if (animator == null) return;

        ResetAnimationBools();


        bool isMoving = currentState == MoveState.Wandering || currentState == MoveState.Returning;

        AreaZone targetArea = lockedArea != null ? lockedArea : currentArea;


        if (isMoving)
        {
            if (targetArea != null && peopleActor.Job == JobType.Carrier)
            {
                CarrierItem carrierItem = peopleActor.CarrierItem;

                switch (carrierItem)
                {
                    case CarrierItem.None:
                        //animator.SetBool("IsCarrying", true);
                        animator.SetBool("IsWalking", true);
                        break;
                    case CarrierItem.Stone:
                        animator.SetBool("IsCarryingRock", true);
                        break;
                    case CarrierItem.CarvedStone:
                        animator.SetBool("IsCarryingBlock", true);
                        break;
                }
            }
            else
            {
                animator.SetBool("IsWalking", true);
            }
        }
        else // 멈춰있는 상태 (Dwelling)
        {
            if (currentState == MoveState.Sitting)
            {
                animator.SetBool("IsSitting", true);
            }
            // << 5. Dwelling 상태에서는 미리 결정된 애니메이션을 재생
            else if (!string.IsNullOrEmpty(currentDwellAnimation))
            {
                animator.SetBool(currentDwellAnimation, true);
            }
        }
    }

    /// <summary>
    /// 현재 이동 속도(moveSpeed)에 맞춰 애니메이터의 재생 속도를 조절합니다.
    /// </summary>
    private void UpdateAnimatorSpeed()
    {
        // 애니메이터가 없거나, 기본 속도가 0 이하면 오류 방지를 위해 실행하지 않습니다.
        if (animator == null || defaultMoveSpeed <= 0) return;

        // 현재 속도가 기본 속도의 몇 배인지 계산합니다. (예: 1.0 / 0.5 = 2배)
        float speedMultiplier = moveSpeed / defaultMoveSpeed;

        // 계산된 배율을 애니메이터의 속도에 그대로 적용합니다.
        animator.speed = speedMultiplier;
    }

    /// <summary>
    /// 모든 커스텀 애니메이션 Bool 파라미터를 false로 리셋합니다.
    /// UpdateAnimatorState에서 새 상태를 설정하기 전에 호출됩니다.
    /// </summary>
    private void ResetAnimationBools()
    {
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsMining", false);
        animator.SetBool("IsCarrying", false);
        animator.SetBool("IsCarryingRock", false);
        animator.SetBool("IsCarryingBlock", false);
        animator.SetBool("IsDigging", false);
        animator.SetBool("IsHammering", false);
        animator.SetBool("IsDoing", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsSitting", false);
    }


    // Public getters
    public MoveState GetCurrentState() => currentState;
    public bool IsSitting() => currentState == MoveState.Sitting;
    public AreaZone GetCurrentArea() => currentArea;
}