using UnityEngine;

[CreateAssetMenu(fileName = "AddClickGoldAmountRate", menuName = "Scriptable Objects/Tech Effect/Add Click Gold Amount Rate")]
// 한 번 클릭할 때, 얻는 금의 양이 비율로 증가 효과
public class AddClickIncreaseGoldAmountRateEffect : BaseTechEffect
{
    public AreaType type;
    public int amount = 0;
    public override void ApplyTechEffect()
    {
        GameManager.instance.AddClickIncreaseGoldAmountRate(type, amount);
    }
}
