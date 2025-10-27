using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼에 무지개 색 반짝임 효과를 추가하는 컴포넌트
/// </summary>
[RequireComponent(typeof(Image))]
public class RainbowButtonEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float colorChangeSpeed = 2f;      // 색상 변경 속도
    [SerializeField] private float pulseSpeed = 3f;            // 펄스(밝기 변화) 속도
    [SerializeField] private float minAlpha = 0.7f;            // 최소 투명도
    [SerializeField] private float maxAlpha = 1f;              // 최대 투명도

    private Image targetImage;
    private bool isEffectActive = false;
    private float hueOffset = 0f;
    private Color originalColor;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        originalColor = targetImage.color;
    }

    private void Update()
    {
        if (!isEffectActive)
            return;

        // 무지개 색상 계산 (HSV 색 공간 사용)
        hueOffset += colorChangeSpeed * Time.deltaTime;
        if (hueOffset > 1f)
            hueOffset -= 1f;

        // 펄스 효과 (알파값 변화)
        float pulse = Mathf.Lerp(minAlpha, maxAlpha, 
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);

        // HSV에서 RGB로 변환하여 색상 적용
        Color rainbowColor = Color.HSVToRGB(hueOffset, 0.8f, 1f);
        rainbowColor.a = pulse;

        targetImage.color = rainbowColor;
    }

    /// <summary>
    /// 무지개 효과 활성화
    /// </summary>
    public void ActivateEffect()
    {
        isEffectActive = true;
        hueOffset = 0f;
    }

    /// <summary>
    /// 무지개 효과 비활성화 및 원래 색상으로 복구
    /// </summary>
    public void DeactivateEffect()
    {
        isEffectActive = false;
        targetImage.color = originalColor;
    }

    /// <summary>
    /// 효과 활성 상태 확인
    /// </summary>
    public bool IsEffectActive()
    {
        return isEffectActive;
    }

    /// <summary>
    /// 원래 색상 저장 (런타임에 색상이 변경된 경우)
    /// </summary>
    public void SaveOriginalColor()
    {
        if (!isEffectActive)
            originalColor = targetImage.color;
    }
}
