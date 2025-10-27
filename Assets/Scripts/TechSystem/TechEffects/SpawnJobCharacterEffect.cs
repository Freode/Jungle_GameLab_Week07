using UnityEngine;

[CreateAssetMenu(fileName = "SpawnJobCharacterEffect", menuName = "Scriptable Objects/Tech Effect/Spawn Job Character Effect")]
public class SpawnJobCharacterEffect : BaseTechEffect
{
    [Header("Target Job")]
    [Tooltip("소환할 캐릭터의 직업")]
    [SerializeField] private JobType targetJobType;
    
    [Header("Target Area (use job default when Normal)")]
    [Tooltip("지정하면 해당 영역으로 배치; Normal이면 직업의 기본 영역 사용")]
    [SerializeField] private AreaType targetArea = AreaType.Normal;
    
    [Header("Spawn Count")]
    [Tooltip("소환할 캐릭터 수")]
    [SerializeField] private int spawnCount = 1;

    public override void ApplyTechEffect()
    {
        // 지정된 수만큼 캐릭터 소환
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnCharacterWithJob();
        }
        
        Debug.Log($"{targetJobType} 직업을 가진 캐릭터 {spawnCount}명을 소환했습니다.");
    }

    private void SpawnCharacterWithJob()
    {
        // PeopleManager를 통해 새로운 캐릭터를 소환
        GameObject newCharacter = PeopleManager.Instance.SpawnSpecialCharacter(targetJobType);
        
        if (newCharacter == null)
        {
            Debug.LogWarning("캐릭터 소환에 실패했습니다.");
            return;
        }

        // 목표 영역이 Normal이면 직업에 맞는 기본 영역 사용, 아니면 지정된 영역 사용
        AreaType finalArea = (targetArea == AreaType.Normal) ? GetDefaultAreaForJob(targetJobType) : targetArea;

        // 소환된 캐릭터를 지정된 영역과 직업으로 이동
        switch (targetJobType)
        {
            case JobType.Worker:
                PeopleManager.Instance.MoveToArea(newCharacter, finalArea, JobType.Worker);
                break;
            case JobType.Miner:
                PeopleManager.Instance.MoveToArea(newCharacter, finalArea, JobType.Miner);
                break;
            case JobType.Carver:
                PeopleManager.Instance.MoveToArea(newCharacter, finalArea, JobType.Carver);
                break;
            case JobType.Carrier:
                PeopleManager.Instance.CheckUnlockArea();
                PeopleManager.Instance.MoveToArea(newCharacter, finalArea, JobType.Carrier);
                break;
            case JobType.Architect:
                PeopleManager.Instance.MoveToArea(newCharacter, finalArea, JobType.Architect);
                break;
            case JobType.God:
                PeopleManager.Instance.MoveToArea(newCharacter, finalArea, JobType.God);
                break;
            case JobType.None:
            default:
                // 기본적으로 Normal 영역에 배치
                PeopleManager.Instance.MoveToArea(newCharacter, AreaType.Normal, JobType.None);
                break;
        }
    }

    /// <summary>각 직업에 맞는 기본 영역을 반환합니다.</summary>
    private AreaType GetDefaultAreaForJob(JobType jobType)
    {
        return jobType switch
        {
            JobType.Worker => AreaType.Gold,
            JobType.Miner => AreaType.Mine,
            JobType.Carver => AreaType.StoneCarving,
            JobType.Carrier => AreaType.Carrier,
            JobType.Architect => AreaType.Architect,
            JobType.God => AreaType.Special,
            _ => AreaType.Normal
        };
    }
}