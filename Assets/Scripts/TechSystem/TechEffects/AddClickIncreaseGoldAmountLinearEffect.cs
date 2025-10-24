using UnityEngine;

[CreateAssetMenu(fileName = "AddClickGoldAmountLinear", menuName = "Scriptable Objects/Tech Effect/Add Click Gold Amount Linear")]
// 한 번 클릭할 때, 얻는 금의 양 선형적 증가 효과
public class AddClickIncreaseGoldAmountLinearEffect : BaseTechEffect
{
    public AreaType type;
    public int amount = 0;
    public override void ApplyTechEffect()
    {
        GameManager.instance.AddClickIncreaseGoldAmountLinear(type, amount);
    }
}
