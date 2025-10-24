using UnityEngine;

// 특정 테크의 최대 수용량(유사 최대 레벨) 업그레이드
[CreateAssetMenu(fileName = "AddMaxCapacityEffect", menuName = "Scriptable Objects/Tech Effect/Add Max Capacity")]
public class AddMaxCapacityEffect : BaseTechEffect
{
    public TechData targetTechData;
    public int amount = 0;

    public override void ApplyTechEffect()
    {
        GameManager.instance.ModifyMaxCapacityEffect(targetTechData, amount);
    }
}
