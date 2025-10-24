using UnityEngine;

[CreateAssetMenu(fileName = "AddPeriodLinearIncreaseGoldAmountAdditionEffect", menuName = "Scriptable Objects/Structure Effect/Add Period Linear Increase Gold Addition Effect")]

public class AddPeriodIncreaseGoldAmountLinearAdditionEffect : BaseStructureEffect
{
    public AreaType areaType;
    public long amount;
    public override string ApplyTechEffect()
    {
        GameManager.instance.IncreasePeriodLinearGoldAcquirementAmount(areaType, amount);
        return name + $"+{amount}";
    }
}
