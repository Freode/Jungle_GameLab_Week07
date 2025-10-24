using UnityEngine;

// 처음으로 기술을 열었을 때, 효과
[CreateAssetMenu(fileName = "UnlockStructureEffect", menuName = "Scriptable Objects/Tech Effect/Unlock Structure Effect")]
public class UnlockStructureEffect : BaseTechEffect
{
    public AreaType areaType;
    public override void ApplyTechEffect()
    {
        GameManager.instance.UnlockStructure(areaType);
    }
}
