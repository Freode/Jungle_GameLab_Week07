using UnityEngine;

[CreateAssetMenu(fileName = "AddFeverCount", menuName = "Scriptable Objects/Tech Effect/Add Fever Count")]
public class AddFeverCountEffect : BaseTechEffect
{

    public float amount = 0f;

    public override void ApplyTechEffect()
    {
        AuthorityManager.instance.IncreaseAddIncreaseAmount(amount);
    }
}
