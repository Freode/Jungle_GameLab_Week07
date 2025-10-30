using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TechPossibleEffect", menuName = "Scriptable Objects/Structure Effect/Tech Possible Effect")]
public class GameClearEffectInStructure : BaseStructureEffect
{
    public Action<TechKind, TechData> OnEvent;
    public TechKind techKind;
    public TechData techData;

    public override string ApplyTechEffect()
    {
        OnEvent?.Invoke(techKind, techData);
        return string.Empty;
    }
}