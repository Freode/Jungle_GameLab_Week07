using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PeopleDropGold : MonoBehaviour
{
    [SerializeField] float inactiveTime = 1f;

    // 골드 주머니 드랍 시작
// PeopleDropGold.cs (StartGoldDrop 수정)
    public void StartGoldDrop(long amount)
    {
        GameManager.instance.AddCurrentGoldAmount(amount);
        GameLogger.Instance.gold.AcquireInteractGoldAmount(amount);

        int pouchCount = GetPouchCount(amount);

        Vector3 baseScreen = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 baseStart  = baseScreen + new Vector3(0f, 100f, 0f);
        Vector3 baseEnd    = baseScreen + new Vector3(0f, 120f, 0f);

        float radius = 5f + 3f * (pouchCount - 1);

        // 여기서 바로 루프 생성하지 않고, 각각을 스케줄러에 enqueue
        for (int i = 0; i < pouchCount; i++)
        {
            Vector2 offset = (pouchCount == 1) ? Vector2.zero : Random.insideUnitCircle * radius;
            Vector3 start = baseStart + new Vector3(offset.x, offset.y, 0f);
            Vector3 end   = baseEnd   + new Vector3(offset.x, offset.y, 0f);

            /*// 스케일 랜덤 범위(옵션) – 원래 주석 처리했던 0.6 고정 대신 범위로
            Vector2 scaleRange = new Vector2(0.6f, 1.0f);*/

            GoldVFXSpawnScheduler.Instance.Enqueue(
                amount: amount,
                start: start,
                end: end,
                color: Color.black,
                parent: GameManager.instance.canvasObject.transform,
                showText: (i == 0),
                showImage: true,
                overrideFontSize: null,
                scaleRange: null,   // ← 랜덤 스케일 미사용
                count: 1
            );
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
