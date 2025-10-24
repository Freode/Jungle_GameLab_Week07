// 파일 이름: NameTagEventChannelSO.cs
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 특정 백성(GameObject)의 이름표를 켜고(true) 끌지(false)를 방송하는 ScriptableObject 채널입니다.
/// </summary>
[CreateAssetMenu(menuName = "Events/Name Tag Event Channel")]
public class NameTagEventChannelSO : ScriptableObject
{
    public UnityAction<GameObject, bool> OnEventRaised;

    public void RaiseEvent(GameObject targetCitizen, bool shouldShow)
    {
        OnEventRaised?.Invoke(targetCitizen, shouldShow);
    }
}