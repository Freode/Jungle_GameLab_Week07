using UnityEngine;

[CreateAssetMenu(fileName = "ShowDiffernetStructure", menuName = "Scriptable Objects/Tech Effect/Show Different Structure")]
// 레벨에 따라 다른 구조물 효과
public class ShowDifferentStructureEffect : BaseTechEffect
{
    public TechData techData;

    public override void ApplyTechEffect()
    {
        // GameManager.instance.ModifyStructureLevel(techData)
    }
}
