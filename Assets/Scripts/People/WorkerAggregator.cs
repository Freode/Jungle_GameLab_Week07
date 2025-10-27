using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무직(Normal)을 제외한 각 구역에서,
/// - 원시 인원수(실제 액터 수)가 threshold(기본 100) 이상이면
/// - 100명 당 1명의 '큰 일꾼(가중치 100)' 으로 자동 합칩니다.
/// 시각적 효과로는 scale을 키우고, 생산/수입은 PeopleManager의 가중치 합으로 동일하게 유지합니다.
/// </summary>
[DisallowMultipleComponent]
public class WorkerAggregator : MonoBehaviour
{
    [Header("Aggregation Rule")]
    [SerializeField, Min(2)] private int threshold = 100;
    [SerializeField, Min(1)] private int weightPerBigUnit = 100;
    [SerializeField] private float bigScale = 1.8f;
    [SerializeField] private bool useGlobalAcrossAllAreas = false;
    [SerializeField] private bool debugLogs = false;

    [Header("Areas")]
    [Tooltip("집계를 제외할 구역(보통 무직)")]
    [SerializeField] private List<AreaType> excludeAreas = new List<AreaType> { AreaType.Normal };

    private bool _isAggregating = false;

    private bool _subscribed = false;

    private void OnEnable()
    {
        EnsureSubscribed();
    }

    private void OnDisable()
    {
        if (_subscribed && PeopleManager.Instance != null)
        {
            PeopleManager.Instance.OnAreaPeopleCountChanged -= OnPeopleChanged;
        }
        _subscribed = false;
    }

    private void Start()
    {
        EnsureSubscribed();
        // 초기 한 번 실행해 정리
        AggregateAllAreas();
    }

    private void Update()
    {
        // 씬 로딩 순서로 인해 OnEnable 시점에 PeopleManager가 아직 없었다면, 런타임에 재시도
        if (!_subscribed)
            EnsureSubscribed();
    }

    private void EnsureSubscribed()
    {
        if (PeopleManager.Instance == null) return;
        if (_subscribed) return;
        PeopleManager.Instance.OnAreaPeopleCountChanged += OnPeopleChanged;
        _subscribed = true;
        if (debugLogs) Debug.Log("[WorkerAggregator] Subscribed to OnAreaPeopleCountChanged");
    }

    private void OnPeopleChanged()
    {
        if (_isAggregating) return; // 재진입 방지
        AggregateAllAreas();
    }

    private void AggregateAllAreas()
    {
        _isAggregating = true;
        if (useGlobalAcrossAllAreas)
        {
            TryAggregateGlobal();
        }
        else
        {
            foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
            {
                if (excludeAreas.Contains(area)) continue;
                TryAggregateArea(area);
            }
        }
        _isAggregating = false;
    }

    private void TryAggregateArea(AreaType area)
    {
        // 원시 액터 목록(키) 획득
        var actors = PeopleManager.Instance.GetPeople(area);
        if (actors == null) return;

        // weight==1(작은 일꾼)만 집계 대상으로 수집
        var smalls = new List<PeopleActor>();
        foreach (var a in actors)
        {
            if (!a) continue;
            if (PeopleManager.Instance.GetActorWeight(a) == 1)
                smalls.Add(a);
        }

    int rawSmallCount = smalls.Count;
    if (debugLogs) Debug.Log($"[WorkerAggregator] Area={area} smallCount={rawSmallCount}, threshold={threshold}");
    if (rawSmallCount < threshold) return;

        // 100명 단위로 집계 (가중치 weightPerBigUnit)
    int groups = rawSmallCount / threshold;
        int toConvert = groups * threshold; // 실제 변환될 수
    if (debugLogs) Debug.Log($"[WorkerAggregator] Area={area} groups={groups}, toConvert={toConvert}");

        int idx = 0;
        for (int g = 0; g < groups; g++)
        {
            // 남길 1명
            var keeper = smalls[idx++];
            if (!keeper) continue;

            // 나머지 threshold-1 명 제거
            for (int i = 1; i < threshold; i++)
            {
                if (idx >= smalls.Count) break;
                var victim = smalls[idx++];
                if (!victim) continue;
                PeopleManager.Instance.DespawnPerson(victim.gameObject);
            }

            // 남긴 1명은 '큰 일꾼'으로 승격: 스케일+가중치 설정
            PromoteToBigUnit(keeper);
        }
    }

    private void TryAggregateGlobal()
    {
        var smalls = new List<PeopleActor>();
        foreach (AreaType area in System.Enum.GetValues(typeof(AreaType)))
        {
            if (excludeAreas.Contains(area)) continue;
            var actors = PeopleManager.Instance.GetPeople(area);
            if (actors == null) continue;
            foreach (var a in actors)
            {
                if (!a) continue;
                if (PeopleManager.Instance.GetActorWeight(a) == 1)
                    smalls.Add(a);
            }
        }

        int rawSmallCount = smalls.Count;
        if (debugLogs) Debug.Log($"[WorkerAggregator] Global smallCount={rawSmallCount}, threshold={threshold}");
        if (rawSmallCount < threshold) return;

        int groups = rawSmallCount / threshold;
        if (debugLogs) Debug.Log($"[WorkerAggregator] Global groups={groups}");
        int idx = 0;
        for (int g = 0; g < groups; g++)
        {
            var keeper = smalls[idx++];
            if (!keeper) continue;

            for (int i = 1; i < threshold; i++)
            {
                if (idx >= smalls.Count) break;
                var victim = smalls[idx++];
                if (!victim) continue;
                PeopleManager.Instance.DespawnPerson(victim.gameObject);
            }

            PromoteToBigUnit(keeper);
        }
    }

    private void PromoteToBigUnit(PeopleActor actor)
    {
        if (!actor) return;

        // 가중치 설정 (생산량 유지)
        PeopleManager.Instance.SetActorWeight(actor, weightPerBigUnit);

        // 무적 설정
        actor.SetImmortal();

        // 시각적 확대 (로컬 스케일 기준)
        var t = actor.transform;
        t.localScale = t.localScale * bigScale;
        if (debugLogs) Debug.Log($"[WorkerAggregator] Promoted {actor.name} to big unit (w={weightPerBigUnit})");
    }
}
