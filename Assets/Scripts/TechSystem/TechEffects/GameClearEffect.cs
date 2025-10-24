using UnityEngine;

// 게임 클리어 효과
[CreateAssetMenu(fileName = "GameClearEffect", menuName = "Scriptable Objects/Tech Effect/Game Clear Effect")]
public class GameClearEffect : BaseTechEffect
{
    public override void ApplyTechEffect()
    {
        // --- Logger Code ---
        string context = $"Timestamp: {System.DateTime.Now}";
        GameLogger.Instance.Log("game_clear_click", context);
        // --- End Logger Code ---

        GameManager.instance.SetIsGameOver(true);
    }
}
