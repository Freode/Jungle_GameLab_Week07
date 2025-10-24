// 파일 이름: HighlightEventChannelSO.cs
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 특정 게임오브젝트의 하이라이트 상태 변경을 방송하는 ScriptableObject 채널입니다.
/// </summary>
[CreateAssetMenu(menuName = "Events/Highlight Event Channel")]
public class HighlightEventChannelSO : ScriptableObject
{
    public UnityAction<GameObject, bool> OnEventRaised;

    /// <summary>
    /// 하이라이트 이벤트를 방송합니다.
    /// </summary>
    /// <param name="target">하이라이트 대상이 될 GameObject</param>
    /// <param name="isHighlighted">하이라이트를 켤 것인지(true) 끌 것인지(false)</param>
    public void RaiseEvent(GameObject target, bool isHighlighted)
    {
        OnEventRaised?.Invoke(target, isHighlighted);
    }
}