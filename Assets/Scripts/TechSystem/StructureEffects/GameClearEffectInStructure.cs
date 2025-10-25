using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameClearEffect", menuName = "Scriptable Objects/Structure Effect/Game Clear Effect")]
public class GameClearEffectInStructure : BaseStructureEffect
{
    public Action<TechData> OnEvent;
    public TechData techData;

    public override string ApplyTechEffect()
    {
        OnEvent?.Invoke(techData);
        return string.Empty;
    }
}