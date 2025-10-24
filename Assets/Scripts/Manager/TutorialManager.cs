using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public TutorialSequence currentSequence;
    public GameObject tutorialPanel; // TutorialOverlay를 할당
    public TextMeshProUGUI descriptionText;
    public Button nextButton;
    public RectTransform highlightHole; // HighlightHole의 RectTransform을 할당

    private int currentStepIndex = 0;
    private Canvas mainCanvas; // 캔버스 캐싱

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCanvas = GetComponentInParent<Canvas>(); // 씬의 최상위 캔버스를 찾아옴
    }

    // ... Start, StartTutorial 메서드는 동일 ...
    void Start()
    {
        StartTutorial(currentSequence);
        // PlayerPrefs 등을 사용하여 튜토리얼을 이미 완료했는지 확인
        //if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 0)
        //{
        //    StartTutorial(currentSequence);
        //}
    }

    public void StartTutorial(TutorialSequence sequence)
    {
        Time.timeScale = 0.0f;
        currentSequence = sequence;
        currentStepIndex = 0;
        tutorialPanel.SetActive(true);
        ShowStep(currentStepIndex);
    }

    void ShowStep(int stepIndex)
    {
        if (stepIndex >= currentSequence.steps.Count) return;

        TutorialStep step = currentSequence.steps[stepIndex];
        descriptionText.text = step.description;

        // 임시 하드 코딩
        if (stepIndex == 2)
            TechViewer.instance.ChangeTechTab(TechKind.Job);
        else if (stepIndex == 3)
            TechViewer.instance.ChangeTechTab(TechKind.Structure);


            // 타겟 찾기
            TutorialTarget[] allTargets = FindObjectsByType<TutorialTarget>(FindObjectsSortMode.None);
        bool targetFound = false;
        foreach (TutorialTarget target in allTargets)
        {
            if (target.id == step.targetId)
            {
                HighlightTarget(target.gameObject, step.isUI);
                targetFound = true;
                break;
            }
        }

        // 타겟을 못찾았다면 하이라이트는 숨김
        if (!targetFound)
        {
            highlightHole.gameObject.SetActive(false);
        }
    }

    void HighlightTarget(GameObject target, bool isUI)
    {
        highlightHole.gameObject.SetActive(true);

        if (isUI)
        {
            RectTransform targetRect = target.GetComponent<RectTransform>();
            highlightHole.position = targetRect.position;
            // UI 크기에 맞게 구멍 크기 조절
            highlightHole.sizeDelta = targetRect.sizeDelta + new Vector2(20, 20); // 약간의 여백
        }
        else // 게임 오브젝트 (2D or 3D)
        {
            // 게임 오브젝트의 3D 월드 좌표를 스크린 좌표로 변환
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(target.transform.position);
            highlightHole.position = screenPosition;

            // 게임 오브젝트 크기에 맞게 구멍 크기 조절 (Collider나 Renderer Bounds 기준)
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                Vector3 min = Camera.main.WorldToScreenPoint(bounds.min);
                Vector3 max = Camera.main.WorldToScreenPoint(bounds.max);
                float width = max.x - min.x;
                float height = max.y - min.y;
                highlightHole.sizeDelta = new Vector2(width, height) + new Vector2(20, 20);
            }
            else // 렌더러가 없으면 기본 크기
            {
                highlightHole.sizeDelta = new Vector2(150, 150);
            }
        }
    }

    public void GoToNextStep()
    {
        currentStepIndex++;
        if (currentStepIndex < currentSequence.steps.Count)
        {
            ShowStep(currentStepIndex);
        }
        else
        {
            EndTutorial();
        }
    }

    void EndTutorial()
    {
        Time.timeScale = 1.0f;
        tutorialPanel.SetActive(false);
        highlightHole.gameObject.SetActive(false);
        //PlayerPrefs.SetInt("TutorialCompleted", 1); // 나중에 추가
        Debug.Log("Tutorial Finished!");
    }
}