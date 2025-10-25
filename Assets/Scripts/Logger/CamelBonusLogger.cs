using UnityEngine;

public class CamelBonusLogger : MonoBehaviour
{
    private const string LOG_FILE_NAME = "CamelBonusResult";

    public void LogBonusResult(int clicks, long goldGained)
    {
        GameLogger.Instance.Log(LOG_FILE_NAME, $"BonusResult/Clicks:{clicks}/GoldGained:{goldGained}");
    }
}
