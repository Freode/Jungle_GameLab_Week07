using UnityEngine;

[CreateAssetMenu(fileName = "AddPeriodGoldAmountLinear", menuName = "Scriptable Objects/Tech Effect/Add Period Gold Amount Linear")]
// 주기적으로 얻는 금의 양이 선형적 증가 효과
public class AddPeriodIncreaseGoldAmountLinearEffect : BaseTechEffect
{
    public AreaType type;
    public int amount = 0;
    public override void ApplyTechEffect()
    {
        GameManager.instance.SetPeriodIncreaseGoldAmountLinear(type, amount);
    }
}
