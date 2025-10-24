using System.Collections;
using UnityEngine;

public class ClickLogger : MonoBehaviour
{
    int sequence = 1;
    decimal totalMouseClick = 0;
    decimal goldClick = 0;
    decimal interactClick = 0;
    decimal upgradeClick = 0;

    private void Start()
    {
        StartCoroutine(UpdateClickLog());
    }

    private void Update()
    {
        // 0 = 좌클릭, 1 = 우클릭
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            totalMouseClick++;
        }
    }

    IEnumerator UpdateClickLog()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);

            if (totalMouseClick == 0) totalMouseClick = 1;

            GameLogger.Instance.Log("Click", $"====== Sequence : {sequence}번 ======");
            GameLogger.Instance.Log("Click", $"[Total Mouse Click : {totalMouseClick}] [Rate : 100%]");
            GameLogger.Instance.Log("Click", $"[Gold Mouse Click : {goldClick}] [Rate : {goldClick / totalMouseClick * 100:F2}%]");
            GameLogger.Instance.Log("Click", $"[Interact Mouse Click : {interactClick}] [Rate : {interactClick / totalMouseClick * 100:F2}%]");
            GameLogger.Instance.Log("Click", $"[Upgrade Mouse Click : {upgradeClick}] [Rate : {upgradeClick / totalMouseClick * 100:F2}%]");
            ++sequence;
        }
    }

    private void OnDisable()
    {
        
    }

    public void AddGoldClick()
    {
        goldClick++;
    }

    public void AddInteractClick()
    {
        interactClick++;
    }

    public void AddUpgradeClick()
    {
        upgradeClick++;
    }
}
