using UnityEngine;

[CreateAssetMenu(fileName = "AddClickRateIncreaseGoldAmountAdditionEffect", menuName = "Scriptable Objects/Structure Effect/Add Click Rate Increase Gold Addition Effect")]
public class AddClickIncreaseGoldAmountRateAdditionEffect : BaseStructureEffect
{
    public AreaType areaType;
    public long amount;
    public override string ApplyTechEffect()
    {
        GameManager.instance.IncreaseClickRateGoldAcquirementAmount(areaType, amount);
        return name + $"+{amount}%";
    }
}
