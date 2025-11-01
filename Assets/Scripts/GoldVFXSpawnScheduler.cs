// GoldVFXSpawnScheduler.cs
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoldVFXSpawnScheduler : MonoBehaviour
{
    public static GoldVFXSpawnScheduler Instance;

    [Header("Budget")]
    [SerializeField] private int maxSpawnsPerFrame = 4;   // 프레임당 생성/활성화 예산
    [SerializeField] private int maxActiveVfx = 120;      // 동시에 살아있는 AcquireInfoUI 상한
    [SerializeField] private int maxQueue = 500;          // 큐 길이 상한(폭주 방지)

    private readonly Queue<SpawnReq> _queue = new Queue<SpawnReq>();
    private int _activeCount;
    
    [Header("Return Budget")]
    [SerializeField] private int maxReturnsPerFrame = 12; // 프레임당 반납 처리량
    private readonly Queue<GameObject> _returnQueue = new Queue<GameObject>();

    // 요청 모델
    private struct SpawnReq
    {
        public long amount;
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public bool showText;
        public bool showImage;
        public float? overrideFontSize;
        public Transform parent;          // 캔버스 등
        public Vector2? scaleRange;       // (min,max)
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // 필요하면 DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 둘 다 비어있을 때만 빠르게 리턴
        if (_queue.Count == 0 && _returnQueue.Count == 0) return;

        // 1) 스폰 배치
        int budget = maxSpawnsPerFrame;
        while (budget-- > 0 && _queue.Count > 0)
        {
            if (_activeCount >= maxActiveVfx) break;

            var req = _queue.Dequeue();
            SpawnOne(req);
        }

        // 2) 반납 배치
        int rbudget = maxReturnsPerFrame;
        while (rbudget-- > 0 && _returnQueue.Count > 0)
        {
            var go = _returnQueue.Dequeue();
            if (go != null)
            {
                ObjectPooler.Instance.ReturnObject(go);
                // _activeCount 감소는 Watcher(OnDisable)에서 처리
            }
        }
    }


    private void SpawnOne(SpawnReq req)
    {
        var obj = ObjectPooler.Instance.SpawnObject(ObjectType.AcquireInfoUI);
        if (!obj) return;

        _activeCount++;
        obj.transform.SetParent(req.parent, false);

        // 스케일 랜덤(옵션)
        if (obj.transform is RectTransform rt && req.scaleRange.HasValue)
        {
            float s = Random.Range(req.scaleRange.Value.x, req.scaleRange.Value.y);
            rt.localScale = new Vector3(s, s, 1f);
        }

        if (obj.TryGetComponent<AcquireGoldAmountUI>(out var ui))
        {
            // AcquireGoldAmountUI가 Return 시 풀로 복귀하므로, 복귀 시점에 _activeCount 감소 필요
            // 간단히 완료 콜백을 넘길 수 없으므로, 아래 헬퍼를 부착해 콜백처럼 사용
            AttachReturnWatcher(obj);

            ui.AcquireGold(
                req.amount,
                req.start,
                req.end,
                req.color,
                req.showText,
                req.showImage,
                req.overrideFontSize
            );
        }
        else
        {
            // 컴포넌트 없으면 즉시 반납
            ObjectPooler.Instance.ReturnObject(obj);
            _activeCount--;
        }
    }

    private void AttachReturnWatcher(GameObject obj)
    {
        if (!obj.TryGetComponent<AcquireVfxReturnWatcher>(out var w))
            w = obj.AddComponent<AcquireVfxReturnWatcher>();
        w.Init(OnOneReturned);
    }

    private void OnOneReturned()
    {
        _activeCount = Mathf.Max(0, _activeCount - 1);
    }

    // 외부에서 호출하는 API
    public void Enqueue(
        long amount, Vector3 start, Vector3 end, Color color,
        Transform parent, bool showText = true, bool showImage = true,
        float? overrideFontSize = null, Vector2? scaleRange = null, int count = 1)
    {
        if (count <= 0) return;

        // 큐 폭주 시 방어: 너무 많으면 텍스트 1개로 축약
        if (_queue.Count + count > maxQueue)
        {
            count = Mathf.Max(1, maxQueue - _queue.Count);
        }

        for (int i = 0; i < count; i++)
        {
            if (_queue.Count >= maxQueue) break;

            _queue.Enqueue(new SpawnReq
            {
                amount = amount,
                start = start,
                end = end,
                color = color,
                showText = (i == 0) && showText,  // 첫 개만 텍스트
                showImage = showImage,
                overrideFontSize = overrideFontSize,
                parent = parent,
                scaleRange = scaleRange
            });
        }
    }
    
    // 외부(또는 UI)에서 즉시 반납하지 말고 스케줄러로 요청
    public void RequestReturn(GameObject go)
    {
        if (go == null) return;
        _returnQueue.Enqueue(go);
    }
}

// 풀 반납 감시용(프리팹 반환 순간을 캐치해서 Active 카운트 줄이기)
public class AcquireVfxReturnWatcher : MonoBehaviour
{
    private System.Action _onReturn;

    public void Init(System.Action onReturn) => _onReturn = onReturn;

    private void OnDisable()
    {
        // 풀로 돌아갈 때 Disable 될 가능성 높음
        _onReturn?.Invoke();
        _onReturn = null;
    }
}


