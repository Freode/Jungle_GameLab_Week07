
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class CatGodMover : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public AreaZone lockedArea;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 targetPosition;
    private bool isMoving = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
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
            animator.SetBool("IsWalking", false);
        }
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x > 0;
        }
    }

    public void StartWandering()
    {
        if (lockedArea == null)
        {
            Debug.LogWarning("lockedArea가 설정되지 않았습니다. 고양이 신이 움직이지 않습니다.");
            return;
        }

        targetPosition = lockedArea.GetRandomPointInside();
        isMoving = true;
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsSitting", false);
    }

    public void Stop()
    {
        isMoving = false;
        targetPosition = transform.position;
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsSitting", false);
    }

    public void StartSitting(float duration)
    {
        isMoving = false;
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsSitting", true);
        StartCoroutine(SitForDuration(duration));
    }

    private IEnumerator SitForDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetBool("IsSitting", false);
    }

    public bool IsSitting()
    {
        return animator.GetBool("IsSitting");
    }
}
