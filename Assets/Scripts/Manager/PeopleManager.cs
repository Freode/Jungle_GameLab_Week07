using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PeopleManager : MonoBehaviour
{
    public static PeopleManager Instance { get; private set; }

    [Header("Event Channels to Listen")]
    public PeopleActorEventChannelSO OnActorDiedChannel;
    public Dictionary<AreaType, bool> checkUnlockStructures;

    // 영역별 인원 목록 + 가중치(Weight)
    // 각 PeopleActor는 기본적으로 weight=1을 가지며, 집계(merge)된 큰 일꾼은 weight>1로 처리됩니다.
    private readonly Dictionary<AreaType, Dictionary<PeopleActor, int>> _areaMaps =
        new Dictionary<AreaType, Dictionary<PeopleActor, int>>();

    // 특수 캐릭터 객체
    private Dictionary<JobType, GameObject> _specialCharacters = new Dictionary<JobType, GameObject>();

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
            _areaMaps[a] = new Dictionary<PeopleActor, int>();
    }

    // --------- 등록/해제 (오버로드 2종) ---------

    /// <summary>현재 부모 체인의 AreaAnchor 기준으로 등록</summary>
    public void Register(PeopleActor actor)
    {
        if (!actor) return;
        var area = ResolveArea(actor.transform);
        _areaMaps[area][actor] = 1; // 기본 가중치 1
        OnAreaPeopleCountChanged?.Invoke();

        // ★ 재정부에 세금 재계산을 명합니다.
        GameManager.instance.RecalculateAllIncomes();
    }

    /// <summary>명시적 AreaType으로 등록 (부모 체인 무시)</summary>
    public void Register(AreaType area, PeopleActor actor)
    {
        if (!actor) return;
        _areaMaps[area][actor] = 1; // 기본 가중치 1
        OnAreaPeopleCountChanged?.Invoke();
        // ★ 재정부에 세금 재계산을 명합니다.
        GameManager.instance.RecalculateAllIncomes();
    }

    /// <summary>어느 영역에 있든 안전하게 해제</summary>
    public void Unregister(PeopleActor actor)
    {
        if (!actor) return;
        // 전 영역에서 제거
        foreach (var map in _areaMaps.Values)
        {
            if (map.ContainsKey(actor)) { map.Remove(actor); }
        }
        OnAreaPeopleCountChanged?.Invoke();
        GameManager.instance.RecalculateAllIncomes();
    }

    /// <summary>부모 변경 등으로 영역이 바뀐 경우 호출</summary>
    public void NotifyAreaChanged(PeopleActor actor)
    {
        if (!actor) return;
        if (actor.GetSpeicalCharacter()) return;
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

        // CatGodMover를 위한 특별 처리
        var catGodMover = obj.GetComponent<CatGodMover>();
        if (catGodMover != null)
        {
            if (areaZone != null)
            {
                catGodMover.lockedArea = areaZone;
            }
            return; // CatGodMover는 PeopleActor나 Mover의 다른 로직을 따르지 않음
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
        if (!_areaMaps.ContainsKey(type)) return null;
        if (RawCount(type) == 0) return null;

        var map = _areaMaps[type];
        foreach (var kv in map)
            return kv.Key.gameObject;
        return null;
    }

    public void DespawnPerson(GameObject obj)
    {
        var actor = obj.GetComponent<PeopleActor>();
        if (!actor) return;
        Unregister(actor);
        
        ObjectPooler.Instance.ReturnObject(obj);
    }

    /// <summary>새로운 사람을 소환합니다.</summary>
    public GameObject SpawnNewPerson()
    {
        // ObjectPooler를 통해 People 타입의 오브젝트를 소환
        GameObject newPerson = ObjectPooler.Instance.SpawnObject(ObjectType.People);
        
        if (newPerson == null)
        {
            Debug.LogError("새로운 사람 소환에 실패했습니다. ObjectPooler에 People 타입이 등록되어 있는지 확인하세요.");
            return null;
        }

        // PeopleActor 컴포넌트 확인
        PeopleActor actor = newPerson.GetComponent<PeopleActor>();
        if (actor == null)
        {
            Debug.LogError("소환된 오브젝트에 PeopleActor 컴포넌트가 없습니다.");
            ObjectPooler.Instance.ReturnObject(newPerson);
            return null;
        }

        // 기본적으로 Normal 영역에 등록
        Register(AreaType.Normal, actor);
        
        Debug.Log($"새로운 사람이 소환되었습니다: {newPerson.name}");
        return newPerson;
    }

    /// <summary>가중치 합산으로 계산한 '실효 인원 수'를 반환합니다. (생산/수입 계산에 사용)</summary>
    public int Count(AreaType area)
    {
        int sum = 0;
        var map = _areaMaps[area];
        foreach (var kv in map)
            sum += Mathf.Max(1, kv.Value);
        return sum;
    }

    /// <summary>실제 액터 개수(오브젝트 수)를 반환합니다. (순수 headcount)</summary>
    public int RawCount(AreaType area) => _areaMaps[area].Count;

    /// <summary>영역 목록(읽기용)</summary>
    public IReadOnlyCollection<PeopleActor> GetPeople(AreaType area) => _areaMaps[area].Keys as IReadOnlyCollection<PeopleActor> ?? new List<PeopleActor>(_areaMaps[area].Keys);

    /// <summary>특정 액터의 가중치를 조회 (기본 1)</summary>
    public int GetActorWeight(PeopleActor actor)
    {
        if (!actor) return 1;
        foreach (var kv in _areaMaps)
        {
            if (kv.Value.TryGetValue(actor, out int w)) return Mathf.Max(1, w);
        }
        return 1;
    }

    /// <summary>특정 액터의 가중치를 설정하고, 수입을 재계산합니다.</summary>
    public void SetActorWeight(PeopleActor actor, int weight)
    {
        if (!actor) return;
        foreach (var kv in _areaMaps)
        {
            if (kv.Value.ContainsKey(actor))
            {
                kv.Value[actor] = Mathf.Max(1, weight);
                OnAreaPeopleCountChanged?.Invoke();
                GameManager.instance.RecalculateAllIncomes();
                return;
            }
        }
    }

    // 특수 캐릭터 생성
    public GameObject SpawnSpecialCharacter(JobType jobType)
    {
        if(_specialCharacters.ContainsKey(jobType))
            return _specialCharacters[jobType];

        _specialCharacters[jobType] = ObjectPooler.Instance.SpawnObject(ObjectType.People);
        
        // 특수 캐릭터로 설정
        if(_specialCharacters[jobType].TryGetComponent(out PeopleActor actor))
            actor.SetSpecialCharacter();
        
        return _specialCharacters[jobType];
    }
    

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
        foreach (var kvp in _areaMaps)
        {
            int raw = kvp.Value.Count;
            int eff = 0;
            foreach (var w in kvp.Value.Values) eff += Mathf.Max(1, w);
            Debug.Log($"Area: {kvp.Key}, Raw: {raw}, Effective: {eff}");
            foreach (var pair in kvp.Value)
                Debug.Log($"  - {pair.Key.name} (w={pair.Value})");
        }
    }
}
