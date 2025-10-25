using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 클릭 수락 간 최소 간격(minInterval)로 연타 보호.
/// - 수락된 클릭은 Debug.Log로A 출력
/// - 거절된 클릭은 남은 대기시간(ms)와 함께 로그(옵션)
/// - 마우스 좌클릭(임시) 또는 UI Button.onClick에 TryClick() 연결
/// </summary>
public class ClickThrottle : MonoBehaviour
{
    [Header("Anti-Spam")]
    [Tooltip("두 클릭 사이의 최소 시간(초). 예: 0.06s ≈ 최대 16.6 CPS")]
    [Range(0.01f, 0.25f)] public float minInterval = 0.06f;

    [Header("Debug")]
    [Tooltip("거절된 클릭도 로그로 볼지 여부")]
    public bool logRejected = true;
    public Button buttonGold;
    public int criticalPercent = 0;

    [Header("Button Anim")]
    [SerializeField] float animationDuration = 0.08f;    // 전체 애니메이션 시간
    [SerializeField] float targetScaleFactor = 1.1f;    // 얼마나 커질 것인지 확인

    private Vector3 originalScale;                      // 원래 버튼 크기
    private Coroutine buttonAnimCoroutine;              // 버튼 애님 코루틴

    // 크리티컬 이벤트
    public static event System.Action OnCriticalHit;
    public static event System.Action OnNormalHit;

    private float lastClickTime = -9999f;
    private int accepted;
    private int rejected;

    // 선택: 1초 단위 CPS 간이 측정
    private float cpsWindowStart;
    private int cpsCount;

    public int tempCount = 0;
    public int mouseCount = 0;
    
    // 시도 시작 시각과 직전 시도 간 간격 추적용
    private float _lastTryStart = -9999f;

    private void Awake()
    {
        cpsWindowStart = Time.unscaledTime;
    }

    private void Start()
    {
        buttonGold.onClick.AddListener(OnButtonGoldClick);
        originalScale = transform.localScale;
    }


    /// <summary>연타 보호를 적용한 클릭 시도</summary>
    public bool TryClick()
    {
        float now = Time.unscaledTime;

        // 직전 시도와의 간격 추적은 유지(통계용)
        float prevTryDt = (_lastTryStart < -9998f) ? -1f : (now - _lastTryStart);
        _lastTryStart = now;

        // 마지막 수락 시각과의 간격
        float dt = now - lastClickTime;

        // 연타 보호: 거절
        if (dt < minInterval)
        {
            rejected++;
            if (logRejected)
            {
                float waitMs = (minInterval - dt) * 1000f;

                // 사진 양식: [TimeStamp] [LogType] Message/Message/Message
                // → 파일에는 [HH:mm:ss] [Click] ... 로 찍힘
                GameLogger.Instance?.Log(
                    "Click",
                    $"Rejected/prevTryDt={(prevTryDt < 0 ? -1 : prevTryDt * 1000f):F0}ms/lastClickToNow={dt * 1000f:F0}ms/needWait={waitMs:F0}ms/rejected={rejected}"
                );
            }
            return false;
        }

        // 수락(로그 없음)
        lastClickTime = now;
        accepted++;
        cpsCount++;

        // 1초 창 리셋(선택)
        if (now - cpsWindowStart >= 1f)
        {
            cpsWindowStart = now;
            cpsCount = 0;
        }

        return true;
    }

    // 클릭 시, 금 획득
    // 금 획득 시 공포 게이지 증가
    private void OnButtonGoldClick()
    {
        if (TryClick() == false)
            return;

        AuthorityManager.instance.IncreaseAuthority();
        ReadyToScaleCoroutine();
        GameManager.instance.HandleGoldClick();

        // 클릭에 대한 골드를 최종값으로 더하기 (GameManager.HandleGoldClick에서 이미 처리되므로 여기서는 제거)
        // GameLogger.Instance.click.AddGoldClick(); // 이 부분은 GameManager에서 처리하도록 변경하거나, 필요에 따라 유지
        // GameLogger.Instance.gold.AcquireNormalGoldAmount(totalAmount); // 이 부분은 GameManager에서 처리하도록 변경하거나, 필요에 따라 유지
    }

    // 버튼 작동 준비
    private void ReadyToScaleCoroutine()
    {
        if (buttonAnimCoroutine != null)
            StopCoroutine(buttonAnimCoroutine);

        buttonAnimCoroutine = StartCoroutine(PunchScaleCoroutine());
    }

    // 버튼 애니메이션 시작
    IEnumerator PunchScaleCoroutine()
    {
        Vector3 targetScale = originalScale * targetScaleFactor;
        float halfDuration = animationDuration / 2f;
        float elapsedTime = 0f;

        // 커지는 애니메이션
        while (elapsedTime < halfDuration)
        {
            buttonGold.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 작아지는 애니메이션
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            buttonGold.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 완료
        buttonGold.transform.localScale = originalScale;
        buttonAnimCoroutine = null;
    }
}
