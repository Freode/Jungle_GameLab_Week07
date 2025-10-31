using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PeopleDropGold : MonoBehaviour
{
    [SerializeField] float inactiveTime = 1f;

    // 골드 주머니 드랍 시작
public void StartGoldDrop(long amount)
{
    // 실제로 호버로 벌어들인 금액: amount
    GameManager.instance.AddCurrentGoldAmount(amount);
    GameLogger.Instance.gold.AcquireInteractGoldAmount(amount);

    // 개수 결정(예시 구간: 필요시 조정 가능)
    int pouchCount = GetPouchCount(amount);

    // 중심 위치(스크린 좌표)
    Vector3 baseScreen = Camera.main.WorldToScreenPoint(transform.position);
    Vector3 baseStart  = baseScreen + new Vector3(0f, 100f, 0f);
    Vector3 baseEnd    = baseScreen + new Vector3(0f, 120f, 0f);

    // 주변 랜덤 산포 반경
    float radius = 5f + 3f * (pouchCount - 1); // 개수 많을수록 조금 더 넓게

    for (int i = 0; i < pouchCount; i++)
    {
        Vector2 offset = (pouchCount == 1)
            ? Vector2.zero
            : Random.insideUnitCircle * radius;

        Vector3 start = baseStart + new Vector3(offset.x, offset.y, 0f);
        Vector3 end   = baseEnd   + new Vector3(offset.x, offset.y, 0f);

        GameObject obj = ObjectPooler.Instance.SpawnObject(ObjectType.AcquireInfoUI);
        obj.transform.SetParent(GameManager.instance.canvasObject.transform, false);

        if (obj.transform is RectTransform rectTransform)
        {
            rectTransform.localScale = Vector3.one * 0.6f;
        }

        if (!obj.TryGetComponent(out AcquireGoldAmountUI acquireComp))
            continue;

        if (i == 0)
        {
            // 대표 1개: 텍스트 표시
            acquireComp.AcquireGold(amount, start, end, Color.black, showText: true);
        }
        else
        {
            // 나머지: 이미지만
            acquireComp.AcquireGold(amount, start, end, Color.black, showText: false);
        }
    }

    StartCoroutine(DestroyTimer());
}

// 금액 → 주머니 개수 맵핑(예시: 요구 사례 반영)
    private int GetPouchCount(long amt)
    {
        const long MIN = 100L;     // 최소치
        const long MAX = 50000L;   // 최대치

        if (amt <= MIN) return 1;
        if (amt >= MAX) return 10;

        // MIN~MAX를 9구간으로 나눠 1~10개로 선형 매핑
        double ratio = (double)(amt - MIN) / (double)(MAX - MIN); // 0.0 ~ 1.0
        int count = 1 + (int)System.Math.Floor(ratio * 9.0 + 1e-9);
        return Mathf.Clamp(count, 1, 10);
    }

    IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(inactiveTime);
        gameObject.SetActive(false);
    }
}
