
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum ClickMode
{
    Whip,
    Explode,
    Thunder
}

public class ClickModeManager : MonoBehaviour
{
    public static ClickModeManager Instance { get; private set; }

    [Header("Buttons")]
    public Button whipButton;
    public Button explodeButton;
    public Button thunderButton;

    [Header("Click Mode Multipliers")]
    [Tooltip("Whip 모드의 데미지/효과 배수")]
    public float whipMultiplier = 1f;
    [Tooltip("Explode 모드의 데미지/효과 배수")]
    public float explodeMultiplier = 1f;
    [Tooltip("Thunder 모드의 데미지/효과 배수")]
    public float thunderMultiplier = 1f;

    public ClickMode CurrentMode { get; private set; } = ClickMode.Whip; // 기본 모드를 Whip으로 설정

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Explode와 Thunder 버튼 초기에는 비활성화 (테크로 언락해야 함)
        if (explodeButton != null)
            explodeButton.interactable = false;
        
        if (thunderButton != null)
            thunderButton.interactable = false;

        // 시작 시 기본 모드인 Whip 버튼을 선택된 상태로 만듭니다.
        SetWhipMode();
    }

    private void Update()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // 현재 선택된 오브젝트가 Whip, Explode 또는 Thunder 버튼이 아니면, 강제로 선택을 되돌립니다.
        if (currentSelected != whipButton.gameObject && currentSelected != explodeButton.gameObject && currentSelected != thunderButton.gameObject)
        {
            if (CurrentMode == ClickMode.Whip)
            {
                EventSystem.current.SetSelectedGameObject(whipButton.gameObject);
            }
            else if (CurrentMode == ClickMode.Explode)
            {
                EventSystem.current.SetSelectedGameObject(explodeButton.gameObject);
            }
            else if (CurrentMode == ClickMode.Thunder)
            {
                EventSystem.current.SetSelectedGameObject(thunderButton.gameObject);
            }
        }
    }

    public void SetWhipMode()
    {
        CurrentMode = ClickMode.Whip;
        EventSystem.current.SetSelectedGameObject(whipButton.gameObject);
        Debug.Log("Click Mode changed to: Whip");
    }

    public void SetExplodeMode()
    {
        // 버튼이 비활성화되어 있으면 변경하지 않음
        if (explodeButton != null && !explodeButton.interactable)
        {
            Debug.LogWarning("Explode 모드가 아직 언락되지 않았습니다!");
            return;
        }

        CurrentMode = ClickMode.Explode;
        EventSystem.current.SetSelectedGameObject(explodeButton.gameObject);
        Debug.Log("Click Mode changed to: Explode");
    }

    public void SetThunderMode()
    {
        // 버튼이 비활성화되어 있으면 변경하지 않음
        if (thunderButton != null && !thunderButton.interactable)
        {
            Debug.LogWarning("Thunder 모드가 아직 언락되지 않았습니다!");
            return;
        }

        CurrentMode = ClickMode.Thunder;
        EventSystem.current.SetSelectedGameObject(thunderButton.gameObject);
        Debug.Log("Click Mode changed to: Thunder");
    }

    /// <summary>
    /// 현재 활성화된 클릭 모드의 배수를 반환합니다.
    /// </summary>
    public float GetCurrentMultiplier()
    {
        switch (CurrentMode)
        {
            case ClickMode.Whip:
                return whipMultiplier;
            case ClickMode.Explode:
                return explodeMultiplier;
            case ClickMode.Thunder:
                return thunderMultiplier;
            default:
                return 1f;
        }
    }

    /// <summary>
    /// 특정 클릭 모드의 배수를 설정합니다.
    /// </summary>
    public void SetClickModeMultiplier(ClickMode mode, float multiplier)
    {
        switch (mode)
        {
            case ClickMode.Whip:
                whipMultiplier = multiplier;
                Debug.Log($"[ClickModeManager] Whip 배수가 {multiplier}로 설정되었습니다.");
                break;
            case ClickMode.Explode:
                explodeMultiplier = multiplier;
                Debug.Log($"[ClickModeManager] Explode 배수가 {multiplier}로 설정되었습니다.");
                break;
            case ClickMode.Thunder:
                thunderMultiplier = multiplier;
                Debug.Log($"[ClickModeManager] Thunder 배수가 {multiplier}로 설정되었습니다.");
                break;
        }
    }
}

