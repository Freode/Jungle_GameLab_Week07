using UnityEngine;

public class ScorpionLogger : MonoBehaviour
{
    private const string LOG_FILE_NAME = "ScorpionStats";

    public void LogSpawn()
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, "Spawned");
    }

    public void LogActiveTime(float activeTime)
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, $"ActiveTime:{activeTime:F2}s");
    }

    public void LogDefeatTime(float timeToDefeat)
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, $"DefeatTime:{timeToDefeat:F2}s");
    }

    public void LogGoldStolen(long goldStolen)
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, $"GoldStolen:{goldStolen}");
    }

    public void LogGoldReturned(long goldReturned)
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, $"GoldReturned:{goldReturned}");
    }
}
