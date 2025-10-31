using UnityEngine;

// 호버 주기 감소
[CreateAssetMenu(fileName = "AddHoverIntervalDown", menuName = "Scriptable Objects/Tech Effect/Add Hover Interval Down")]
public class AddHoverIntervalDown : BaseTechEffect
{
    public float amount = 0;

    public override void ApplyTechEffect()
    {
        GameManager.instance.AddHoverPeriod(amount);
    }
}
