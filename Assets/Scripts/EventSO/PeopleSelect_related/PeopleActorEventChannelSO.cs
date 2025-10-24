// 파일 이름: PeopleActorEventChannelSO.cs
using UnityEngine;
using UnityEngine.Events;

// 유니티 에디터의 Create 메뉴에 이 에셋을 만들 수 있는 옵션을 추가합니다.
[CreateAssetMenu(menuName = "Events/PeopleActor Event Channel")]
public class PeopleActorEventChannelSO : ScriptableObject
{
    // PeopleActor 타입의 데이터를 함께 전달할 구독자 명단입니다.
    public UnityAction<PeopleActor> OnEventRaised;

    // 방송국에서 이 함수를 호출하며, 선택된 actor 정보를 인자로 넘겨줍니다.
    public void RaiseEvent(PeopleActor actor)
    {
        // 구독자 명단에 등록된 모든 함수에 actor 정보를 전달하며 실행시킵니다.
        OnEventRaised?.Invoke(actor);
    }
}