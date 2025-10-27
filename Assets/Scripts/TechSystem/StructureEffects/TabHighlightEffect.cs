using UnityEngine;

[CreateAssetMenu(fileName = "TabHighlightEffect", menuName = "Scriptable Objects/Structure Effect/Tab Highlight Effect")]
public class TabHighlightEffect : BaseStructureEffect
{
    [Header("하이라이트할 탭 선택")]
    public TechKind targetTab = TechKind.Job;

    public override string ApplyTechEffect()
    {
        // 탭 하이라이트 활성화
        if (TechViewer.instance != null)
        {
            TechViewer.instance.ActivateTabHighlight(targetTab);
        }
        
        return $"{GetTabName(targetTab)} 탭 하이라이트 활성화";
    }

    private string GetTabName(TechKind techKind)
    {
        switch (techKind)
        {
            case TechKind.Structure:
                return "건물";
            case TechKind.Job:
                return "일꾼";
            case TechKind.Special:
                return "특수";
            default:
                return "알 수 없음";
        }
    }
}
