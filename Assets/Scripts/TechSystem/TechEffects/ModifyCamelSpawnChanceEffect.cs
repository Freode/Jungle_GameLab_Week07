using UnityEngine;

/// <summary>
/// 낙타 이벤트의 등장 확률을 변경하는 TechEffect
/// </summary>
[CreateAssetMenu(fileName = "ModifyCamelSpawnChanceEffect", menuName = "Scriptable Objects/Tech Effect/Modify Camel Spawn Chance")]
public class ModifyCamelSpawnChanceEffect : BaseTechEffect
{
    [Header("Camel Spawn Chance Settings")]
    [Tooltip("변경할 등장 확률 (0.001 = 0.1%, 0.01 = 1%, 0.1 = 10%)")]
    public float newSpawnChance = 0.004f;

    [Tooltip("기존 확률에 더할지(true) 또는 새 값으로 설정할지(false)")]
    public bool additive = false;

    public override void ApplyTechEffect()
    {
        if (CamelEventSystem.instance == null)
        {
            Debug.LogWarning("[ModifyCamelSpawnChanceEffect] CamelEventSystem 인스턴스를 찾을 수 없습니다.");
            return;
        }

        if (additive)
        {
            CamelEventSystem.instance.AddSpawnChance(newSpawnChance);
            Debug.Log($"[ModifyCamelSpawnChanceEffect] 낙타 등장 확률 증가: +{newSpawnChance * 100f}%");
        }
        else
        {
            CamelEventSystem.instance.SetSpawnChance(newSpawnChance);
            Debug.Log($"[ModifyCamelSpawnChanceEffect] 낙타 등장 확률 설정: {newSpawnChance * 100f}%");
        }
    }

    public string GetDescription()
    {
        if (additive)
            return $"낙타 등장 확률 +{newSpawnChance * 100f:F2}%";
        else
            return $"낙타 등장 확률 {newSpawnChance * 100f:F2}%로 변경";
    }
}
