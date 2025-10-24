using UnityEngine;
using UnityEngine.UI; // UI 관련 클래스를 사용하기 위해 필요합니다.

// 이 스크립트는 Image 컴포넌트가 있는 게임 오브젝트에만 추가할 수 있도록 강제합니다.
[RequireComponent(typeof(Image))]
public class RainbowHammer : MonoBehaviour
{
    [Tooltip("색상이 다음 색상으로 완전히 변경되는 데 걸리는 시간 (초 단위)")]
    public float transitionDuration = 0.1f;

    private Image buttonImage;
    private Color startColor;
    private Color targetColor;
    private float lerpProgress;

    // 스크립트 인스턴스가 로드될 때 호출됩니다.
    void Awake()
    {
        buttonImage = GetComponent<Image>();
        // 시작 색상과 목표 색상을 랜덤으로 초기화합니다.
        startColor = GenerateRandomBrightColor();
        targetColor = GenerateRandomBrightColor();
        lerpProgress = 0f;
    }

    // 매 프레임마다 호출됩니다.
    void Update()
    {
        // 전환 진행률을 시간에 따라 증가시킵니다.
        lerpProgress += Time.deltaTime / transitionDuration;

        // 시작 색상과 목표 색상 사이를 부드럽게 보간(Lerp)하여 색상을 적용합니다.
        // Mathf.Clamp01은 진행률 값이 0과 1 사이를 벗어나지 않도록 보장합니다.
        buttonImage.color = Color.Lerp(startColor, targetColor, Mathf.Clamp01(lerpProgress));

        // 전환이 완료되면 (진행률이 1 이상이 되면)
        if (lerpProgress >= 1.0f)
        {
            // 진행률을 리셋합니다. (정확한 타이밍을 위해 나머지 값을 유지)
            lerpProgress %= 1.0f;

            // 현재 목표 색상이 다음 전환의 시작 색상이 됩니다.
            startColor = targetColor;

            // 새로운 목표 색상을 랜덤으로 다시 정합니다.
            targetColor = GenerateRandomBrightColor();
        }
    }

    /// <summary>
    /// 밝고 선명한 랜덤 색상을 생성하는 도우미 함수입니다.
    /// </summary>
    private Color GenerateRandomBrightColor()
    {
        // HSV 색상 모델을 사용하여 항상 밝고 선명한 색상을 보장합니다.
        return Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
    }
}