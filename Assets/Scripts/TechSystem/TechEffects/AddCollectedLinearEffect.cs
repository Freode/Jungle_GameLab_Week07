using UnityEngine;

// 특정 구역의 1인당 징수금 올리는 효과
[CreateAssetMenu(fileName = "AddCollectedLinearEffect", menuName = "Scriptable Objects/Tech Effect/Add Collected Linear Effect")]
public class AddCollectedLinearEffect : BaseTechEffect
{
    public AreaType areaType;
    public long amount;
    public override void ApplyTechEffect()
    {
        GameManager.instance.IncreaseCollectedAmount(areaType, amount);
    }
}
