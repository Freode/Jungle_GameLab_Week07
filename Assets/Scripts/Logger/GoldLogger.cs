using System.Collections;
using UnityEngine;

public class GoldLogger : MonoBehaviour
{
    int sequence = 1;
    decimal acquireNormalGoldAmount = 0;
    decimal acuqireAutoGoldAmount = 0;

    private void Start()
    {
        StartCoroutine(UpdateGoldLog());
    }

    IEnumerator UpdateGoldLog()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);

            decimal totalGoldAmount = acquireNormalGoldAmount + acuqireAutoGoldAmount;
            if (totalGoldAmount == 0) totalGoldAmount = 1;

            GameLogger.Instance.Log("GoldAcquirement", $"====== Sequence : {sequence}번 ======");
            GameLogger.Instance.Log("GoldAcquirement", $"[Total Gold Acquirement : {FuncSystem.Format(totalGoldAmount)}] [Real Value : {totalGoldAmount:F0}] [Rate : 100%]");
            GameLogger.Instance.Log("GoldAcquirement", $"[Normal Gold Acquirement : {FuncSystem.Format(acquireNormalGoldAmount)}] [Real Value : {acquireNormalGoldAmount:F0}] [Rate : {acquireNormalGoldAmount / totalGoldAmount * 100:F2}%]");
            GameLogger.Instance.Log("GoldAcquirement", $"[Auto Gold Acquirement : {FuncSystem.Format(acuqireAutoGoldAmount)}] [Real Value : {acuqireAutoGoldAmount:F0}] [Rate : {acuqireAutoGoldAmount / totalGoldAmount * 100:F2}%]");

            ++sequence;
        }
    }

    public void AcquireAutoGoldAmount(long amount)
    {
        acuqireAutoGoldAmount += amount;
    }

    public void AcquireNormalGoldAmount(long amount)
    {
        acquireNormalGoldAmount += amount;
    }
}
