using UnityEngine;

public class SpecialUpgradeLogger : MonoBehaviour
{
    private const string LOG_FILE_NAME = "SpecialUpgrade";

    /// <summary>
    /// 특수 업그레이드 로그 기록
    /// </summary>
    /// <param name="techName">업그레이드한 기술 이름</param>
    /// <param name="level">업그레이드 후 레벨</param>
    /// <param name="cost">업그레이드 비용</param>
    public void LogUpgrade(string techName, int level, long cost)
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, $"Upgraded/{techName}/Level:{level}/Cost:{cost}");
    }

    /// <summary>
    /// 고양이 신 소환 로그
    /// </summary>
    /// <param name="catGodType">고양이 신 타입</param>
    public void LogCatGodSpawn(string catGodType)
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, $"CatGodSpawned/{catGodType}");
    }
}
