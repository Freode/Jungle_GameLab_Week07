using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GoldLogger : MonoBehaviour
{
    int accumulateSequence = 1;
    decimal accumulateNormalGoldAmount = 0;
    decimal accumulateAutoGoldAmount = 0;

    int acquireSequence = 1;

    private void Start()
    {
        StartCoroutine(UpdateAccumulateGoldLog());
        StartCoroutine(UpdateAcquireGoldLog());
    }

    IEnumerator UpdateAccumulateGoldLog()
    {
        // GameLogger.Instance.Log("AccumulateGold", $"====== Interval : 20s ======");
        while (true)
        {
            yield return new WaitForSeconds(20f);

            decimal totalGoldAmount = accumulateNormalGoldAmount + accumulateAutoGoldAmount;
            if (totalGoldAmount == 0) totalGoldAmount = 1;

            // GameLogger.Instance.Log("AccumulateGold", $"====== Sequence : {accumulateSequence}번 ======");
            GameLogger.Instance.Log("AccumulateGold", $"[Total_Gold_Accumulate:{FuncSystem.Format(totalGoldAmount)}] [Real_Value:{totalGoldAmount:F0}] [Rate:100.00%]");
            GameLogger.Instance.Log("AccumulateGold", $"[Normal_Gold_Accumulate:{FuncSystem.Format(accumulateNormalGoldAmount)}] [Real_Value:{accumulateNormalGoldAmount:F0}] [Rate:{accumulateNormalGoldAmount / totalGoldAmount * 100:F2}%]");
            GameLogger.Instance.Log("AccumulateGold", $"[Auto_Gold_Accumulate:{FuncSystem.Format(accumulateAutoGoldAmount)}] [Real_Value:{accumulateAutoGoldAmount:F0}] [Rate:{accumulateAutoGoldAmount / totalGoldAmount * 100:F2}%]");

            ++accumulateSequence;
        }
    }

    IEnumerator UpdateAcquireGoldLog()
    {
        // GameLogger.Instance.Log("AcquireGold", $"====== Interval : 5s ======");
        while (true)
        {
            yield return new WaitForSeconds(5f);

            decimal acquireClick = GameManager.instance.GetClickIncreaseTotalAmount();
            decimal acquireAuto = GameManager.instance.GetPeriodIncreaseTotalAmount();
            decimal totalAcquire = acquireAuto + acquireClick;
            if (totalAcquire == 0) totalAcquire = 1;

            // GameLogger.Instance.Log("AcquireGold", $"=======Sequence:{acquireSequence}번======");
            GameLogger.Instance.Log("AcquireGold", $"[Total_Gold_Accumulate:{FuncSystem.Format(totalAcquire)}] [Real_Value:{totalAcquire:F0}] [Rate:100.00%]");
            GameLogger.Instance.Log("AcquireGold", $"[Normal_Gold_Accumulate:{FuncSystem.Format(acquireClick)}] [Real_Value:{acquireClick:F0}] [Rate:{acquireClick / totalAcquire * 100:F2}%]");
            GameLogger.Instance.Log("AcquireGold", $"[Auto_Gold_Accumulate:{FuncSystem.Format(acquireAuto)}] [Real_Value:{acquireAuto:F0}] [Rate:{acquireAuto / totalAcquire * 100:F2}%]");

            ++acquireSequence;
        }
    }

    public void AcquireAutoGoldAmount(long amount)
    {
        accumulateAutoGoldAmount += amount;
    }

    public void AcquireNormalGoldAmount(long amount)
    {
        accumulateNormalGoldAmount += amount;
    }
}
