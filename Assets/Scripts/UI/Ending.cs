using TMPro;
using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class Ending : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI endingText;

    [Header("Fade In Settings")]
    [Tooltip("시작 전 대기 시간(초)")]
    public float delay = 0f;

    [Tooltip("페이드 인에 걸리는 시간(초)")]
    public float duration = 1.5f;

    [Tooltip("Time.timeScale=0 이어도 동작시키려면 On")]
    public bool useUnscaledTime = true;

    [Tooltip("시작 시 자동으로 재생")]
    public bool playOnStart = true;

    [Tooltip("커브로 가속/감속 조절")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Color _baseColor;

    void Awake()
    {
        if (endingText == null)
        {
            Debug.LogError("[Ending] TextMeshProUGUI가 없습니다.");
            enabled = false;
            return;
        }

        // 시작 알파 0으로 세팅
        _baseColor = endingText.color;
        var c = _baseColor;
        c.a = 0f;
        endingText.color = c;
    }

    void Start()
    {
        if (playOnStart) Play();
    }

    /// <summary>외부에서 호출해 페이드 인 시작</summary>
    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(CoFadeIn());
    }

    /// <summary>알파를 0으로 초기화</summary>
    public void ResetAlpha()
    {
        var c = endingText.color;
        c.a = 0f;
        endingText.color = c;
    }

    IEnumerator CoFadeIn()
    {
        if (delay > 0f)
        {
            float tWait = 0f;
            while (tWait < delay)
            {
                tWait += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }

        float t = 0f;
        while (t < duration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = ease.Evaluate(p);

            var c = _baseColor;
            c.a = eased;                  // 0→1로 알파 증가
            endingText.color = c;

            yield return null;
        }

        // 마무리로 알파를 1로 고정
        var final = _baseColor;
        final.a = 1f;
        endingText.color = final;
    }
}
