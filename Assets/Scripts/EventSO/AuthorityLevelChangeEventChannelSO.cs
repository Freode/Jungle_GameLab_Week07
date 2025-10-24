using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 권위 레벨(int)과 현재 색상(Color)의 변경을 방송하는 ScriptableObject 채널입니다.
/// </summary>
[CreateAssetMenu(menuName = "Events/Authority Level Change Event Channel")]
public class AuthorityLevelChangeEventChannelSO : ScriptableObject
{
    public UnityAction<int, Color> OnEventRaised;

    public void RaiseEvent(int level, Color color)
    {
        OnEventRaised?.Invoke(level, color);
    }
}