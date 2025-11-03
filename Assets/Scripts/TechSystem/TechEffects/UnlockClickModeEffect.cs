using UnityEngine;

/// <summary>
/// 클릭 모드(Explode, Thunder)를 언락하는 테크 효과
/// </summary>
[CreateAssetMenu(fileName = "UnlockClickModeEffect", menuName = "Scriptable Objects/Tech Effect/Unlock Click Mode Effect")]
public class UnlockClickModeEffect : BaseTechEffect
{
    [Header("언락할 클릭 모드")]
    [Tooltip("언락할 클릭 모드를 선택하세요 (Explode 또는 Thunder)")]
    public ClickMode clickModeToUnlock;

    [Header("클릭 모드 배수 설정")]
    [Tooltip("이 클릭 모드의 데미지/효과 배수를 설정합니다")]
    [Min(0.1f)]
    public float damageMultiplier = 1f;

    public override void ApplyTechEffect()
    {
        if (ClickModeManager.Instance == null)
        {
            Debug.LogError("[UnlockClickModeEffect] ClickModeManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        // 배수 설정
        ClickModeManager.Instance.SetClickModeMultiplier(clickModeToUnlock, damageMultiplier);

        switch (clickModeToUnlock)
        {
            case ClickMode.Explode:
                UnlockExplodeMode();
                break;
            case ClickMode.Thunder:
                UnlockThunderMode();
                break;
            case ClickMode.Whip:
                Debug.LogWarning("[UnlockClickModeEffect] Whip 모드는 기본 모드이므로 언락할 필요가 없습니다.");
                break;
            default:
                Debug.LogError($"[UnlockClickModeEffect] 알 수 없는 클릭 모드: {clickModeToUnlock}");
                break;
        }
    }

    /// <summary>
    /// Explode 모드 언락
    /// </summary>
    private void UnlockExplodeMode()
    {
        if (ClickModeManager.Instance.explodeButton != null)
        {
            ClickModeManager.Instance.explodeButton.interactable = true;
            Debug.Log("[UnlockClickModeEffect] Explode 모드가 언락되었습니다!");
        }
        else
        {
            Debug.LogError("[UnlockClickModeEffect] Explode 버튼을 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// Thunder 모드 언락
    /// </summary>
    private void UnlockThunderMode()
    {
        if (ClickModeManager.Instance.thunderButton != null)
        {
            ClickModeManager.Instance.thunderButton.interactable = true;
            Debug.Log("[UnlockClickModeEffect] Thunder 모드가 언락되었습니다!");
        }
        else
        {
            Debug.LogError("[UnlockClickModeEffect] Thunder 버튼을 찾을 수 없습니다!");
        }
    }
}
