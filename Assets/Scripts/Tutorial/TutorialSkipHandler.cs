using UnityEngine;

public class TutorialSkipHandler : MonoBehaviour
{
    [Tooltip("스킵 버튼을 눌렀을 때 비활성화할 튜토리얼 패널 오브젝트를 할당해주세요.")]
    public GameObject tutorialPanel;

    public void SkipTutorial()
    {
        // 멈춰있던 게임 시간을 다시 흐르게 합니다.
        Time.timeScale = 1.0f;

        // 할당된 튜토리얼 패널이 있다면 비활성화합니다.
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("TutorialSkipHandler: 튜토리얼 패널이 할당되지 않았습니다!");
        }

        Debug.Log("Tutorial Skipped and Time Scale is now 1.0");
    }
}
