using UnityEngine;

// 자동 클릭 간격 줄이는 효과
[CreateAssetMenu(fileName = "AddAutoClickIntervalEffect", menuName = "Scriptable Objects/Tech Effect/Add Auto Click Interval")]

public class AddAutoClickIntervalEffect : BaseTechEffect
{
    public float amount = 0f;
    public override void ApplyTechEffect()
    {
        GameManager.instance.IncreaseAutoClickInterval(amount);
    }
}
