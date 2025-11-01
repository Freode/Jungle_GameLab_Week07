using UnityEngine;

// PitHouse에서 백수(Worker) 캐릭터를 즉시 스폰하는 효과
[CreateAssetMenu(fileName = "SpawnUselessEffect", menuName = "Scriptable Objects/Tech Effect/Spawn Useless Effect")]
public class SpawnUselessEffect : BaseTechEffect
{
    [Header("Spawn Settings")]
    [Tooltip("스폰할 캐릭터 수")]
    public int spawnCount = 1;

    [Header("Character Settings")]
    [Tooltip("스폰할 캐릭터의 지역 타입 (기본값: Gold)")]
    public AreaType targetAreaType = AreaType.Gold;

    [Tooltip("스폰할 캐릭터의 직업 타입 (기본값: Worker)")]
    public JobType targetJobType = JobType.Worker;

    public override void ApplyTechEffect()
    {
        // PitHouse 찾기
        PitHouse pitHouse = Object.FindFirstObjectByType<PitHouse>();
        
        if (pitHouse == null)
        {
            Debug.LogWarning("[SpawnUselessEffect] PitHouse를 찾을 수 없습니다!");
            return;
        }

        // 지정된 횟수만큼 스폰
        for (int i = 0; i < spawnCount; i++)
        {
            pitHouse.ForceSpawnOnce();
        }

        Debug.Log($"[SpawnUselessEffect] {spawnCount}명의 {targetJobType} 캐릭터를 {targetAreaType} 지역에 스폰했습니다.");
    }
}
