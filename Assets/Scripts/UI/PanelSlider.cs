using UnityEngine;

public class PanelSlider : MonoBehaviour
{
    [Header("Panel References")]
    public RectTransform targetPanel;

    [Header("Slide Settings")]
    [Tooltip("마우스가 화면 오른쪽 가장자리에서 이 거리 안에 들어오면 패널이 슬라이드됩니다.")]
    public float triggerZoneWidth = 100f; // 픽셀 단위
    [Tooltip("패널이 숨겨져 있을 때의 X 위치입니다.")]
    public float initialXPosition = 1175f;
    [Tooltip("패널이 숨겨져 있을 때의 Y 위치입니다.")]
    public float initialYPosition = 0f;
    [Tooltip("패널이 보여질 때의 X 위치입니다.")]
    public float targetXPosition = 750f;
    [Tooltip("패널이 보여질 때의 Y 위치입니다.")]
    public float targetYPosition = 0f;
    [Tooltip("패널이 슬라이드하는 속도입니다.")]
    public float slideSpeed = 5f;

    [Header("Trigger Zones (Screen Coordinates)")]
    [Tooltip("패널을 열기 위한 초기 트리거 영역 (화면 좌표)")]
    public Rect initialTriggerRect = new Rect(0, 0, 100, 1080); // 예시: 왼쪽 100px, 전체 높이
    [Tooltip("패널이 한번 열린 후, 열린 상태를 유지하기 위한 확장 트리거 영역 (화면 좌표)")]
    public Rect openTriggerRect = new Rect(0, 0, 500, 1080); // 예시: 왼쪽 500px, 전체 높이

    private bool isPanelOpen = false;

    private void Awake()
    {
        if (targetPanel == null)
        {
            targetPanel = GetComponent<RectTransform>();
        }
        // 초기 위치 설정
        Vector3 currentPos = targetPanel.anchoredPosition;
        targetPanel.anchoredPosition = new Vector3(initialXPosition, initialYPosition, currentPos.z);
    }

    private void Update()
    {
        if (targetPanel == null) return;

        Vector3 mousePosition = Input.mousePosition;

        Vector3 currentAnchoredPos = targetPanel.anchoredPosition;
        float targetX;

        // 마우스가 초기 트리거 영역에 있는지 확인
        bool isMouseInInitialTriggerZone = initialTriggerRect.Contains(mousePosition);
        // 마우스가 열린 상태 유지 영역에 있는지 확인
        bool isMouseInOpenTriggerZone = openTriggerRect.Contains(mousePosition);
        // 마우스가 패널 위에 있는지 확인
        bool isMouseOverPanel = RectTransformUtility.RectangleContainsScreenPoint(targetPanel, Input.mousePosition, Camera.main);

        // 패널 열림 상태 로직
        if (isMouseInInitialTriggerZone || isMouseOverPanel)
        {
            isPanelOpen = true;
        }
        else if (isPanelOpen && !isMouseInOpenTriggerZone && !isMouseOverPanel)
        {
            isPanelOpen = false;
        }

        // 목표 X 위치 결정
        float targetY;
        if (isPanelOpen)
        {
            targetX = targetXPosition;
            targetY = targetYPosition;
        }
        else
        {
            targetX = initialXPosition;
            targetY = initialYPosition;
        }

        // 패널을 목표 X, Y 위치로 부드럽게 이동
        targetPanel.anchoredPosition = Vector3.Lerp(
            currentAnchoredPos,
            new Vector3(targetX, targetY, currentAnchoredPos.z),
            Time.deltaTime * slideSpeed
        );
    }
}
