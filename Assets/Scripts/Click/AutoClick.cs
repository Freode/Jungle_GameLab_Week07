using System.Collections;
using UnityEngine;

public class AutoClick : MonoBehaviour
{
    [SerializeField] float interval = 10f;     // 실행 주기

    private float _localInterval = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _localInterval = interval;
        GameManager.instance.IncreaseAutoClickInterval(_localInterval);
        StartCoroutine(UpdateAutoClick());
    }

    IEnumerator UpdateAutoClick()
    {
        yield return new WaitForSeconds(0.5f);

        while(true)
        {
            // 자동 클릭을 Interval 시간만큼 나눠서 진행
            // 현재 시점에서 자동 클릭 횟수
            long baseCount = GameManager.instance.GetAutoClickCount();

            long divCount = baseCount / (long)_localInterval;
            long curCount = baseCount % (long)_localInterval;

            for (int i = 0; i < (long)interval; i++)
            {
                _localInterval = GameManager.instance.GetAutoClickInterval();
                yield return new WaitForSeconds(_localInterval / interval);

                long newCount = GameManager.instance.GetAutoClickCount() - baseCount;

                long autoClickCount = divCount + newCount;
                // 1번 더 추가
                if (i < curCount)
                    ++autoClickCount;

                GameManager.instance.HandleAutoGoldClick(autoClickCount);

                // 오토 클릭 실행
                baseCount = baseCount + newCount;
            }
        }
    }
}
