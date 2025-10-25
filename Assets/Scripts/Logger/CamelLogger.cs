using UnityEngine;

public class CamelLogger : MonoBehaviour
{
    private const string LOG_FILE_NAME = "CamelStats";

    public void LogSpawn()
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, "Spawned");
    }

    public void LogDefeated()
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, "DefeatedByInteraction");
    }

    public void LogDisappeared()
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, "DisappearedWithoutInteraction");
    }
}
