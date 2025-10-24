using UnityEngine;

[DisallowMultipleComponent]
public class ObjectPoolInfo : MonoBehaviour
{
    [Header("이 프리팹/인스턴스가 속하는 풀 타입")]
    public ObjectType type;

    // 풀로 되돌아갈 때 자동으로 비활성/귀환하고 싶다면 편의용 API
    // Public ? Private ?
    public void ReturnToPool()
    {
        ObjectPooler.Instance.ReturnObject(gameObject);
    }
}
