using UnityEngine;

/// <summary>
/// 낙타 보너스 지속시간을 비율로 증가시키는 TechEffect
/// </summary>
[CreateAssetMenu(fileName = "AddCamelBonusDurationRate", menuName = "Scriptable Objects/Tech Effect/Add Camel Bonus Duration Rate")]
public class AddCamelBonusDurationRateEffect : BaseTechEffect
{
    [Header("Camel Bonus Duration (Rate)")]
    [Tooltip("추가할 지속시간 비율 (예: 50 입력 시 기존 지속시간의 50% 추가)")]
    public int amount = 50;

    public override void ApplyTechEffect()
    {
        if (CamelEventSystem.instance == null)
        {
            Debug.LogWarning("[AddCamelBonusDurationRateEffect] CamelEventSystem 인스턴스를 찾을 수 없습니다.");
            return;
        }

        CamelEventSystem.instance.AddBonusDurationRate(amount);
        Debug.Log($"[AddCamelBonusDurationRateEffect] 낙타 보너스 지속시간 +{amount}% 증가");
    }

    public string GetDescription()
    {
        return $"낙타 보너스 지속시간 +{amount}%";
    }
}
