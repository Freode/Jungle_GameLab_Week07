using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class TutorialPage
{
    [Header("Tutorial Image")]
    public Sprite pageImage;
    [Header("Button Position (Anchored)")]
    public Vector2 buttonAnchoredPosition = Vector2.zero;
}

public class Tutorial : MonoBehaviour
{
    [Header("Tutorial Page List")]
    public List<TutorialPage> pages = new List<TutorialPage>();

    [Header("UI References")]
    public Image tutorialImage;
    public Button nextButton;

    private int currentPage = 0;

    private float prevTimeScale = 1f;

    private void Start()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        ShowPage(0);
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }

    private void ShowPage(int pageIndex)
    {
        if (pages == null || pages.Count == 0 || pageIndex < 0 || pageIndex >= pages.Count)
        {
            EndTutorial();
            return;
        }

        currentPage = pageIndex;
        var page = pages[pageIndex];
        if (tutorialImage != null)
            tutorialImage.sprite = page.pageImage;
        if (nextButton != null)
        {
            var rect = nextButton.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = page.buttonAnchoredPosition;
        }
    }

    private void OnNextButtonClicked()
    {
        int nextPage = currentPage + 1;
        if (nextPage < pages.Count)
        {
            ShowPage(nextPage);
        }
        else
        {
            EndTutorial();
        }
    }

    private void EndTutorial()
    {
    // 튜토리얼 UI 비활성화 또는 종료 처리
    Time.timeScale = prevTimeScale;
    gameObject.SetActive(false);
    }
}
