using UnityEngine;
using System.Collections;

public class ImageShaker : MonoBehaviour
{
    [Header("Channels to Subscribe")]
    public VoidEventChannelSO eventChannel;

    [Header("Shake Settings")]
    [Tooltip("흔들림이 지속될 시간(초)")]
    public float duration = 0.2f;

    [Tooltip("흔들림의 강도")]
    public float magnitude = 5f;

    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        if (eventChannel != null)
        {
            eventChannel.OnEventRaised += Shake;
        }
    }

    void OnDisable()
    {
        if (eventChannel != null)
        {
            eventChannel.OnEventRaised -= Shake;
        }
    }

    // 방송을 받으면 이 함수가 호출됨
    public void Shake()
    {
        // 이전에 실행 중인 흔들림이 있다면 중단하고 새 흔들림 시작
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            rectTransform.anchoredPosition = originalPosition; // 즉시 원위치
        }
        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        originalPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 랜덤한 방향으로 위치를 흔듦
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            rectTransform.anchoredPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 시간이 지나면 정확히 원래 위치로 복귀
        rectTransform.anchoredPosition = originalPosition;
        shakeCoroutine = null;
    }
}