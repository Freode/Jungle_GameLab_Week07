using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class GameOverButtonAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Vector2 startPosition = Vector2.zero;
    [SerializeField] private Vector2 targetPosition = new Vector2(-821f, -345f);
    [SerializeField] private float delayBeforeMove = 1f;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optional Visuals")]
    [SerializeField] private bool scaleUpEffect = true;
    [SerializeField] private float scaleStart = 0.6f;
    [SerializeField] private AudioClip appearSound;

    private RectTransform rectTransform;
    private AudioSource audioSource;
    private bool hasPlayed = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // 버튼 활성화될 때마다 연출 시작
        if (!hasPlayed)
        {
            hasPlayed = true;
            StartCoroutine(AnimateButton());
        }
    }

    private IEnumerator AnimateButton()
    {
        // 초기 세팅
        rectTransform.anchoredPosition = startPosition;
        if (scaleUpEffect)
            rectTransform.localScale = Vector3.one * scaleStart;

        // 사운드 재생
        if (appearSound != null)
            audioSource.PlayOneShot(appearSound);

        // 대기
        yield return new WaitForSeconds(delayBeforeMove);

        // 이동 및 스케일 애니메이션
        float elapsed = 0f;
        Vector2 start = rectTransform.anchoredPosition;
        Vector2 end = targetPosition;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float curvedT = moveCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.Lerp(start, end, curvedT);
            if (scaleUpEffect)
                rectTransform.localScale = Vector3.Lerp(Vector3.one * scaleStart, Vector3.one, curvedT);

            yield return null;
        }

        rectTransform.anchoredPosition = end;
        rectTransform.localScale = Vector3.one;
    }
}
