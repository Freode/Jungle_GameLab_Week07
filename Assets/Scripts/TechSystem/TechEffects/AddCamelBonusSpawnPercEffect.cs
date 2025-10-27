using UnityEngine;

[CreateAssetMenu(fileName = "AddCamelBonusSpawnPercEffect", menuName = "Scriptable Objects/Tech Effect/Add Camel Bonus Spawn Perc")]
public class AddCamelBonusSpawnPercEffect : BaseTechEffect
{
    public float amount = 0f;
    public override void ApplyTechEffect()
    {
        CamelEventSystem.instance.AddSpawnPerc(amount);
    }
}
