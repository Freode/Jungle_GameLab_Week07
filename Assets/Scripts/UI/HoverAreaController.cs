using UnityEngine;

public class HoverAreaController : MonoBehaviour
{
    private RectTransform rectTransform;
    public float radius;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Assuming the UI element is a circle, the radius can be derived from its size.
        radius = rectTransform.sizeDelta.x / 2;
    }

    public bool IsPositionInside(Vector3 worldPosition)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        return Vector2.Distance(screenPosition, rectTransform.position) <= radius;
    }
}
