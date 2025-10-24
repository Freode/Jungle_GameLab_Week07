using UnityEngine;

[CreateAssetMenu(fileName = "AddClickLinearIncreaseGoldAmountAdditionEffect", menuName = "Scriptable Objects/Structure Effect/Add Click Linear Increase Gold Amount Addition Effect")]

public class AddClickIncreaseGoldAmountLinearAdditionEffect : BaseStructureEffect
{
    public AreaType areaType;
    public long amount;
    public override string ApplyTechEffect()
    {
        GameManager.instance.IncreaseClickLinearGoldAcquirementAmount(areaType, amount);
        return name + $"+{amount}";
    }
}
