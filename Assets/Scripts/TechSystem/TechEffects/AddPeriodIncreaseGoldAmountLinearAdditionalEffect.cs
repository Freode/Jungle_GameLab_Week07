using UnityEngine;

// 1인당 벌어들이는 세금을 증가
[CreateAssetMenu(fileName = "AddPeriodIncreaseGoldAmountLinearAdditionalEffect", menuName = "Scriptable Objects/Tech Effect/Add Period Increase Gold Amount Linear Additional")]
public class AddPerriodIncreaseGoldAmountLinearAdditionalEffect : BaseTechEffect
{
    public AreaType areaType;
    public long amount;

    public override void ApplyTechEffect()
    {
        GameManager.instance.IncreasePeriodLinearGoldAcquirementAmount(areaType, amount);
    }
}
