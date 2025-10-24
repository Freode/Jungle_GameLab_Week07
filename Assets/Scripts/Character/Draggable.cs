// 파일 이름: Draggable.cs (전면 개정안)
using UnityEngine;
using System.Collections;

public class Draggable : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("강에 빠진 후 몇 초 뒤에 사라질지 설정합니다.")]
    public float timeToDieInRiver = 5f;

    [Tooltip("오브젝트의 현재 상태를 나타냅니다. (예: 0 for Idle, 1 for Mining)")]
    public int currentState = 0;

    [Header("애니메이터 파라미터 이름")]
    [Tooltip("Hanging 상태를 제외한 모든 행동 상태의 Bool 파라미터 이름을 적어주세요.")]
    public string[] stateParameterNames = { "IsWalking", "IsMining", "IsSwimming", "IsDoing", "IsHammering", "IsDigging", "IsCarrying", "IsCarryingBlock", "IsCarryingRock" };

    [Header("흔들기 감지")]
    [Tooltip("이 값 이상의 '흔들림 에너지'가 모이면 이벤트가 발생")]
    public float shakeThreshold = 20f;
    [Tooltip("움직임에 따라 에너지가 얼마나 민감하게 쌓일지 결정")]
    public float shakeSensitivity = 0.01f;
    [Tooltip("가만히 있을 때, 에너지가 초당 얼마나 감소할지 결정")]
    public float shakeDecayRate = 20f;
    [Tooltip("흔들기 이벤트 발생 후 다음 감지까지 필요한 대기 시간.")]
    public float shakeCooldown = 3.0f;
    
    public GameObject dropObject;

    [Header("방송할 채널")]
    [Tooltip("백성이 선택되었을 때 보고를 올릴 채널입니다.")]
    public PeopleActorEventChannelSO OnPeopleSelectedChannel;
    [Header("감정 표현 설정")]
    [Tooltip("감정 표현을 제어할 자식 오브젝트의 Animator입니다.")]
    public Animator emotionAnimator;

    // --- 내부 변수 ---
    private Vector3 offset;
    private bool isDragging = false;
    private bool isOverRiver = false;
    private Coroutine deathCoroutine;
    private Animator anim;
    private Mover spriteMover;
    private PeopleActor selfActor;
    private float lastVelocityX = 0f;
    private float currentShakeEnergy = 0f;
    private bool isShakeOnCooldown = false;
    private Vector3 lastPosition;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteMover = GetComponent<Mover>();
        selfActor = GetComponent<PeopleActor>();
    }

    // ★★★ 핵심: 새로운 감찰 초소 'Update' ★★★
    void Update()
    {
        // 폐하께서 '왼손'을 누르시는 그 순간을 감지합니다 (마우스 왼쪽 버튼 클릭)
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 커서 아래에 있는 것이 '나' 자신인지 확인합니다.
            if (IsMouseCurrentlyOver())
            {
                GameLogger.Instance.click.AddInteractClick();
                HandleDragStart();
            }
        }

        // 폐하께서 '왼손'을 떼시는 그 순간을 감지합니다.
        if (Input.GetMouseButtonUp(0))
        {
            // 드래그 중이었다면, 드래그를 종료하는 명을 내립니다.
            if (isDragging)
            {
                HandleDragEnd();
            }
        }

        // 폐하께서 '왼손'을 누르고 계시는 동안 계속 감지합니다.
        if (Input.GetMouseButton(0))
        {
            // 드래그 중일 때만 백성을 이끕니다.
            if (isDragging)
            {
                HandleDragging();
            }
        }
    }
    
    // 드래그 시작을 처리하는 새로운 임무
    void HandleDragStart()
    {
        if (selfActor != null && OnPeopleSelectedChannel != null)
        {
            OnPeopleSelectedChannel.RaiseEvent(selfActor);
        }
        
        if (isOverRiver) return;

        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
        lastPosition = transform.position; // 흔들기 감지를 위해 초기 위치 저장

        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }

        if (anim != null)
        {
            ResetAllStateBools();
            anim.SetTrigger("OnHang");
        }
    }

    // 드래그 중일 때 처리하는 새로운 임무
    void HandleDragging()
    {
        transform.position = GetMouseWorldPos() + offset;
        if (dropObject != null)
            DetectShaking();
    }

    // 드래그 종료를 처리하는 새로운 임무
    void HandleDragEnd()
    {
        isDragging = false;

        if (anim != null)
        {
            anim.SetTrigger("OnDrop");
        }

        if (isOverRiver)
        {
            if (anim != null)
            {
                anim.SetBool("IsSwimming", true);
            }
            deathCoroutine = StartCoroutine(DieInRiver());
        }
    }
    
    // 마우스가 현재 이 오브젝트 위에 있는지 확인하는 임무
    private bool IsMouseCurrentlyOver()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        return (hit.collider != null && hit.collider.gameObject == this.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("River"))
        {
            isOverRiver = true;
            // Mover.moveSpeed = 0f;
        }
        else if (other.CompareTag("Jail"))
        {
            Debug.Log("감옥에 들어갔습니다!");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("River"))
        {
            isOverRiver = false;
            Mover.moveSpeed = Mover.defaultMoveSpeed;

            if (anim != null)
            {
                anim.SetBool("IsSwimming", false);
            }

            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
                deathCoroutine = null;
            }
        }
        else if (other.CompareTag("Jail"))
        {
            Debug.Log("감옥에서 나왔습니다!");
        }
    }

    void ResetAllStateBools()
    {
        foreach (string paramName in stateParameterNames)
        {
            anim.SetBool(paramName, false);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    // [핵심 수정] DieInRiver 코루틴 변경
    IEnumerator DieInRiver()
    {
        spriteMover.LockToArea(null);
        // 1. 설정된 시간만큼 기다림
        yield return new WaitForSeconds(timeToDieInRiver);

        // 2. 시간이 지난 후에도 여전히 강 위에 있는지 최종 확인
        if (isOverRiver)
        {
            // 3. 죽음이 확정되면 더 이상 드래그할 수 없도록 이 스크립트를 비활성화
            this.enabled = false;

            // 4. 죽는 애니메이션 재생
            if (anim != null)
            {
                anim.SetTrigger("OnSwimDeath");
            }



            // 5. 애니메이션이 끝날 때까지 기다림 (애니메이션 길이를 1초로 가정)
            //    만약 애니메이션 길이가 다르다면 이 숫자를 맞춰주세요.
            yield return new WaitForSeconds(1f);

            // 6. 애니메이션이 끝난 후 오브젝트 파괴
            PeopleManager.Instance.DespawnPerson(this.gameObject);
        }
    }

    //흔들기를 감지하는 핵심 로직
    void DetectShaking()
    {
        Debug.Log("Detect");
        // 현재 프레임의 속도 계산
        Vector3 currentVelocity = (transform.position - lastPosition) / Time.deltaTime;

        // X축(좌우) 방향이 이전 프레임과 반대일 때 에너지를 더함 (핵심!)
        if (Mathf.Sign(currentVelocity.x) != Mathf.Sign(lastVelocityX) && lastVelocityX != 0)
        {
            // 속도가 빠를수록 더 많은 에너지를 얻음
            currentShakeEnergy += Mathf.Abs(currentVelocity.x) * shakeSensitivity;
        }

        // 에너지를 서서히 감소시킴
        currentShakeEnergy -= shakeDecayRate * Time.deltaTime;
        currentShakeEnergy = Mathf.Max(0, currentShakeEnergy); // 에너지가 0 밑으로 내려가지 않도록 함

        // 에너지가 임계값을 넘으면 이벤트 발생!
        if (currentShakeEnergy >= shakeThreshold && isShakeOnCooldown == false)
        {
            OnShakeDetected();
            currentShakeEnergy = 0f; // 이벤트 발생 후 에너지 초기화
        }

        // 현재 위치와 속도를 다음 프레임 계산을 위해 저장
        lastPosition = transform.position;
        if (Mathf.Abs(currentVelocity.x) > 0.1f) // 아주 작은 움직임은 무시
        {
            lastVelocityX = currentVelocity.x;
        }
    }

    // 흔들기가 감지되었을 때 실행될 함수
    void OnShakeDetected()
    {
        isShakeOnCooldown = true;
        StartCoroutine(ShakeCooldownCoroutine());
        Debug.Log("Shake");
        GameManager.instance.DropGoldEasterEgg(dropObject);
        if (emotionAnimator != null)
        {
            // "Emoji_Question"이라는 신호(Trigger)를 보내어 감정을 표출시킵니다.
            // (백성이 "어찌하여 나를 흔드시나이까?" 하고 묻는 듯한 감정이옵니다)
            emotionAnimator.SetTrigger("Emoji_Question");
        }
    }

    // 쿨타임 관리 코루틴
    IEnumerator ShakeCooldownCoroutine()
    {
        // 설정된 쿨타임 시간만큼 기다림
        yield return new WaitForSeconds(shakeCooldown);

        // 쿨타임이 끝나면 플래그를 다시 false로 변경
        isShakeOnCooldown = false;
        Debug.Log("흔들기 쿨타임이 종료되었습니다.");
    }
}