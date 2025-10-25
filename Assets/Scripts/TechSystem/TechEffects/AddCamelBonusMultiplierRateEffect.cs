using UnityEngine;

/// <summary>
/// 낙타 보너스 배수를 비율로 증가시키는 TechEffect
/// </summary>
[CreateAssetMenu(fileName = "AddCamelBonusMultiplierRate", menuName = "Scriptable Objects/Tech Effect/Add Camel Bonus Multiplier Rate")]
public class AddCamelBonusMultiplierRateEffect : BaseTechEffect
{
    [Header("Camel Bonus Multiplier (Rate)")]
    [Tooltip("추가할 보너스 배수 비율 (예: 50 입력 시 기존 배수의 50% 추가)")]
    public int amount = 50;

    public override void ApplyTechEffect()
    {
        if (CamelEventSystem.instance == null)
        {
            Debug.LogWarning("[AddCamelBonusMultiplierRateEffect] CamelEventSystem 인스턴스를 찾을 수 없습니다.");
            return;
        }

        CamelEventSystem.instance.AddBonusMultiplierRate(amount);
        Debug.Log($"[AddCamelBonusMultiplierRateEffect] 낙타 보너스 배수 +{amount}% 증가");
    }

    public string GetDescription()
    {
        return $"낙타 보너스 배수 +{amount}%";
    }
}
