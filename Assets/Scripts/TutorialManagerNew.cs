using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManagerNew : MonoBehaviour
{
    public static TutorialManagerNew Instance { get; private set; }

    public TextMeshProUGUI tutorialText;
    public AuthorityInfoUI authorityInfoUI; // Assign this in the Inspector
    public int tutorialStep = 0;
    private bool isFirstMoneyCollected = false;
    private float typingSpeed = 0.05f;
    private Coroutine typewriterCoroutine;
    private RectTransform panelRectTransform;
    private int lastAuthorityStack;
    private bool authoritySpendPending = false;    // 포인트 소비가 발생했음을 기억(언제 발생했든)


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (tutorialText != null)
        {
            panelRectTransform = tutorialText.transform.parent.GetComponent<RectTransform>();
        }
    }

    void Start()
    {
        if (panelRectTransform != null)
        {
            panelRectTransform.anchoredPosition = Vector2.zero;
        }

        // 권위 포인트 스택 초기값과 이벤트 구독
        lastAuthorityStack = GameManager.instance.GetAuthroityLevelUpStack();
        GameManager.instance.OnAuthorityLevelStackChanged += HandleAuthorityStackChanged;

        StartCoroutine(TutorialCoroutine());
    }

    
    void OnDestroy()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnAuthorityLevelStackChanged -= HandleAuthorityStackChanged;
    }


    IEnumerator TutorialCoroutine()
    {
        ShowTutorialStep(tutorialStep); // Step 0
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);

        NextStep(); // Step 1
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);

        // Move panel for step 2
        if (panelRectTransform != null)
        {
            panelRectTransform.anchoredPosition = new Vector2(-575, 252);
        }

        NextStep(); // Step 2
    }

    void Update()
    {

    }

    public void OnFirstMoneyCollected()
    {
        if (tutorialStep == 2 && !isFirstMoneyCollected && typewriterCoroutine == null)
        {
            isFirstMoneyCollected = true;
            StartCoroutine(AfterMoneyCollectedSequence());
        }
    }
    
    private void GoToStep(int step)
    {
        // 진행 중인 타자기 코루틴이 있으면 정지
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        tutorialStep = step;
        ShowTutorialStep(step);
    }

// LevelUpSequence() 수정
    IEnumerator LevelUpSequence()
    {
        panelRectTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        panelRectTransform.anchoredPosition = new Vector2(375, 34);
        panelRectTransform.gameObject.SetActive(true);

        GoToStep(8);
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);

        GoToStep(9);  // ★ 새로 삽입한 브릿지 문장
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);

        GoToStep(10); // ★ 기존 9가 10으로 밀렸으므로 여기까지 안내
    }

    
    private void HandleAuthorityStackChanged()
    {
        int cur = GameManager.instance.GetAuthroityLevelUpStack();

        // 스택 감소(=소비)만 감지
        if (cur < lastAuthorityStack)
        {
            authoritySpendPending = true;

            // 이미 9번 문장 타자기가 끝난 상태라면 곧장 10으로
            if (tutorialStep == 10 && typewriterCoroutine == null)
            {
                authoritySpendPending = false; // 소진
                GoToStep(11);
            }
        }

        lastAuthorityStack = cur;
    }




    IEnumerator AfterMoneyCollectedSequence()
    {
        NextStep(); // Step 3
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);

        NextStep(); // Step 4
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);

        panelRectTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);

        panelRectTransform.anchoredPosition = new Vector2(-535, -313);
        panelRectTransform.gameObject.SetActive(true);

        NextStep(); // Step 5
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);

        NextStep(); // Step 6
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);

        // Check level after step 6
        if (authorityInfoUI != null)
        {
            string levelText = authorityInfoUI.textLevel.text.Substring(4);
            if (int.TryParse(levelText, out int level) && level >= 2)
            {
                StopAllCoroutines();
                StartCoroutine(LevelUpSequence());
                yield break; // End this coroutine
            }
        }

        // Panel movement for step 7
        panelRectTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        panelRectTransform.anchoredPosition = new Vector2(-575, 252);
        panelRectTransform.gameObject.SetActive(true);

        NextStep(); // Step 7
        yield return new WaitUntil(() => typewriterCoroutine == null);

        // Loop to check for level up
        while (tutorialStep == 7)
        {
            if (authorityInfoUI != null)
            {
                string levelText = authorityInfoUI.textLevel.text.Substring(4);
                if (int.TryParse(levelText, out int level) && level >= 2)
                {
                    StopAllCoroutines();
                    StartCoroutine(LevelUpSequence());
                    break;
                }
            }
            yield return null;
        }
    }

    public void NextStep()
    {
        tutorialStep++;
        ShowTutorialStep(tutorialStep);
    }

    void ShowTutorialStep(int step)
    {
        if (tutorialText == null)
        {
            Debug.LogError("Tutorial Text (TextMeshProUGUI) is not assigned in the Inspector!");
            return;
        }

        string textToShow = "";
        switch (step)
        {
            case 0:
                textToShow = "더 피라미드에 오신 여러분 환영합니다.";
                break;
            case 1:
                textToShow = "돈을 벌기 위해서는 일꾼들을 부려야합니다.";
                break;
            case 2:
                textToShow = "일꾼들 위에 마우스를 올려보세요.";
                break;
            case 3:
                textToShow = "잘하셨습니다!";
                break;
            case 4:
                textToShow = "이렇게 일꾼들을 채찍질해서 돈을 획득할 수 있습니다.";
                break;
            case 5:
                textToShow = "이곳에서 권위레벨, 권위 포인트, 권위 게이지를 볼 수 있습니다.";
                break;
            case 6:
                textToShow = "권위게이지가 끝까지 차면 권위 레벨이 올라가고 포인트를 하나 얻습니다.";
                break;
            case 7:
                textToShow = "일꾼을 더 때려보세요!";
                break;
            case 8:
                textToShow = "권위 레벨을 올리셨군요!";
                break;
            case 9: // ★ 새로 추가된 브릿지 문장
                textToShow = "'권위'탭을 눌러서 권위 포인트를 사용해보세요.";
                break;
            case 10: // (기존 case 9)
                textToShow = "이곳에는 채찍의 능력을 올릴 수 있습니다.";
                break;
            case 11: // (기존 case 10)
                textToShow = "잘하셨습니다!";
                break;
            default:
                textToShow = "튜토리얼 완료!";
                StartCoroutine(EndTutorialCoroutine());
                break;
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        typewriterCoroutine = StartCoroutine(TypewriterCoroutine(textToShow));
    }

    IEnumerator EndTutorialCoroutine()
    {
        yield return new WaitUntil(() => typewriterCoroutine == null);
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    IEnumerator TypewriterCoroutine(string text)
    {
        tutorialText.text = "";
        foreach (char c in text)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typewriterCoroutine = null;

        // ✅ 10번 문장 타이핑이 '방금' 끝났고, 그 전에 포인트를 이미 썼다면 즉시 11번으로
        if (tutorialStep == 10 && authoritySpendPending)
        {
            authoritySpendPending = false;  // 소진
            yield return new WaitForSeconds(1f);
            GoToStep(11);
        }
    }


}
