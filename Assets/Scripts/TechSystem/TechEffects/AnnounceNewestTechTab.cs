using UnityEngine;

// 보상 쿨타임 효과
[CreateAssetMenu(fileName = "AnnounceNewestTechTab", menuName = "Scriptable Objects/Tech Effect/Announce Newest Tech Tab Effect")]
public class AnnounceNewestTechTab : BaseTechEffect
{
    public TechKind techKind;
    public bool isOnce;
    private bool _isOnce = true;

    public override void ApplyTechEffect()
    {
        if(isOnce)
        {
            if(_isOnce)
            {
                _isOnce = false;
                TechViewer.instance.ActivateTabHighlight(techKind);
                TechViewer.instance.AnnounceNewestTechOnTab(techKind);
            }
        }
        else
        {
            TechViewer.instance.ActivateTabHighlight(techKind);
            TechViewer.instance.AnnounceNewestTechOnTab(techKind);
        }
    }
}
