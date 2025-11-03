using UnityEngine;

[DisallowMultipleComponent]
public class ObjectPoolInfo : MonoBehaviour
{
    [Header("Pool Type for this GameObject/Instance")]
    public ObjectType type;

    // 풀로 되돌릴 때 자동으로 비활성화/반환하고 싶다면 사용할 API
    // Public ? Private ?
    public void ReturnToPool()
    {
        ObjectPooler.Instance.ReturnObject(gameObject);
    }
}
