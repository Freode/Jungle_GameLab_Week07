using UnityEngine;

[CreateAssetMenu(fileName = "AddRespawnUselessPeople", menuName = "Scriptable Objects/Tech Effect/Add Respawn Useless People")]
// 주기적으로 얻는 금의 양이 선형적 증가 효과
public class AddRespawnUselessPeopleEffect : BaseTechEffect
{
    public float amount = 0f;
    public override void ApplyTechEffect()
    {
        GameManager.instance.ModifyRespawnUselessPeople(amount);
    }
}
