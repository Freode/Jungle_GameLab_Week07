using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public ObjectType type;
        public GameObject prefab;
        [Min(0)] public int initialSize = 0;
        public bool expandable = true; // Queue가 비면 Instantiate 허용
    }

    public static ObjectPooler Instance { get; private set; }

    [Header("타입 ↔ 프리팹 매핑 및 초기 크기")]
    public List<PoolEntry> entries = new();

    // 타입별 큐
    private readonly Dictionary<ObjectType, Queue<GameObject>> _pools = new();
    // 타입별 프리팹/옵션
    private readonly Dictionary<ObjectType, PoolEntry> _registry = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[ObjectPooler] 중복 인스턴스가 감지되어 이전 인스턴스를 대체합니다.");
        }
        Instance = this;

        // 레지스트리/큐 초기화
        foreach (var e in entries)
        {
            if (e.prefab == null)
            {
                Debug.LogError($"[ObjectPooler] {e.type} 의 prefab이 비어있습니다.");
                continue;
            }

            if (_registry.ContainsKey(e.type))
            {
                Debug.LogWarning($"[ObjectPooler] 중복 타입 등록 감지: {e.type}. 마지막 등록으로 덮어서 사용합니다.");
            }
            _registry[e.type] = e;

            if (!_pools.ContainsKey(e.type))
                _pools[e.type] = new Queue<GameObject>();

            // Prewarm
            for (int i = 0; i < e.initialSize; i++)
            {
                var go = Instantiate(e.prefab, transform);
                if (go.GetComponent<ObjectPoolInfo>() == null)
                    go.AddComponent<ObjectPoolInfo>().type = e.type;
                else
                    go.GetComponent<ObjectPoolInfo>().type = e.type;

                go.SetActive(false);
                _pools[e.type].Enqueue(go);
            }
        }
    }

    

    /// <summary>
    /// 간단 Spawn: 위치/회전 기본값
    /// </summary>
    /// <summary>간단 Spawn: 위치/회전 기본값</summary>
    public GameObject SpawnObject(ObjectType type)
    {
        return SpawnObject(type, Vector3.zero, Quaternion.identity, null);
    }


    /// <summary>
    /// 풀에서 꺼내 활성화 → 부모 null(또는 지정 parent) → 위치/회전 세팅
    /// Queue가 비면 Instantiate하고 ObjectPoolInfo 붙임
    /// </summary>
    /// 


    public GameObject SpawnObject(ObjectType type, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!_pools.ContainsKey(type))
            _pools[type] = new Queue<GameObject>();

        GameObject go = null;

        // 1) Queue에서 반환
        if (_pools[type].Count > 0)
        {
            go = _pools[type].Dequeue();
        }
        else
        {
            // 4) Queue가 비어있다면 Instantiate
            if (!_registry.TryGetValue(type, out var entry) || entry.prefab == null)
            {
                Debug.LogError($"[ObjectPooler] {type} 타입이 레지스트리에 없습니다. entries에 등록하세요.");
                return null;
            }

            if (!entry.expandable)
            {
                Debug.LogWarning($"[ObjectPooler] {type} 풀은 확장 불가로 설정되어 있고, 재고가 없습니다.");
                return null;
            }

            go = Instantiate(entry.prefab);
            // AddComponent로 ObjectPoolInfo를 보장
            var info = go.GetComponent<ObjectPoolInfo>();
            if (info == null) info = go.AddComponent<ObjectPoolInfo>();
            info.type = type;
        }

        // 2) SetActive(true)
        go.SetActive(true);

        // 3) 부모를 null (필요 시 parent 지정 우선)
        go.transform.SetParent(parent ? parent : null, worldPositionStays: false);

        // 위치/회전 세팅
        go.transform.SetPositionAndRotation(position, rotation);

        
        return go;
    }

    /// <summary>
    /// 인자로 받은 오브젝트를 비활성화 → 풀러의 자식으로 이동 → 타입 확인 후 해당 큐에 삽입
    /// </summary>
    public void ReturnObject(GameObject obj)
    {
        if (obj == null) return;

        // 1) SetActive(false)
        obj.SetActive(false);

        // 2) 풀러의 자식으로 이동
        obj.transform.SetParent(transform, worldPositionStays: false);

        // 3) 타입 확인 후 큐 삽입
        //var info = obj.GetComponent<ObjectPoolInfo>();
        var info = obj.TryGetComponent<ObjectPoolInfo>(out var temp) ? temp : null;

        if (info == null)
        {
            // 명세대로라면 AddComponent는 Spawn 시 보장되지만, 혹시 없으면 여기서 붙여도 됨.
            // 다만 타입을 모르면 재사용이 애매해 로그만 남김.
            Debug.LogWarning($"[ObjectPooler] 반환된 오브젝트에 ObjectPoolInfo가 없습니다. 수동 할당이 필요합니다. ({obj.name})");
            return;
        }

        if (!_pools.ContainsKey(info.type))
            _pools[info.type] = new Queue<GameObject>();

        _pools[info.type].Enqueue(obj);
    }

    // 선택: 런타임에 타입/프리팹 등록 & 선행채움이 필요할 때
    public void Register(ObjectType type, GameObject prefab, int prewarm = 0, bool expandable = true)
    {
        var entry = new PoolEntry { type = type, prefab = prefab, initialSize = prewarm, expandable = expandable };
        _registry[type] = entry;
        if (!_pools.ContainsKey(type))
            _pools[type] = new Queue<GameObject>();

        for (int i = 0; i < prewarm; i++)
        {
            var go = Instantiate(prefab, transform);
            var info = go.GetComponent<ObjectPoolInfo>() ?? go.AddComponent<ObjectPoolInfo>();
            info.type = type;
            go.SetActive(false);
            _pools[type].Enqueue(go);
        }
    }

    // 선택: 해당 타입 재고 수 조회
    public int Count(ObjectType type) => _pools.TryGetValue(type, out var q) ? q.Count : 0;

    // 선택: 해당 타입 풀 보유 여부
    public bool HasPool(ObjectType type) => _registry.ContainsKey(type);
}
