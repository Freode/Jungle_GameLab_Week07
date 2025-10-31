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
        StartCoroutine(LogMousePositionRoutine());
    }

    private void Update()
    {
        // 0 = 좌클릭, 1 = 우클릭
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            totalMouseClick++;
            
            // 클릭 위치 로그 기록
            LogClickPosition(Input.GetMouseButtonDown(0) ? 0 : 1);
        }
    }

    IEnumerator UpdateClickLog()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);

            if (totalMouseClick == 0) totalMouseClick = 1;

            // GameLogger.Instance.Log("Click", $"====== Sequence : {sequence}번 ======");
            GameLogger.Instance.Log("Click", $"[Total_Mouse_Click:{totalMouseClick}] [Rate:100%]");
            GameLogger.Instance.Log("Click", $"[Gold_Mouse_Click:{goldClick}] [Rate:{goldClick / totalMouseClick * 100:F2}%]");
            GameLogger.Instance.Log("Click", $"[Interact_Mouse_Click:{interactClick}] [Rate:{interactClick / totalMouseClick * 100:F2}%]");
            GameLogger.Instance.Log("Click", $"[Upgrade_Mouse_Click:{upgradeClick}] [Rate:{upgradeClick / totalMouseClick * 100:F2}%]");

            ++sequence;
        }
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

    // 1초마다 마우스 위치를 로그로 기록
    IEnumerator LogMousePositionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // 스크린 좌표로 마우스 위치 기록
            Vector3 mousePos = Input.mousePosition;
            
            // 월드 좌표도 함께 기록 (카메라 기준)
            Vector3 worldPos = Camera.main != null 
                ? Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane))
                : Vector3.zero;

            GameLogger.Instance.Log("MousePosition", 
                $"Screen:[{mousePos.x:F0},{mousePos.y:F0}]/World:[{worldPos.x:F2},{worldPos.y:F2}]");
        }
    }

    // 클릭 발생 시 클릭 위치를 로그로 기록
    private void LogClickPosition(int button)
    {
        Vector3 mousePos = Input.mousePosition;
        
        // 월드 좌표 변환
        Vector3 worldPos = Camera.main != null 
            ? Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane))
            : Vector3.zero;

        string buttonType = button == 0 ? "Left" : "Right";
        
        GameLogger.Instance.Log("ClickPosition", 
            $"{buttonType}/Screen:[{mousePos.x:F0},{mousePos.y:F0}]/World:[{worldPos.x:F2},{worldPos.y:F2}]");
    }
}
