using UnityEngine;

[DisallowMultipleComponent]
public class ObjectPoolInfo : MonoBehaviour
{
    [Header("�� ������/�ν��Ͻ��� ���ϴ� Ǯ Ÿ��")]
    public ObjectType type;

    // Ǯ�� �ǵ��ư� �� �ڵ����� ��Ȱ��/��ȯ�ϰ� �ʹٸ� ���ǿ� API
    // Public ? Private ?
    public void ReturnToPool()
    {
        ObjectPooler.Instance.ReturnObject(gameObject);
    }
}
