using UnityEngine;
using System.Collections.Generic;

public class HoverRewardController : MonoBehaviour
{
    [Header("Hover Settings")] [Tooltip("The radius of the hover effect circle.")]
    public float hoverRadius = 1.5f;

    [Tooltip("The layer mask to filter for citizens.")]
    public LayerMask citizenLayer;

    [Header("Visualizer Settings")] [Tooltip("The LineRenderer to draw the circle.")]
    public LineRenderer circleRenderer;

    [Tooltip("The number of segments to make the circle smooth.")]
    public int segments = 50;

    [Tooltip("The color of the circle.")] public Color circleColor = Color.white;

    [Tooltip("The width of the circle line.")]
    public float circleWidth = 0.1f;

    private List<CitizenHighlighter> lastHoveredCitizens = new List<CitizenHighlighter>();

    void Start()
    {
        if (circleRenderer != null)
        {
            circleRenderer.positionCount = segments + 1;
            circleRenderer.useWorldSpace = true;
            circleRenderer.startColor = circleColor;
            circleRenderer.endColor = circleColor;
            circleRenderer.startWidth = circleWidth;
            circleRenderer.endWidth = circleWidth;
        }
    }

    void Update()
    {
        if (ClickModeManager.Instance.CurrentMode == ClickMode.Heart)
        {
            if (circleRenderer != null)
            {
                circleRenderer.enabled = true;
            }

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (circleRenderer != null)
            {
                DrawCircle(mousePosition);
            }

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(mousePosition, hoverRadius, citizenLayer);

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
                if (!currentHoveredCitizens.Contains(citizen))
                {
                    citizen.SetHovered(false);
                }
            }

            lastHoveredCitizens = currentHoveredCitizens;
        }
        else
        {
            if (circleRenderer != null)
            {
                circleRenderer.enabled = false;
            }

            foreach (var citizen in lastHoveredCitizens)
            {
                citizen.SetHovered(false);
            }

            lastHoveredCitizens.Clear();
        }
    }

    void DrawCircle(Vector2 center)
    {
        if (circleRenderer == null) return;

        float angle = 0f;
        for (int i = 0; i < (segments + 1); i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * hoverRadius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * hoverRadius;

            circleRenderer.SetPosition(i, new Vector3(center.x + x, center.y + y, 0));

            angle += (360f / segments);
        }
    }

    // Public setters for circleWidth and hoverRadius
    public void SetCircleWidth(float width)
    {
        circleWidth = width;
        if (circleRenderer != null)
        {
            circleRenderer.startWidth = circleWidth;
            circleRenderer.endWidth = circleWidth;
        }
    }

    public void SetHoverRadius(float radius)
    {
        hoverRadius = radius;
        // No need to call DrawCircle here, as Update() will call it next frame.
    }
}