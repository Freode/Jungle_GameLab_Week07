using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 낙타 특별 이벤트를 제어하는 스크립트입니다. (UI 버전)
/// </summary>
public class CamelController : MonoBehaviour, IPointerClickHandler
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 20f; // 낙타가 화면에 머무는 시간
    [SerializeField] private float floatSpeed = 1f; // 위아래로 움직이는 속도
    [SerializeField] private float floatHeight = 10f; // 위아래로 움직이는 높이 (UI 좌표 기준)
    [SerializeField] private float scaleSpeed = 1f; // 크기가 변하는 속도
    [SerializeField] private float scaleAmount = 0.05f; // 크기가 변하는 정도

    private RectTransform rectTransform; // RectTransform 참조
    private Vector2 initialPosition; // 초기 위치 (anchoredPosition)
    private Vector3 initialScale; // 초기 크기

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        initialScale = rectTransform.localScale;

        // 20초 후에 낙타가 스스로 파괴되도록 설정
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // 둥실둥실 뜨는 효과
        HandleFloatingEffect();
        // 크기가 변하는 효과
        HandleScalingEffect();
    }

    /// <summary>
    /// 낙타 UI를 클릭했을 때 호출됩니다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // CamelEventSystem에 보너스 활성화를 요청
        if (CamelEventSystem.instance != null)
        {
            CamelEventSystem.instance.ActivateCamelBonus();
        }
        
        // 보너스 활성화 후 낙타는 즉시 사라짐
        Destroy(gameObject);
    }

    /// <summary>
    /// 낙타가 위아래로 둥실둥실 움직이는 효과를 처리합니다.
    /// </summary>
    private void HandleFloatingEffect()
    {
        float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        rectTransform.anchoredPosition = new Vector2(initialPosition.x, newY);
    }

    /// <summary>
    /// 낙타의 크기가 주기적으로 커졌다 작아지는 효과를 처리합니다.
    /// </summary>
    private void HandleScalingEffect()
    {
        float newScale = 1 + Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        rectTransform.localScale = initialScale * newScale;
    }
}