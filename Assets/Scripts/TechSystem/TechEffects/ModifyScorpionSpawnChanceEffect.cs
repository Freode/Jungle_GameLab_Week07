using UnityEngine;

/// <summary>
/// 전갈 이벤트의 등장 확률을 변경하는 TechEffect
/// </summary>
[CreateAssetMenu(fileName = "ModifyScorpionSpawnChanceEffect", menuName = "Scriptable Objects/Tech Effect/Modify Scorpion Spawn Chance")]
public class ModifyScorpionSpawnChanceEffect : BaseTechEffect
{
    [Header("Scorpion Spawn Chance Settings")]
    [Tooltip("변경할 등장 확률 (0.001 = 0.1%, 0.01 = 1%, 0.1 = 10%)")]
    public float newSpawnChance = 0.001f;

    [Tooltip("기존 확률에 더할지(true) 또는 새 값으로 설정할지(false)")]
    public bool additive = false;

    public override void ApplyTechEffect()
    {
        if (ScorpionEventSystem.instance == null)
        {
            Debug.LogWarning("[ModifyScorpionSpawnChanceEffect] ScorpionEventSystem 인스턴스를 찾을 수 없습니다.");
            return;
        }

        if (additive)
        {
            ScorpionEventSystem.instance.AddSpawnChance(newSpawnChance);
            Debug.Log($"[ModifyScorpionSpawnChanceEffect] 전갈 등장 확률 증가: +{newSpawnChance * 100f}%");
        }
        else
        {
            ScorpionEventSystem.instance.SetSpawnChance(newSpawnChance);
            Debug.Log($"[ModifyScorpionSpawnChanceEffect] 전갈 등장 확률 설정: {newSpawnChance * 100f}%");
        }
    }

    public string GetDescription()
    {
        if (additive)
            return $"전갈 등장 확률 +{newSpawnChance * 100f:F2}%";
        else
            return $"전갈 등장 확률 {newSpawnChance * 100f:F2}%로 변경";
    }
}
