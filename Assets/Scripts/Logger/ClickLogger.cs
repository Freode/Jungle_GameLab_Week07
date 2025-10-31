using System.Collections;
using UnityEngine;

public class ClickLogger : MonoBehaviour
{
    int sequence = 1;
    decimal totalMouseClick = 0;
    decimal goldClick = 0;
    decimal interactClick = 0;
    decimal upgradeClick = 0;
    
    Camera _cam;
    Rect _screenRect;
    
    void Start()
    {
        _cam = Camera.main;
        _screenRect = new Rect(0, 0, Screen.width, Screen.height);

        StartCoroutine(UpdateClickLog());
        StartCoroutine(LogMousePositionRoutine());
    }


    void Update()
    {
        if (_screenRect.width != Screen.width || _screenRect.height != Screen.height)
            _screenRect = new Rect(0, 0, Screen.width, Screen.height);

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            totalMouseClick++;
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

            Vector3 mousePos = Input.mousePosition;
            Vector2 world2D;
            bool ok = TryGetMouseWorld2D(mousePos, out world2D);

            if (ok)
            {
                GameLogger.Instance.Log("MousePosition",
                    $"Screen:[{mousePos.x:F0},{mousePos.y:F0}]/World:[{world2D.x:F2},{world2D.y:F2}]");
            }
            else
            {
                GameLogger.Instance.Log("MousePosition",
                    $"Screen:[{mousePos.x:F0},{mousePos.y:F0}]/World:[invalid]");
            }
        }
    }

    // 클릭 발생 시 클릭 위치를 로그로 기록
    private void LogClickPosition(int button)
    {
        Vector3 mousePos = Input.mousePosition;
        Vector2 world2D;
        bool ok = TryGetMouseWorld2D(mousePos, out world2D);

        string buttonType = button == 0 ? "Left" : "Right";

        if (ok)
        {
            GameLogger.Instance.Log("ClickPosition",
                $"{buttonType}/Screen:[{mousePos.x:F0},{mousePos.y:F0}]/World:[{world2D.x:F2},{world2D.y:F2}]");
        }
        else
        {
            GameLogger.Instance.Log("ClickPosition",
                $"{buttonType}/Screen:[{mousePos.x:F0},{mousePos.y:F0}]/World:[invalid]");
        }
    }
    
    // 안전한 월드 좌표 변환
    bool TryGetMouseWorld2D(Vector3 screenMousePos, out Vector2 world2D)
    {
        world2D = default;

        if (_cam == null || !_cam.isActiveAndEnabled) return false;

        if (!_screenRect.Contains(new Vector2(screenMousePos.x, screenMousePos.y)))
            return false;

        float worldPlaneZ = 0f;
        float distance = worldPlaneZ - _cam.transform.position.z;
        if (distance <= 0f) distance = Mathf.Abs(distance);

        Vector3 sp = new Vector3(screenMousePos.x, screenMousePos.y, distance);

        if (float.IsNaN(sp.x) || float.IsNaN(sp.y) || float.IsNaN(sp.z) ||
            float.IsInfinity(sp.x) || float.IsInfinity(sp.y) || float.IsInfinity(sp.z))
            return false;

        Vector3 world = _cam.ScreenToWorldPoint(sp);
        if (float.IsNaN(world.x) || float.IsNaN(world.y)) return false;

        world2D = new Vector2(world.x, world.y);
        return true;
    }

}
