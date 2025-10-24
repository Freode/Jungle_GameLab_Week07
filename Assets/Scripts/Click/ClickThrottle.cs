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
        float now = Time.unscaledTime;              // 타임스케일 영향 없음
        float dt = now - lastClickTime;

        if (dt < minInterval)
        {
            rejected++;
            if (logRejected)
            {
                float waitMs = (minInterval - dt) * 1000f;
                // Debug.Log($"[Click] REJECTED (anti-spam). Wait ~{waitMs:F0} ms | Rejected={_rejected}");
            }
            return false;
        }

        lastClickTime = now;
        accepted++;
        cpsCount++;

        // 1초 창으로 CPS 출력(선택)
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
        // 공식 : 선형 증가량 * 비율 증가량
        long totalAmount = GameManager.instance.GetClickIncreaseTotalAmount();

        Color color;
        int random = UnityEngine.Random.Range(1, 101);
        // 크리티컬 O - 권위 레벨 영향 받지 않음
        if(random <= criticalPercent)
        {
            totalAmount *= 100;
            color = Color.red;
            OnCriticalHit?.Invoke(); // 크리티컬 이벤트 발생
        }
        // 크리티컬 X - 권위 레벨 영향 받음
        else
        {
            totalAmount *= GameManager.instance.GetCurrentAuthority();
            color = Color.green;
            OnNormalHit?.Invoke(); // 일반 이벤트 발생
        }
        ReadyToScaleCoroutine();
        GameManager.instance.IncreaseGoldAmountWhenClicked(totalAmount, color);
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
