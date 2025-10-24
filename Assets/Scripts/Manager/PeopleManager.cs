using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PeopleManager : MonoBehaviour
{
    public static PeopleManager Instance { get; private set; }

    [Header("Event Channels to Listen")]
    public PeopleActorEventChannelSO OnActorDiedChannel;
    public Dictionary<AreaType, bool> checkUnlockStructures;

    // 영역별 인원 목록
    private readonly Dictionary<AreaType, HashSet<PeopleActor>> _areaSets =
        new Dictionary<AreaType, HashSet<PeopleActor>>();

    [SerializeField] AreaAnchor[] AreaAnchors;
    [SerializeField] AreaZone[] AreaZones;

    public event System.Action OnAreaPeopleCountChanged;            // 구역에 있는 사람 변경


    private void OnEnable()
    {
        if (OnActorDiedChannel != null)
        {
            OnActorDiedChannel.OnEventRaised += HandleActorDeath;
        }
    }

    private void OnDisable()
    {
        if (OnActorDiedChannel != null)
        {
            OnActorDiedChannel.OnEventRaised -= HandleActorDeath;
        }
    }

    private void HandleActorDeath(PeopleActor actor)
    {
        if (actor == null) return;
        Unregister(actor);
        ObjectPooler.Instance.ReturnObject(actor.gameObject);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        foreach (AreaType a in System.Enum.GetValues(typeof(AreaType)))
            _areaSets[a] = new HashSet<PeopleActor>();
    }

    // --------- 등록/해제 (오버로드 2종) ---------

    /// <summary>현재 부모 체인의 AreaAnchor 기준으로 등록</summary>
    public void Register(PeopleActor actor)
    {
        if (!actor) return;
        var area = ResolveArea(actor.transform);
        _areaSets[area].Add(actor);
        OnAreaPeopleCountChanged?.Invoke();

        // ★ 재정부에 세금 재계산을 명합니다.
        GameManager.instance.RecalculateAllIncomes();
    }

    /// <summary>명시적 AreaType으로 등록 (부모 체인 무시)</summary>
    public void Register(AreaType area, PeopleActor actor)
    {
        if (!actor) return;
        _areaSets[area].Add(actor);
        OnAreaPeopleCountChanged?.Invoke();
        // ★ 재정부에 세금 재계산을 명합니다.
        GameManager.instance.RecalculateAllIncomes();
    }

    /// <summary>어느 영역에 있든 안전하게 해제</summary>
    public void Unregister(PeopleActor actor)
    {
        if (!actor) return;
        // 혹시 캐시가 없다면 전 영역에서 제거 (HashSet이면 비용 작음)
        foreach (var set in _areaSets.Values)
            set.Remove(actor);
        OnAreaPeopleCountChanged?.Invoke();
        GameManager.instance.RecalculateAllIncomes();
    }

    /// <summary>부모 변경 등으로 영역이 바뀐 경우 호출</summary>
    public void NotifyAreaChanged(PeopleActor actor)
    {
        if (!actor) return;
        Unregister(actor);
        Register(actor); // 새 부모 기준으로 재등록
    }

    public void SetParentToNewAnchor(GameObject obj, AreaType newType)
    {
        for (int i = 0; i < AreaAnchors.Length; i++)
        {
            if (AreaAnchors[i].area == newType)
            {
                obj.transform.SetParent(AreaAnchors[i].transform, true);
                return;
            }
        }
    }

    public void MoveToArea(GameObject obj, AreaType newArea, JobType _job)
    {
        AreaZone areaZone = null;
        for (int i = 0; i < AreaZones.Length; i++)
        {
            
            if (AreaZones[i].areaType == newArea)
            {
                
                areaZone = AreaZones[i];
                break;
            }
        }
        var actor = obj.GetComponent<PeopleActor>();
        var mover = obj.GetComponent<Mover>();
        if (!actor || !mover || !areaZone) return;
        actor.ApplyJop(_job);
        mover.LockToArea(areaZone);
        SetParentToNewAnchor(obj, newArea);
        NotifyAreaChanged(actor);
    }

    public void SetAreaLock(GameObject obj, AreaType newArea)
    {
        AreaZone areaZone = null;
        for (int i = 0; i < AreaZones.Length; i++)
        {

            if (AreaZones[i].areaType == newArea)
            {

                areaZone = AreaZones[i];
                break;
            }
        }
        var actor = obj.GetComponent<PeopleActor>();
        var mover = obj.GetComponent<Mover>();
        if (!actor || !mover || !areaZone) return;
        mover.LockToArea(areaZone);
    }

    public void CheckUnlockArea()
    {
        if (checkUnlockStructures == null)
        {
            checkUnlockStructures = GameManager.instance.GetCheckUnlockStructures();
        }
    }

    public GameObject SelectOnePerson(AreaType type)
    {
        if (!_areaSets.ContainsKey(type)) return null;
        if (Count(type) == 0) return null;

        var set = _areaSets[type];
        foreach (var person in set)
            return person.gameObject;
        return null;
    }

    public void DespawnPerson(GameObject obj)
    {
        var actor = obj.GetComponent<PeopleActor>();
        if (!actor) return;
        Unregister(actor);
        
        ObjectPooler.Instance.ReturnObject(obj);
    }

    /// <summary>영역 수</summary>
    public int Count(AreaType area) => _areaSets[area].Count;

    /// <summary>영역 목록(읽기용)</summary>
    public IReadOnlyCollection<PeopleActor> GetPeople(AreaType area) => _areaSets[area];

    

    // --------- 내부 유틸 ---------

    private static AreaType ResolveArea(Transform t)
    {
        if (!t) return AreaType.Normal;
        var anchor = t.GetComponentInParent<AreaAnchor>(true);
        return anchor ? anchor.area : AreaType.Normal;
    }


    

    // ----------------------------------------
    [ContextMenu("Print All Areas")]
    public void PrintAllAreas()
    {
        foreach (var kvp in _areaSets)
        {
            Debug.Log($"Area: {kvp.Key}, Count: {kvp.Value.Count}");
            foreach (var actor in kvp.Value)
                Debug.Log($"  - {actor.name}");
        }
    }
}
