
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 보너스 효과의 시각적 표현을 관리합니다. (예: 화면 전체 페이드 효과)
/// </summary>
public class BonusEffectController : MonoBehaviour
{
    [SerializeField] private Image bonusEffectImage; // 화면을 덮는 UI 이미지
    [SerializeField] private float fadeDuration = 1.0f; // 페이드 효과 지속 시간
    [SerializeField] private float maxAlpha = 0.5f; // 최대 알파 값 (0~1 사이)

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (bonusEffectImage == null)
        {
            Debug.LogError("BonusEffectImage가 할당되지 않았습니다.");
            enabled = false;
            return;
        }
        // 시작 시 알파값을 0으로 설정
        Color color = bonusEffectImage.color;
        color.a = 0;
        bonusEffectImage.color = color;
        bonusEffectImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 페이드 인 효과를 시작합니다.
    /// </summary>
    public void StartFadeIn()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        bonusEffectImage.gameObject.SetActive(true);
        fadeCoroutine = StartCoroutine(Fade(0, maxAlpha));
    }

    /// <summary>
    /// 페이드 아웃 효과를 시작합니다.
    /// </summary>
    public void StartFadeOut()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(Fade(maxAlpha, 0));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        Color color = bonusEffectImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            bonusEffectImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        bonusEffectImage.color = color;

        if (endAlpha == 0)
        {
            bonusEffectImage.gameObject.SetActive(false);
        }
    }
}
