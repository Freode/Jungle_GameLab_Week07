using UnityEngine;

[CreateAssetMenu(fileName = "AddLifeRateEffect", menuName = "Scriptable Objects/Structure Effect/Add Life Rate Effect")]
public class AddLifeRateEffect : BaseStructureEffect
{
    public float amount;

    public override string ApplyTechEffect()
    {
        GameManager.instance.IncreaseAdditionalLifeRate(amount);
        return $"캐릭터 생존 확률 +{amount}%";
    }
}
