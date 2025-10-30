using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class TabPositionController : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool isOut = false;
    
    [SerializeField] private float slideAmount = 20f;  // 얼마나 왼쪽으로 이동할지 (픽셀)

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void TogglePosition()
    {
        if (isOut)
        {
            // 원래 위치로
            rectTransform.anchoredPosition = originalPosition;
        }
        else
        {
            // 왼쪽으로 이동
            rectTransform.anchoredPosition = originalPosition + new Vector2(-slideAmount, 0);
        }
        isOut = !isOut;
    }

    public void ResetPosition()
    {
        if (isOut)
        {
            rectTransform.anchoredPosition = originalPosition;
            isOut = false;
        }
    }

    public void SlideOut()
    {
        if (!isOut)
        {
            rectTransform.anchoredPosition = originalPosition + new Vector2(-slideAmount, 0);
            isOut = true;
        }
    }
}