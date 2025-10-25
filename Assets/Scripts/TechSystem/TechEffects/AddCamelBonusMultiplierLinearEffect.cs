using UnityEngine;

/// <summary>
/// 낙타 보너스 배수를 증가시키는 TechEffect (선형)
/// </summary>
[CreateAssetMenu(fileName = "AddCamelBonusMultiplierLinear", menuName = "Scriptable Objects/Tech Effect/Add Camel Bonus Multiplier Linear")]
public class AddCamelBonusMultiplierLinearEffect : BaseTechEffect
{
    [Header("Camel Bonus Multiplier (Linear)")]
    [Tooltip("추가할 보너스 배수 (예: 5 입력 시 기존 배수에 +5)")]
    public int amount = 5;

    public override void ApplyTechEffect()
    {
        if (CamelEventSystem.instance == null)
        {
            Debug.LogWarning("[AddCamelBonusMultiplierLinearEffect] CamelEventSystem 인스턴스를 찾을 수 없습니다.");
            return;
        }

        CamelEventSystem.instance.AddBonusMultiplier(amount);
        Debug.Log($"[AddCamelBonusMultiplierLinearEffect] 낙타 보너스 배수 +{amount} 증가");
    }

    public string GetDescription()
    {
        return $"낙타 보너스 배수 +{amount}";
    }
}
