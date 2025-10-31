using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 특정 영역 내의 시민들에게 보상(징수) 절차를 트리거하는 범용 컨트롤러입니다.
/// 인스펙터에서 영역의 형태(사각형 또는 원)와 채우기 색상을 선택할 수 있습니다.
/// </summary>
public class AreaRewardController : MonoBehaviour
{
    // 영역 형태를 선택하기 위한 열거형
    public enum AreaShape { Rectangle, Circle }

    [Header("Area Settings")]
    [Tooltip("감지 영역의 형태 (사각형 또는 원)")]
    public AreaShape shape = AreaShape.Rectangle;

    [Tooltip("사각형의 너비 (Rectangle 모드 전용)")]
    public float areaWidth = 10f;

    [Tooltip("사각형의 높이 (Rectangle 모드 전용)")]
    public float areaHeight = 5f;

    [Tooltip("원의 반지름 (Circle 모드 전용)")]
    public float areaRadius = 5f;

    [Tooltip("감지할 시민의 레이어 마스크")]
    public LayerMask citizenLayer;

    [Header("Visualizer Settings")]
    [Tooltip("영역의 테두리를 그릴 LineRenderer")]
    public LineRenderer areaRenderer;

    [Tooltip("영역 내부를 채울 SpriteRenderer")]
    public SpriteRenderer fillRenderer;

    [Tooltip("테두리 색상")]
    public Color areaColor = Color.yellow;

    [Tooltip("채우기 색상 (알파값으로 투명도 조절)")]
    public Color fillColor = new Color(1.0f, 1.0f, 0.0f, 0.2f);

    [Tooltip("라인의 너비")]
    public float lineWidth = 0.1f;

    [Tooltip("원을 그릴 때 사용할 선분의 수 (Circle 모드 전용)")]
    public int circleSegments = 50;

    private List<CitizenHighlighter> lastHoveredCitizens = new List<CitizenHighlighter>();

    void Start()
    {
        if (areaRenderer != null)
        {
            areaRenderer.useWorldSpace = true;
            areaRenderer.startColor = areaColor;
            areaRenderer.endColor = areaColor;
            areaRenderer.startWidth = lineWidth;
            areaRenderer.endWidth = lineWidth;
        }

        if (fillRenderer != null)
        {
            fillRenderer.transform.localPosition = Vector3.zero;
        }
    }

    void Update()
    {
        Vector2 center = transform.position;
        UpdateAreaVisuals(center);

        Collider2D[] hitColliders = DetectCitizens(center);

        List<CitizenHighlighter> currentHoveredCitizens = new List<CitizenHighlighter>();

        foreach (var hitCollider in hitColliders)
        {
            CitizenHighlighter highlighter = hitCollider.GetComponent<CitizenHighlighter>();
            if (highlighter != null)
            {
                currentHoveredCitizens.Add(highlighter);
                highlighter.TriggerReward();
                highlighter.SetHovered(true);
            }
        }

        foreach (var citizen in lastHoveredCitizens)
        {
            if (citizen != null && !currentHoveredCitizens.Contains(citizen))
            {
                citizen.SetHovered(false);
            }
        }

        lastHoveredCitizens = currentHoveredCitizens;
    }

    private Collider2D[] DetectCitizens(Vector2 center)
    {
        switch (shape)
        {
            case AreaShape.Rectangle:
                return Physics2D.OverlapBoxAll(center, new Vector2(areaWidth, areaHeight), 0, citizenLayer);
            case AreaShape.Circle:
                return Physics2D.OverlapCircleAll(center, areaRadius, citizenLayer);
            default:
                return new Collider2D[0];
        }
    }

    private void UpdateAreaVisuals(Vector2 center)
    {
        if (areaRenderer != null)
        {
            areaRenderer.enabled = true;
            switch (shape)
            {
                case AreaShape.Rectangle:
                    DrawRectangle(center);
                    break;
                case AreaShape.Circle:
                    DrawCircle(center);
                    break;
            }
        }

        if (fillRenderer != null)
        {
            fillRenderer.enabled = true;
            // 재질의 색상을 직접 변경해야 커스텀 쉐이더의 _Color 프로퍼티와 상호작용합니다.
            fillRenderer.material.color = fillColor;
            switch (shape)
            {
                case AreaShape.Rectangle:
                    fillRenderer.transform.localScale = new Vector3(areaWidth * 0.56f, areaHeight * 0.56f, 1);
                    break;
                case AreaShape.Circle:
                    fillRenderer.transform.localScale = new Vector3(areaRadius * 2, areaRadius * 2, 1);
                    break;
            }
        }
    }

    void DrawRectangle(Vector2 center)
    {
        areaRenderer.positionCount = 4;
        areaRenderer.loop = true;

        float halfWidth = areaWidth / 2;
        float halfHeight = areaHeight / 2;

        areaRenderer.SetPosition(0, new Vector3(center.x - halfWidth, center.y + halfHeight, 0));
        areaRenderer.SetPosition(1, new Vector3(center.x + halfWidth, center.y + halfHeight, 0));
        areaRenderer.SetPosition(2, new Vector3(center.x + halfWidth, center.y - halfHeight, 0));
        areaRenderer.SetPosition(3, new Vector3(center.x - halfWidth, center.y - halfHeight, 0));
    }

    void DrawCircle(Vector2 center)
    {
        areaRenderer.positionCount = circleSegments + 1;
        areaRenderer.loop = false;

        float angle = 0f;
        for (int i = 0; i < (circleSegments + 1); i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * areaRadius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * areaRadius;

            areaRenderer.SetPosition(i, new Vector3(center.x + x, center.y + y, 0));

            angle += (360f / circleSegments);
        }
    }

    void OnDisable()
    {
        if (areaRenderer != null)
        {
            areaRenderer.enabled = false;
        }
        if (fillRenderer != null)
        {
            fillRenderer.enabled = false;
        }

        foreach (var citizen in lastHoveredCitizens)
        {
            if (citizen != null)
            {
                citizen.SetHovered(false);
            }
        }
        lastHoveredCitizens.Clear();
    }
}
