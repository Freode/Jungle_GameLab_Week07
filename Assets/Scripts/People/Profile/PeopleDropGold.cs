using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PeopleDropGold : MonoBehaviour
{
    [SerializeField] float inactiveTime = 1f;

    // 골드 주머니 드랍 시작
    public void StartGoldDrop(int weight)
    {
        GameObject obj = ObjectPooler.Instance.SpawnObject(ObjectType.AcquireInfoUI);
        obj.transform.SetParent(GameManager.instance.canvasObject.transform);

        obj.TryGetComponent(out AcquireGoldAmountUI acquireComp);
        if (acquireComp == null)
            return;

        // 스케일을 0.6배로 설정
        if (obj.transform is RectTransform rectTransform)
        {
            rectTransform.localScale = Vector3.one * 0.6f;
        }

        long baseAmount = GameManager.instance.GetClickIncreaseTotalAmount();
        long randomAmount = FuncSystem.RandomLongRange(baseAmount * 2, baseAmount * 8);
        long amount = randomAmount * weight;

        GameManager.instance.AddCurrentGoldAmount(amount);
        GameLogger.Instance.gold.AcquireInteractGoldAmount(amount);

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        acquireComp.AcquireGold(amount, screenPosition + new Vector3(0f, 30f, 0f), screenPosition + new Vector3(0f, 60f, 0f), Color.black);

        StartCoroutine(DestroyTimer());
    }

    IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(inactiveTime);
        gameObject.SetActive(false);
    }
}
