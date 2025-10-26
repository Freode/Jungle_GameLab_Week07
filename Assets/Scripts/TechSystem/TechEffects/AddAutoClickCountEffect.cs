using UnityEngine;

// 자동 클릭 횟수 증가
[CreateAssetMenu(fileName = "AddAutoClickCountEffect", menuName = "Scriptable Objects/Tech Effect/Add Auto Click Count")]
public class AddAutoClickCountEffect : BaseTechEffect
{
    public long amount = 0;

    public override void ApplyTechEffect()
    {
        GameManager.instance.IncreaseAutoClickCount(amount);
    }
}
