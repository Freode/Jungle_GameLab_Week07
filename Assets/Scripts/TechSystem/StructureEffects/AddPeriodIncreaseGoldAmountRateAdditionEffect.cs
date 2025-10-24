using UnityEngine;

[CreateAssetMenu(fileName = "AddPeriodRateIncreaseGoldAmountAdditionEffect", menuName = "Scriptable Objects/Structure Effect/Add Period Rate Increase Gold Addition Effect")]
public class AddPeriodIncreaseGoldAmountRateAdditionEffect : BaseStructureEffect
{
    public AreaType areaType;
    public long amount;
    public override string ApplyTechEffect()
    {
        GameManager.instance.IncreasePeriodRateGoldAcquirementAmount(areaType, amount);
        return name + $"+{amount}%";
    }
}
