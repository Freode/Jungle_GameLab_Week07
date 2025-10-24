using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class YearDisplayUI : MonoBehaviour
{
    // ★ 'GameTimeManager'에 대한 직접 참조를 삭제했습니다!
    
    [Header("UI 설정")]
    public Slider yearProgressBar;
    public float secondsPerYear = 10f; // UI가 스스로 '1년'의 길이를 알게 합니다.

    [Header("구독할 채널")]
    public IntEventChannelSO OnYearChangedChannel;

    [Header("UI 컴포넌트")]
    public TextMeshProUGUI yearText;

    // ★ UI가 자체적으로 시간을 측정할 타이머
    private float timer = 0f;

    private void OnEnable()
    {
        OnYearChangedChannel.OnEventRaised += OnYearChanged;
    }

    private void OnDisable()
    {
        OnYearChangedChannel.OnEventRaised -= OnYearChanged;
    }

    // ★ Update 함수가 이제 GameTimeManager를 참조하지 않습니다.
    void Update()
    {
        // 스스로 시간을 셉니다.
        timer += Time.deltaTime;
        
        // 스스로의 타이머를 기준으로 진행 바를 채웁니다.
        // Mathf.Clamp01은 값이 0과 1사이를 넘지 않도록 보장해줍니다.
        yearProgressBar.value = Mathf.Clamp01(timer / secondsPerYear);
    }

    // '연도 변경' 방송을 받으면 호출되는 함수
    private void OnYearChanged(int newYear)
    {
        // ★ 방송을 받으면 타이머를 0으로 즉시 초기화!
        timer = 0f;
        
        // 텍스트 업데이트
        if (newYear < 0)
        {
            yearText.text = $"BC {-newYear}";
        }
        else
        {
            yearText.text = $"AD {newYear}";
        }
    }
}