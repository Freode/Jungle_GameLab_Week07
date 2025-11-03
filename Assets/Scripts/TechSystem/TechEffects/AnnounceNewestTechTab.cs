using UnityEngine;

// 보상 쿨타임 효과
[CreateAssetMenu(fileName = "AnnounceNewestTechTab", menuName = "Scriptable Objects/Tech Effect/Announce Newest Tech Tab Effect")]
public class AnnounceNewestTechTab : BaseTechEffect
{
    public TechKind techKind;
    public override void ApplyTechEffect()
    {
        TechViewer.instance.AnnounceNewestTechOnTab(techKind);
    }
}
