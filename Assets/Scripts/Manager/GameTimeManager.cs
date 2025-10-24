using UnityEngine;
// using TMPro; // 더 이상 UI를 직접 제어하지 않으므로 이 줄은 없어도 됩니다.

public class GameTimeManager : MonoBehaviour
{
    [Header("Event Channels")]
    public VoidEventChannelSO OnYearPassedChannel; // 1년 지났다는 '신호'
    public IntEventChannelSO OnYearChangedChannel; // '새로운 연도' 정보 전달

    [Header("Time Settings")]
    public float secondsPerYear = 10f;
    public int startYear = -2560;

    // ★ UI 관련 변수와 함수가 모두 사라졌습니다.
    private int currentYear;
    private float yearTimer;

    void Start()
    {
        currentYear = startYear;
        // ★ 시작하자마자 현재 연도를 방송!
        OnYearChangedChannel.RaiseEvent(currentYear);
    }

    void Update()
    {
        yearTimer += Time.deltaTime;

        if (yearTimer >= secondsPerYear)
        {
            yearTimer = 0f;
            currentYear++;
            if (currentYear == 0) currentYear = 1;

            // '1년 지남' 신호 방송 (나이 먹기 용)
            OnYearPassedChannel.RaiseEvent();
            // ★ '변경된 현재 연도' 정보 방송 (UI 표시용)
            OnYearChangedChannel.RaiseEvent(currentYear);
        }
    }

    public float GetYearProgress()
    {
        return yearTimer / secondsPerYear;
    }
}