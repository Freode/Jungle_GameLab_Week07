using UnityEngine;

// 권위 경험치 배율 증가
[CreateAssetMenu(fileName = "AddAuthorityExpMultipleEffect", menuName = "Scriptable Objects/Tech Effect/Add Authority Exp Multiple Effect")]
public class AddAuthorityExpMultipleEffect : BaseTechEffect
{
    public long amount = 0;
    public override void ApplyTechEffect()
    {
        GameManager.instance.AddIncreaseAuthorityExpMultiple(amount);
    }
}
