using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameClearEffect", menuName = "Scriptable Objects/Structure Effect/Game Clear Effect")]
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