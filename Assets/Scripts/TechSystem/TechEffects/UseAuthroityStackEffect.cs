using UnityEngine;

// 권능 스택(스킬 포인트) 사용
[CreateAssetMenu(fileName = "UseAuthorityStackEffect", menuName = "Scriptable Objects/Tech Effect/Use Authroity Stack Effect")]
public class UseAuthroityStackEffect : BaseTechEffect
{
    public TechKind techKind;
    public TechData techData;

    public override void ApplyTechEffect()
    {
        GameManager.instance.UseAuthorityLevelStack(techKind, techData);
    }
}
