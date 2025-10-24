// 파일 이름: VoidEventChannelSO.cs
using UnityEngine;
using UnityEngine.Events;

// 유니티 에디터의 Create 메뉴에 이 에셋을 만들 수 있는 옵션을 추가합니다.
// menuName을 "Events/Void Event Channel"로 지정했기 때문에
// Create 메뉴 아래에 Events 폴더가 생기고 그 안에 Void Event Channel이 보입니다.
[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannelSO : ScriptableObject
{
    // 데이터 없이 신호만 주고받을 것이므로, 매개변수가 없는 UnityAction을 사용합니다.
    public UnityAction OnEventRaised;

    // 방송국에서 이 함수를 호출하여 "방송 시작!" 신호를 보냅니다.
    public void RaiseEvent()
    {
        // 구독자가 한 명이라도 있으면(null이 아니면), 명단에 있는 모든 함수를 실행시킵니다.
        OnEventRaised?.Invoke();
    }
}