using UnityEngine;

// 1인당 벌어들이는 세금을 증가
[CreateAssetMenu(fileName = "AddClickIncreaseGoldAmountLinearAdditionalEffect", menuName = "Scriptable Objects/Tech Effect/Add Click Increase Gold Amount Linear Additional")]
public class AddClickIncreaseGoldAmountLinearAdditionalEffect : BaseTechEffect
{
    public AreaType areaType;
    public long amount;

    public override void ApplyTechEffect()
    {
        GameManager.instance.IncreaseClickLinearGoldAcquirementAmount(areaType, amount);
    }
}
