using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FinishGameEffect", menuName = "Scriptable Objects/Structure Effect/Finish Game Effect")]
public class FinishGame : BaseStructureEffect
{
    public Action OnEvent;

    public override string ApplyTechEffect()
    {
        OnEvent?.Invoke();
        return string.Empty;
    }
}
