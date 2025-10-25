using UnityEngine;

/// <summary>
/// 낙타 보너스 지속시간을 증가시키는 TechEffect (선형)
/// </summary>
[CreateAssetMenu(fileName = "AddCamelBonusDurationLinear", menuName = "Scriptable Objects/Tech Effect/Add Camel Bonus Duration Linear")]
public class AddCamelBonusDurationLinearEffect : BaseTechEffect
{
    [Header("Camel Bonus Duration (Linear)")]
    [Tooltip("추가할 지속시간 (초 단위, 예: 5 입력 시 +5초)")]
    public float amount = 5f;

    public override void ApplyTechEffect()
    {
        if (CamelEventSystem.instance == null)
        {
            Debug.LogWarning("[AddCamelBonusDurationLinearEffect] CamelEventSystem 인스턴스를 찾을 수 없습니다.");
            return;
        }

        CamelEventSystem.instance.AddBonusDuration(amount);
        Debug.Log($"[AddCamelBonusDurationLinearEffect] 낙타 보너스 지속시간 +{amount}초 증가");
    }

    public string GetDescription()
    {
        return $"낙타 보너스 지속시간 +{amount}초";
    }
}
