using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMPro 네임스페이스 추가

/// <summary>
/// UI 요소(이미지, 텍스트)에 무지개 색 반짝임 효과를 추가하는 컴포넌트
/// </summary>
public class RainbowButtonEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("true로 설정하면 시작하자마자 효과가 활성화됩니다.")]
    [SerializeField] private bool activateOnStart = false;
    [SerializeField] private float colorChangeSpeed = 2f;      // 색상 변경 속도
    [SerializeField] private float pulseSpeed = 3f;            // 펄스(밝기 변화) 속도
    [SerializeField] private float minAlpha = 0.7f;            // 최소 투명도
    [SerializeField] private float maxAlpha = 1f;              // 최대 투명도

    // Image와 TMP_Text의 공통 부모인 Graphic을 사용하여 두 컴포넌트 모두 지원
    private Graphic targetGraphic; 
    private bool isEffectActive = false;
    private float hueOffset = 0f;
    private Color originalColor;

    private void Awake()
    {
        // Graphic 컴포넌트를 가져옴 (Image, RawImage, Text, TMP_Text 등)
        targetGraphic = GetComponent<Graphic>(); 
        if (targetGraphic == null)
        {
            Debug.LogError("RainbowButtonEffect: 이 게임오브젝트에 Image 또는 TextMeshProUGUI 컴포넌트가 없습니다.");
            enabled = false; // 컴포넌트를 찾지 못하면 스크립트 비활성화
            return;
        }
        originalColor = targetGraphic.color;
    }

    private void Start()
    {
        if (activateOnStart)
        {
            ActivateEffect();
        }
    }

    private void Update()
    {
        if (!isEffectActive || targetGraphic == null)
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

        targetGraphic.color = rainbowColor;
    }

    /// <summary>
    /// 무지개 효과 활성화
    /// </summary>
    public void ActivateEffect()
    {
        if (targetGraphic == null) return;
        isEffectActive = true;
        hueOffset = 0f;
    }

    /// <summary>
    /// 무지개 효과 비활성화 및 원래 색상으로 복구
    /// </summary>
    public void DeactivateEffect()
    {
        if (targetGraphic == null) return;
        isEffectActive = false;
        targetGraphic.color = originalColor;
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
        if (!isEffectActive && targetGraphic != null)
            originalColor = targetGraphic.color;
    }
}
