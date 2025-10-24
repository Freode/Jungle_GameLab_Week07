using UnityEngine;

[DisallowMultipleComponent]
public class PitHouse : MonoBehaviour
{
    [Header("Spawn Pipeline")]
    [SerializeField] private PeopleSpawner spawner;     // PeopleSpawner 레퍼런스

    [Header("Placement")]
    [SerializeField] private Transform spawnPoint;      // 스폰 위치(없으면 이 오브젝트 위치)
    [SerializeField] private Transform peopleParent;    // 스폰된 사람들을 붙일 부모(개수 카운트 대상)

    [Header("Rules")]
    [Min(0.1f)]
    [SerializeField] private float respawnInterval = 5f; // 리스폰 주기(초)
    [SerializeField] private float minRespawnInterval = 0.25f; // 리스폰 최소 주기(초)
    [Min(0)]
    [SerializeField] private int maxPeople = 10;         // 최대 인원(peopleParent 하위)
    [SerializeField] private bool countOnlyActive = true;// 활성화된 자식만 카운트할지

    [Header("Behaviour")]
    [SerializeField] private bool spawnOnStart = true;   // 시작 즉시 한 명 스폰
    [SerializeField] private bool autoRun = true;        // 자동 리스폰 ON/OFF

    float _timer;

    void Reset()
    {
        // 자동 참조 편의
        if (!spawner) spawner = FindFirstObjectByType<PeopleSpawner>();
        if (!spawnPoint) spawnPoint = transform;
    }

    void Start()
    {
        GameManager.instance.OnModifyRespawnUselessPeople += ModifyRespawnUselessPeople;
        GameManager.instance.OnGetRespawnTime += GetRespawnInterval;
        GameManager.instance.OnGetNextRespawnTime += CalculateRespawnUselessPeople;

        if (!spawner) spawner = FindFirstObjectByType<PeopleSpawner>();
        if (!spawnPoint) spawnPoint = transform;

        if (spawnOnStart)
            TrySpawn();
    }

    private void OnDestroy()
    {
        GameManager.instance.OnModifyRespawnUselessPeople -= ModifyRespawnUselessPeople;
        GameManager.instance.OnGetRespawnTime -= GetRespawnInterval;
        GameManager.instance.OnGetNextRespawnTime -= CalculateRespawnUselessPeople;
    }

    void Update()
    {
        if (!autoRun) return;

        _timer += Time.deltaTime;
        if (_timer >= respawnInterval)
        {
            _timer = 0f;
            TrySpawn();
        }
    }


    void TrySpawn()
    {
        if (!spawner || !peopleParent)
        {
            Debug.LogWarning($"[PitHouse] Spawner or peopleParent missing on {name}");
            return;
        }
        int current = GetPeopleCount();
        if (current >= maxPeople) return;
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        var actor = spawner.SpawnFromProfile(pos, Quaternion.identity, null, peopleParent);
        if (!actor) { Debug.LogWarning("[PitHouse] Spawn failed."); return; }

        // people 등록
        PeopleManager.Instance.Register(actor);

        // --- 이동 관련 초기화 ---
        var go = actor.gameObject;

        // 1) 물리 컴포넌트 보장
        var rb = go.GetComponent<Rigidbody2D>() ?? go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var col = go.GetComponent<Collider2D>();
        if (!col) col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = false;

        // 2) Mover 부착 및 세팅
        var mover = go.GetComponent<Mover>() ?? go.AddComponent<Mover>();
        
        // 3) lockedArea 설정 후 강제 초기화
        mover.LockToArea(FindClosestZone(AreaType.Normal, (Vector2)pos));
        mover.ForceInitialize(); // 추가: 즉시 초기화하여 올바른 목표 설정
    }

    // 지정 타입의 가장 가까운 존 찾기
    AreaZone FindClosestZone(AreaType type, Vector2 from)
    {
        AreaZone best = null;
        float bestSqr = float.PositiveInfinity;
        foreach (var z in FindObjectsByType<AreaZone>(FindObjectsSortMode.None))
        {
            if (!z || z.areaType != type) continue;
            float d = ((Vector2)z.transform.position - from).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = z; }
        }
        return best;
    }


    int GetPeopleCount()
    {
        if (!peopleParent) return 0;

        if (!countOnlyActive)
            return peopleParent.childCount;

        // 활성 오브젝트 + PeopleActor만 카운트 (풀에서 비활성/이동된 오브젝트 제외)
        int c = 0;
        for (int i = 0; i < peopleParent.childCount; i++)
        {
            var t = peopleParent.GetChild(i);
            if (!t.gameObject.activeInHierarchy) continue;
            if (t.GetComponent<PeopleActor>()) c++;
        }
        return c;
    }

    // 외부에서 수동으로 스폰 트리거할 때 호출 가능
    public void ForceSpawnOnce()
    {
        TrySpawn();
    }

    // 생성 주기 계산
    private float CalculateRespawnUselessPeople(float amount)
    {
        return Mathf.Max(respawnInterval + amount, minRespawnInterval);
    }

    // 생성 주기 변경
    private void ModifyRespawnUselessPeople(float amount)
    {
        respawnInterval = CalculateRespawnUselessPeople(amount);
    }

    // 리스폰 시간 반환
    public float GetRespawnInterval()
    {
        return respawnInterval;
    }

    // 런타임에 설정 변경용 간단 API
    public void SetMaxPeople(int value) => maxPeople = Mathf.Max(0, value);
    public void SetInterval(float seconds) => respawnInterval = Mathf.Max(0.1f, seconds);
}
