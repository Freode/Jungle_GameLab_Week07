using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TechEachButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action OnClickStart;
    public event Action OnClickEnd;

    // 마우스 클릭 중
    public void OnPointerDown(PointerEventData eventData)
    {
        OnClickStart?.Invoke();
    }

    // 마우스 클릭 해제
    public void OnPointerUp(PointerEventData eventData)
    {
        OnClickEnd?.Invoke();
    }
}
