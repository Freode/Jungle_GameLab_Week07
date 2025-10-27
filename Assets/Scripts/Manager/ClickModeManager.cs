
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum ClickMode
{
    Heart,
    Hit
}

public class ClickModeManager : MonoBehaviour
{
    public static ClickModeManager Instance { get; private set; }

    [Header("Buttons")]
    public Button heartButton;
    public Button hitButton;

    public ClickMode CurrentMode { get; private set; } = ClickMode.Heart; // 기본 모드를 Heart로 설정

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작 시 기본 모드인 Heart 버튼을 선택된 상태로 만듭니다.
        SetHeartMode();
    }

    private void Update()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // 현재 선택된 오브젝트가 Heart 또는 Hit 버튼이 아니면, 강제로 선택을 되돌립니다.
        if (currentSelected != heartButton.gameObject && currentSelected != hitButton.gameObject)
        {
            if (CurrentMode == ClickMode.Heart)
            {
                EventSystem.current.SetSelectedGameObject(heartButton.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(hitButton.gameObject);
            }
        }
    }

    public void SetHeartMode()
    {
        CurrentMode = ClickMode.Heart;
        EventSystem.current.SetSelectedGameObject(heartButton.gameObject);
        Debug.Log("Click Mode changed to: Heart");
    }

    public void SetHitMode()
    {
        CurrentMode = ClickMode.Hit;
        EventSystem.current.SetSelectedGameObject(hitButton.gameObject);
        Debug.Log("Click Mode changed to: Hit");
    }
}
