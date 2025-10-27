using UnityEngine;

public class CamelLogger : MonoBehaviour
{
    private const string LOG_FILE_NAME = "CamelEvent";

    public void LogSpawn()
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, "Spawned");
    }

    public void LogDefeated(int clicks, long goldGained, int multiplier)
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, $"DefeatedByInteraction/Clicks:{clicks}/GoldGained:{goldGained}/Multiplier:{multiplier}");
    }

    public void LogDisappeared()
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, "DisappearedWithoutInteraction");
    }
}
