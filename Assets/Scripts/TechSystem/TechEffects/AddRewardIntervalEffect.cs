using UnityEngine;

// 보상 쿨타임 효과
[CreateAssetMenu(fileName = "AddRewardIntervalEffect", menuName = "Scriptable Objects/Tech Effect/Add Reward Interval Effect")]
public class AddRewardIntervalEffect : BaseTechEffect
{
    public float amount = 0f;

    public override void ApplyTechEffect()
    {
        GameManager.instance.AddRewardInterva(amount);
    }
}
