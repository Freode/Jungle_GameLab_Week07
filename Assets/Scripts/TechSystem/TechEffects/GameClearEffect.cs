using UnityEngine;

// 게임 클리어 효과
[CreateAssetMenu(fileName = "GameClearEffect", menuName = "Scriptable Objects/Tech Effect/Game Clear Effect")]
public class GameClearEffect : BaseTechEffect
{
    public override void ApplyTechEffect()
    {
        GameManager.instance.SetIsGameOver(true);
    }
}
