using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 일반 버튼에 무지개 효과를 쉽게 적용할 수 있는 헬퍼 컴포넌트
/// Inspector에서 설정하거나 코드로 제어 가능
/// </summary>
[RequireComponent(typeof(Button))]
public class RainbowButtonTrigger : MonoBehaviour
{
    [Header("자동 설정")]
    [Tooltip("게임 시작 시 자동으로 효과 활성화")]
    [SerializeField] private bool activateOnStart = false;

    [Tooltip("특정 시간 후에 자동으로 효과 활성화")]
    [SerializeField] private bool activateAfterDelay = false;
    [SerializeField] private float delayTime = 2f;

    [Tooltip("버튼 클릭 시 자동으로 효과 비활성화")]
    [SerializeField] private bool deactivateOnClick = true;

    [Tooltip("효과 활성화 후 일정 시간 뒤 자동으로 비활성화")]
    [SerializeField] private bool autoDeactivateAfterDuration = false;
    [SerializeField] private float autoDeactivateDuration = 2f;

    [Header("수동 제어 (Inspector에서 테스트용)")]
    [SerializeField] private bool testActivate = false;
    [SerializeField] private bool testDeactivate = false;

    private Button button;
    private RainbowButtonEffect rainbowEffect;

    private void Awake()
    {
        button = GetComponent<Button>();
        
        // RainbowButtonEffect 가져오기 또는 추가
        rainbowEffect = GetComponent<RainbowButtonEffect>();
        if (rainbowEffect == null)
        {
            rainbowEffect = gameObject.AddComponent<RainbowButtonEffect>();
        }

        // 버튼 클릭 리스너 추가
        if (deactivateOnClick)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void Start()
    {
        if (activateOnStart)
        {
            ActivateEffect();
        }

        if (activateAfterDelay)
        {
            Invoke(nameof(ActivateEffect), delayTime);
        }
    }

    private void Update()
    {
        // Inspector에서 테스트용
        if (testActivate)
        {
            testActivate = false;
            ActivateEffect();
        }

        if (testDeactivate)
        {
            testDeactivate = false;
            DeactivateEffect();
        }
    }

    private void OnButtonClicked()
    {
        if (deactivateOnClick)
        {
            DeactivateEffect();
        }
    }

    /// <summary>
    /// 무지개 효과 활성화 (외부에서 호출 가능)
    /// </summary>
    public void ActivateEffect()
    {
        if (rainbowEffect != null)
        {
            rainbowEffect.ActivateEffect();

            // 자동 비활성화 설정이 켜져 있으면 타이머 시작
            if (autoDeactivateAfterDuration)
            {
                CancelInvoke(nameof(DeactivateEffect)); // 기존 타이머 취소
                Invoke(nameof(DeactivateEffect), autoDeactivateDuration);
            }
        }
    }

    /// <summary>
    /// 무지개 효과 비활성화 (외부에서 호출 가능)
    /// </summary>
    public void DeactivateEffect()
    {
        if (rainbowEffect != null)
        {
            CancelInvoke(nameof(DeactivateEffect)); // 예약된 자동 비활성화 취소
            rainbowEffect.DeactivateEffect();
        }
    }

    /// <summary>
    /// 효과 토글
    /// </summary>
    public void ToggleEffect()
    {
        if (rainbowEffect != null)
        {
            if (rainbowEffect.IsEffectActive())
            {
                DeactivateEffect();
            }
            else
            {
                ActivateEffect();
            }
        }
    }

    /// <summary>
    /// 효과 활성 상태 확인
    /// </summary>
    public bool IsEffectActive()
    {
        return rainbowEffect != null && rainbowEffect.IsEffectActive();
    }

    private void OnDestroy()
    {
        if (button != null && deactivateOnClick)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
