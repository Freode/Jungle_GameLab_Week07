using UnityEngine;

public class PopulationManager : MonoBehaviour // 간단히 구현 (싱글톤 등은 필요시 추가)
{
    public PeopleActorEventChannelSO OnActorDiedChannel;

    private void OnEnable()
    {
        OnActorDiedChannel.OnEventRaised += DespawnPerson;
    }

    private void OnDisable()
    {
        OnActorDiedChannel.OnEventRaised -= DespawnPerson;
    }

    // 폐하께서 주신 Despawn 로직을 여기에 적용
    public void DespawnPerson(PeopleActor actor)
    {
        if (actor == null) return;
        
        // Unregister(actor); // 이 부분은 실제 PopulationManager의 목록 관리 로직에 맞게 수정 필요
        Debug.Log($"{actor.DisplayName}의 영혼을 거두었습니다...");
        
        // ObjectPooler가 있다면 아래 코드를 사용
        ObjectPooler.Instance.ReturnObject(actor.gameObject);

        // 없다면 간단히 파괴
        //Destroy(actor.gameObject);
    }
}