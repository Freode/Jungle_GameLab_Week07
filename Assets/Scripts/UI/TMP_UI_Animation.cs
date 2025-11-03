
using UnityEngine;
using TMPro;

/// <summary>
/// TextMeshPro UI 오브젝트의 크기와 회전을 시간에 따라 부드럽게 변경하여
/// 살아 움직이는 듯한 효과를 주는 스크립트입니다.
/// </summary>
public class TMP_UI_Animation : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("크기 변경 속도")]
    public float scaleSpeed = 2.0f;

    [Tooltip("최대 확대/축소 비율")]
    public float scaleAmount = 0.1f;

    [Tooltip("회전 속도")]
    public float rotationSpeed = 3.0f;

    [Tooltip("최대 회전 각도")]
    public float rotationAmount = 10.0f;

    private RectTransform rectTransform;
    private Vector3 initialScale;

    /// <summary>
    /// 스크립트가 시작될 때 초기값을 설정합니다.
    /// </summary>
    void Start()
    {
        // RectTransform 컴포넌트를 가져옵니다. UI 오브젝트의 위치, 크기, 회전을 제어합니다.
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("TMP_UI_Animation: RectTransform 컴포넌트를 찾을 수 없습니다. UI 오브젝트에 추가해주세요.");
            enabled = false; // 스크립트 비활성화
            return;
        }

        // 초기 크기 값을 저장합니다.
        initialScale = rectTransform.localScale;
    }

    /// <summary>
    /// 매 프레임마다 호출되어 애니메이션 효과를 적용합니다.
    /// </summary>
    void Update()
    {
        // 1. 크기 변경 (Scaling)
        // Mathf.Sin 함수를 사용하여 -1과 1 사이를 부드럽게 반복하는 값을 만듭니다.
        // 시간에 scaleSpeed를 곱하여 속도를 조절합니다.
        float scaleFactor = 1.0f + Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        rectTransform.localScale = initialScale * scaleFactor;

        // 2. 회전 (Rotation)
        // 크기 변경과 마찬가지로 Sin 함수를 사용하여 좌우로 부드럽게 기울어지는 값을 만듭니다.
        float rotationAngle = Mathf.Sin(Time.time * rotationSpeed) * rotationAmount;
        // Z축을 기준으로 회전하는 Quaternion 값을 만듭니다.
        rectTransform.localRotation = Quaternion.Euler(0, 0, rotationAngle);
    }
}
