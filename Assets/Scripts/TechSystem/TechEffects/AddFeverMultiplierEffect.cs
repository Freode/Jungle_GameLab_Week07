using UnityEngine;

[CreateAssetMenu(fileName = "AddFeverMultiplier", menuName = "Scriptable Objects/Tech Effect/Add Fever Multiplier")]
/// <summary>
/// 피버 타임 배율을 증가시키는 테크 효과
/// </summary>
public class AddFeverMultiplierEffect : BaseTechEffect
{
    [Tooltip("피버 타임 배율에 추가될 값")]
    public float amount = 0f; 

    public override void ApplyTechEffect()
    {
        AuthorityManager.instance.IncreaseFeverMultiplier(amount);
    }
}
