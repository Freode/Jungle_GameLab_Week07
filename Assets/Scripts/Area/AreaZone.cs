using UnityEngine;
using System.Collections.Generic;

public enum AreaType { Normal, Mine, Carrier, Architect, StoneCarving, Gold ,Prison, Pyramid, Clear, Barrack, Temple, Brewery}

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class AreaZone : MonoBehaviour
{
    [Header("Area Settings")]
    public AreaType areaType = AreaType.Normal;

    [Header("Random Point Settings")]
    [SerializeField] private float edgePadding = 0.5f; // 가장자리로부터의 여백

    private BoxCollider2D boxCollider;
    private List<Mover> moversInside = new List<Mover>();

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }


    // 영역 내부의 랜덤한 점 반환
    public Vector2 GetRandomPointInside()
    {
        Bounds bounds = boxCollider.bounds;

        float minX = bounds.min.x + edgePadding;
        float maxX = bounds.max.x - edgePadding;
        float minY = bounds.min.y + edgePadding;
        float maxY = bounds.max.y - edgePadding;

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        return new Vector2(randomX, randomY);
    }

    // 특정 점이 영역 내부에 있는지 확인
    public bool IsPointInside(Vector2 point)
    {
        return boxCollider.OverlapPoint(point);
    }

    // 영역 중심점 반환
    public Vector2 GetCenter()
    {
        return boxCollider.bounds.center;
    }

    // 영역 크기 반환
    public Vector2 GetSize()
    {
        return boxCollider.bounds.size;
    }

    // 특정 Mover를 이 영역에 고정
    public void LockMover(Mover mover)
    {
        if (mover != null)
        {
            mover.LockToArea(this);
        }
    }

    // 특정 Mover의 영역 고정 해제
    public void UnlockMover(Mover mover)
    {
        if (mover != null)
        {
            mover.UnlockArea();
        }
    }

    // 이 영역 내의 모든 Mover를 고정
    public void LockAllMoversInside()
    {
        foreach (Mover mover in moversInside)
        {
            if (mover != null)
            {
                LockMover(mover);
            }
        }
    }

    // 이 영역 내의 모든 Mover 고정 해제
    public void UnlockAllMoversInside()
    {
        foreach (Mover mover in moversInside)
        {
            if (mover != null)
            {
                UnlockMover(mover);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Mover mover = other.GetComponent<Mover>();
        if (mover != null && !moversInside.Contains(mover))
        {
            moversInside.Add(mover);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Mover mover = other.GetComponent<Mover>();
        if (mover != null)
        {
            moversInside.Remove(mover);
        }
    }

    private Color GetColorByType(AreaType type)
    {
        switch (type)
        {
            case AreaType.Normal:
                return new Color(0f, 1f, 0f, 0.3f); // 초록
            case AreaType.Mine:
                return new Color(0.6f, 0.4f, 0.2f, 0.3f); // 갈색
            case AreaType.Carrier:
                return new Color(0f, 0.5f, 1f, 0.3f); // 파란색
            case AreaType.Architect:
                return new Color(1f, 0.8f, 0f, 0.3f); // 노란색
            case AreaType.StoneCarving:
                return new Color(0.5f, 0.5f, 0.5f, 0.3f); // 회색
            case AreaType.Prison:
                return new Color(1f, 0f, 0f, 0.3f); // 빨간색
            default:
                return new Color(1f, 1f, 1f, 0.3f); // 흰색
        }
    }

    // Public getters
    public AreaType GetAreaType() => areaType;
    public List<Mover> GetMoversInside() => new List<Mover>(moversInside);
    public int GetMoverCount() => moversInside.Count;
}