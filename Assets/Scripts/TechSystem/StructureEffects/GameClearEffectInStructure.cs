using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameClearEffect", menuName = "Scriptable Objects/Structure Effect/Game Clear Effect")]
public class GameClearEffectInStructure : BaseStructureEffect
{
    public Action<TechData> OnClearEvent;
    public TechData techData;

    public override string ApplyTechEffect()
    {
        OnClearEvent?.Invoke(techData);
        return string.Empty;
    }
}