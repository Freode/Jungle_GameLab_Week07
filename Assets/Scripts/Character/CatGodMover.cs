using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class CatGodMover : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public AreaZone lockedArea;

    [Header("선택: 영역 콜라이더 직접 지정(없으면 lockedArea에서 시도)")]
    [SerializeField] private Collider2D areaCollider2D;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 targetPosition;
    private bool isMoving = false;

    private bool _paused = false;
    private bool _lifted = false;

    // ▼ 수동 앉기(우클릭 토글) 플래그
    private bool _manualSit = false;
    public bool IsManualSit => _manualSit;

    [SerializeField] private float resumeDelayAfterDrop = 0.35f;
    private float _resumeBlockedUntil = 0f;

    private static readonly int HashIsWalking = Animator.StringToHash("IsWalking");
    private static readonly int HashIsSitting = Animator.StringToHash("IsSitting");
    private static readonly int HashIsLifted  = Animator.StringToHash("IsLifted");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = (Vector2)transform.position;

        // Special 영역 자동 연결
        if (lockedArea == null)
        {
            var zones = FindObjectsOfType<AreaZone>(includeInactive: false);
            foreach (var z in zones)
            {
                if (z != null && z.GetAreaType() == AreaType.Special)
                {
                    SetArea(z);
                    break;
                }
            }
            if (lockedArea == null)
                Debug.LogWarning("[CatGodMover] AreaType.Special 영역을 씬에서 찾지 못했습니다.");
        }

        if (areaCollider2D == null && lockedArea != null)
            areaCollider2D = lockedArea.GetComponent<Collider2D>();
    }

    public void SetArea(AreaZone zone)
    {
        lockedArea = zone;
        if (lockedArea != null && areaCollider2D == null)
            areaCollider2D = lockedArea.GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (_paused || _lifted) return;
        if (isMoving) MoveToTarget();
    }

    private void MoveToTarget()
    {
        Vector2 currentPos = transform.position;
        Vector2 direction = (targetPosition - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, targetPosition);

        UpdateSpriteDirection(direction);

        if (distance > 0.1f)
        {
            Vector2 newPosition = currentPos + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
        else
        {
            isMoving = false;
            animator.SetBool(HashIsWalking, false);
        }
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.01f)
            spriteRenderer.flipX = direction.x > 0;
    }

    public void StartWandering()
    {
        if (_lifted || _manualSit) return; // 수동 앉기 중에는 배회 금지
        if (lockedArea == null) { Debug.LogWarning("[CatGodMover] lockedArea가 비어 있습니다."); return; }

        targetPosition = lockedArea.GetRandomPointInside();
        isMoving = true;
        animator.SetBool(HashIsWalking, true);
        animator.SetBool(HashIsSitting, false);
    }

    // 내부판정
    public bool IsInsideArea(Vector2 worldPos)
    {
        if (lockedArea != null)
            return lockedArea.IsPointInside(worldPos);
        if (areaCollider2D != null)
            return areaCollider2D.OverlapPoint(worldPos);
        return true;
    }

    public void Stop()
    {
        isMoving = false;
        targetPosition = transform.position;
        animator.SetBool(HashIsWalking, false);
        animator.SetBool(HashIsSitting, false);
    }

    public void StartSitting(float duration)
    {
        if (_lifted || _manualSit) return; // 수동 앉기 우선
        isMoving = false;
        animator.SetBool(HashIsWalking, false);
        animator.SetBool(HashIsSitting, true);
        StartCoroutine(SitForDuration(duration));
    }

    private IEnumerator SitForDuration(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            if (_lifted || _manualSit) break; // 들리거나 수동 앉기 전환 시 즉시 중단
            t += Time.deltaTime;
            yield return null;
        }
        if (!_manualSit) // 수동 앉기가 아니면만 해제
            animator.SetBool(HashIsSitting, false);
    }

    public bool IsSitting()
    {
        return animator.GetBool("IsSitting");
    }

    // === Lift 진입/해제 ===
    public void OnLiftStart()
    {
        _lifted = true;
        isMoving = false;
        targetPosition = transform.position;

        animator.SetBool(HashIsWalking, false);
        animator.SetBool(HashIsSitting, false); // 들고 있는 동안은 일단 해제(들림 애니 우선)
        animator.SetBool(HashIsLifted, true);
    }

    public void OnLiftEnd()
    {
        _lifted = false;
        animator.SetBool(HashIsLifted, false);

        // 드롭 직후 잠깐 Idle 유지
        ForceIdle(resumeDelayAfterDrop);

        // 수동 앉기 켜져 있으면 다시 앉기 상태로 복귀
        if (_manualSit)
        {
            animator.SetBool(HashIsWalking, false);
            animator.SetBool(HashIsSitting, true);
        }
    }

    public void ForceIdle(float seconds)
    {
        Stop();
        _resumeBlockedUntil = Time.time + Mathf.Max(0f, seconds);
    }

    public bool IsResumeBlocked => Time.time < _resumeBlockedUntil;
    public bool IsLifted() => _lifted;

    public void Pause()
    {
        _paused = true;
        isMoving = false;
        targetPosition = transform.position;
        animator.SetBool(HashIsWalking, false);
    }

    public void Resume()
    {
        _paused = false;
    }

    // === 수동 앉기 토글 API ===
    public void ToggleManualSit()
    {
        if (_manualSit) DisableManualSit();
        else EnableManualSit();
    }

    public void EnableManualSit()
    {
        _manualSit = true;
        isMoving = false;
        animator.SetBool(HashIsWalking, false);
        animator.SetBool(HashIsSitting, true);
        // 상태머신은 IsManualSit을 보고 알아서 대기
    }

    public void DisableManualSit()
    {
        _manualSit = false;
        // 해제 시, 바로 움직이지 않도록 짧은 Idle 유지(선택)
        ForceIdle(0.2f);
        animator.SetBool(HashIsSitting, false);
    }
}
