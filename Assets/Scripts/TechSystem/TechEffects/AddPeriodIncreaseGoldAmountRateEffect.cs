using UnityEngine;

[CreateAssetMenu(fileName = "AddPeriodGoldAmountRate", menuName = "Scriptable Objects/Tech Effect/Add Period Gold Amount Rate")]
public class AddPeriodIncreaseGoldAmountRateEffect : BaseTechEffect
{
    public AreaType type;
    public int amount = 0;

    public override void ApplyTechEffect()
    {
        GameManager.instance.AddPeriodIncreaseGoldAmountRate(type, amount);
    }
}
