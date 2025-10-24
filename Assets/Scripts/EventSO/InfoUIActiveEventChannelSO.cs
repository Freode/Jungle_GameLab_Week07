using UnityEngine;
using UnityEngine.Events;

// 정보 UI를 띄울 이벤트
[CreateAssetMenu(menuName = "Events/Info UI Active Event Channel")]
public class InfoUIActiveEventChannelSO : ScriptableObject
{
    public UnityAction<string, string, Sprite, Vector3> OnEventRaised;

    public void RaiseEvent(string name, string description, Sprite icon, Vector3 loc)
    {
        OnEventRaised?.Invoke(name, description, icon, loc);
    }
}
