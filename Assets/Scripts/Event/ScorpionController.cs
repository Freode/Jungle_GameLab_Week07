using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 전갈 프리팹에 부착되어 클릭 및 시각적 피드백, 움직임, 스케일 애니메이션을 처리합니다。
/// </summary>
public class ScorpionController : MonoBehaviour, IPointerClickHandler
{
    private ScorpionEventSystem eventSystem; // 이벤트 시스템 참조
    private Image scorpionImage; // 전갈 이미지 컴포넌트
    private Color originalColor; // 원래 색상
    private Vector3 originalScale; // 원래 스케일

    [Header("Visuals")]
    [SerializeField] private float flashDuration = 0.1f; // 색상 깜빡이는 시간
    [SerializeField] private float scaleFactor = 1.1f; // 스케일 애니메이션 최대 배율
    [SerializeField] private float scaleDuration = 1.0f; // 스케일 애니메이션 한 사이클 시간

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 50f; // 이동 속도 (픽셀/초)
    [SerializeField] private float moveInterval = 2f; // 다음 목표 지점까지 이동하는 시간

    private Vector2 minSpawnBounds; // 이동 가능한 최소 좌표
    private Vector2 maxSpawnBounds; // 이동 가능한 최대 좌표

    public void Initialize(ScorpionEventSystem system, Vector2 minBounds, Vector2 maxBounds)
    {
        eventSystem = system;
        minSpawnBounds = minBounds;
        maxSpawnBounds = maxBounds;
    }

    private void Awake()
    {
        scorpionImage = GetComponent<Image>();
        if (scorpionImage != null)
        {
            originalColor = scorpionImage.color;
            originalScale = transform.localScale;
            Debug.Log($"[ScorpionController] Original color captured: {originalColor}, Original scale: {originalScale}");
        }
        else
        {
            Debug.LogError("[ScorpionController] Image component not found on Scorpion prefab!");
        }
    }

    private void Start()
    {
        StartCoroutine(MoveAroundCoroutine());
        StartCoroutine(ScaleAnimationCoroutine());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventSystem == null) return;

        eventSystem.OnScorpionClicked();
        StartCoroutine(FlashColor(Color.red));
        Debug.Log("[ScorpionController] Scorpion clicked, starting FlashColor coroutine.");
    }

    /// <summary>
/// 전갈 이미지를 지정된 색상으로 깜빡이는 코루틴입니다.
/// </summary>
    private IEnumerator FlashColor(Color flashColor)
    {
        if (scorpionImage == null) yield break;

        Color currentColor = scorpionImage.color; // 현재 색상 저장
        scorpionImage.color = flashColor; // 지정된 색상으로 변경
        Debug.Log($"[ScorpionController] Image color set to {flashColor}.");
        yield return new WaitForSeconds(flashDuration);
        scorpionImage.color = originalColor; // 원래 색상으로 복원
        Debug.Log("[ScorpionController] Image color restored to original.");
    }

    /// <summary>
/// 전갈이 캔버스 내에서 천천히 움직이도록 하는 코루틴입니다.
/// </summary>
    private IEnumerator MoveAroundCoroutine()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        while (true)
        {
            Vector2 targetPosition = new Vector2(
                Random.Range(minSpawnBounds.x, maxSpawnBounds.x),
                Random.Range(minSpawnBounds.y, maxSpawnBounds.y)
            );

            float journeyLength = Vector2.Distance(rectTransform.anchoredPosition, targetPosition);
            float startTime = Time.time;

            while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 1f)
            {
                float distCovered = (Time.time - startTime) * moveSpeed;
                float fractionOfJourney = distCovered / journeyLength;
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, fractionOfJourney);
                yield return null;
            }
            rectTransform.anchoredPosition = targetPosition; // 목표 위치에 정확히 도달
            yield return new WaitForSeconds(moveInterval); // 다음 이동 전 잠시 대기
        }
    }

    /// <summary>
/// 전갈의 스케일을 천천히 커졌다 작아지게 하는 코루틴입니다.
/// </summary>
    private IEnumerator ScaleAnimationCoroutine()
    {
        while (true)
        {
            // 커지는 애니메이션
            float timer = 0f;
            while (timer < scaleDuration / 2f)
            {
                transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, timer / (scaleDuration / 2f));
                timer += Time.deltaTime;
                yield return null;
            }

            // 작아지는 애니메이션
            timer = 0f;
            while (timer < scaleDuration / 2f)
            {
                transform.localScale = Vector3.Lerp(originalScale * scaleFactor, originalScale, timer / (scaleDuration / 2f));
                timer += Time.deltaTime;
                yield return null;
            }
            transform.localScale = originalScale; // 정확한 원래 스케일로 복원
        }
    }
}

