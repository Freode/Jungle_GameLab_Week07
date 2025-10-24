// 파일 이름: AgeingSystem.cs
using UnityEngine;

[RequireComponent(typeof(PeopleActor))]
public class AgeingSystem : MonoBehaviour
{
    public VoidEventChannelSO OnYearPassedChannel;
    public OutFloatEventChannelSO OnGetAdditionLifeRateChannel;               // 추가 생존 확률을 가져오는 채널

    public SurvivalProfile survivalProfile;
    private PeopleActor owner;

    void Awake()
    {
        owner = GetComponent<PeopleActor>();
    }

    private void OnEnable()
    {
        OnYearPassedChannel.OnEventRaised += GetOlder;
    }

    private void OnDisable()
    {
        OnYearPassedChannel.OnEventRaised -= GetOlder;
    }

    private void GetOlder()
    {
        owner.AddAge(1);
        // ★ 운명의 심판 시작
        if (survivalProfile != null)
        {
            float survivalChance = survivalProfile.GetSurvivalChance(owner.Age) + OnGetAdditionLifeRateChannel.RaiseEvent();

            // 주사위를 굴려 생존 확률보다 높게 나오면 (불운하면)
            if (Random.value > survivalChance)
            {
                // PeopleActor에게 죽음을 명함
                // owner.Die();
            }
        }
    }
}