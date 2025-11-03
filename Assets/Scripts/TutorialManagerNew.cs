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
    private bool structureUpgraded = false;         // 업그레이드가 발생한 적 있는가
    private bool upgradedFlagBefore14Finish = false; // 14 타자기 '완료 전'에 업그레이드가 있었는가
    private bool hasShownWorkerHireOnce = false; // '일꾼을 추가했습니다!' 1회만
    private int structureUpgradeCount = 0;        // 구조물 업그레이드(=건설) 발생 횟수
    private bool hasStartedHireGuideOnce = false; // 18단계(고용 안내)로 진입한 적 있는가
    private bool hasShownCatGuide = false; // 고양이 튜토리얼 1회만

    private bool firstCatPicked = false;   // ★ 최초 고양이 선택 여부
    private bool waitingFirstCatPick = false; // ★ 대기 상태 플래그(가독성용)
    
    private bool isWatchingRatioButton = false;   // 21 이후부터 클릭 감시 시작
    private bool ratioButtonClicked = false;      // 해당 버튼이 최초 클릭되었는가
    
    private int ratioButtonClickCount = 0;     // 21 이후 버튼 클릭 누적
    private bool hasShownRatioSecondHint = false; // CASE 22 한 번만
    
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
        
        TechEachUI.OnAnyStructureUpgraded += HandleStructureUpgraded; 
        StructureApperance.OnFirstLevelUpButtonShown += HandleFirstLevelUpButtonShown;
        StructureApperance.OnLevelUpButtonPressed += HandleLevelUpButtonPressed;
        TechEachUI.OnAnyWorkerHired += HandleWorkerHired;  
        AuthorityInfoUI.OnAuthorityLevelChanged += HandleAuthorityLevelChanged;
        TechEachUI.OnFirstCatSelected += HandleFirstCatSelected;

        StartCoroutine(TutorialCoroutine());
    }

    
    void OnDestroy()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnAuthorityLevelStackChanged -= HandleAuthorityStackChanged;
        TechEachUI.OnAnyStructureUpgraded -= HandleStructureUpgraded; // ★ 해지
        StructureApperance.OnFirstLevelUpButtonShown -= HandleFirstLevelUpButtonShown;
        StructureApperance.OnLevelUpButtonPressed -= HandleLevelUpButtonPressed; 
        TechEachUI.OnAnyWorkerHired -= HandleWorkerHired; 
        AuthorityInfoUI.OnAuthorityLevelChanged -= HandleAuthorityLevelChanged;
        TechEachUI.OnFirstCatSelected -= HandleFirstCatSelected;
    }
    
    private void HandleFirstCatSelected(CatGodType _)
    {
        if (firstCatPicked) return;
        firstCatPicked = true;
    }
    
    public void NotifyRatioButtonClicked()
    {
        if (!isWatchingRatioButton) return;   // 21 이전엔 무시

        ratioButtonClickCount++;

        // 첫 번째 클릭: 즉시 패널 OFF (기존 동작 유지)
        if (ratioButtonClickCount == 1)
        {
            if (panelRectTransform != null)
                panelRectTransform.gameObject.SetActive(false);
            return;
        }

        // 두 번째 클릭: CASE 22 한 번만 노출
        if (ratioButtonClickCount == 2 && !hasShownRatioSecondHint)
        {
            hasShownRatioSecondHint = true;

            if (panelRectTransform != null)
            {
                panelRectTransform.anchoredPosition = new Vector2(346f, -327f);
                panelRectTransform.gameObject.SetActive(true);
            }

            GoToStep(22); // 텍스트 출력은 ShowTutorialStep/Typewriter에서 처리
            return;
        }

        // 세 번째 클릭 이후는 무시 (원하면 여기서 추가 액션 가능)
    }


    
    // 권위 레벨 변경을 받았을 때
    private void HandleAuthorityLevelChanged(int newLevel)
    {
        if (hasShownCatGuide) return;
        if (newLevel >= 6)
        {
            hasShownCatGuide = true;
            StartCoroutine(CatGuideSequence());
        }
    }

// 고양이 튜토리얼 3문장 시퀀스 + (최초 고양이 선택 대기) + 종료 안내 3문장 후 패널 OFF
private IEnumerator CatGuideSequence()
{
    // 진행 중 타자기 중단
    if (typewriterCoroutine != null)
    {
        StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = null;
    }

    // 패널 위치 지정 후 표시 (378,114)
    if (panelRectTransform != null)
    {
        panelRectTransform.gameObject.SetActive(true);
        panelRectTransform.anchoredPosition = new Vector2(378f, 114f);
    }

    // 3문장 출력
    tutorialStep = 101;
    typewriterCoroutine = StartCoroutine(TypewriterCoroutine("이제 고양이를 활성화 할수있습니다."));
    yield return new WaitUntil(() => typewriterCoroutine == null);
    yield return new WaitForSeconds(1f);

    tutorialStep = 102;
    typewriterCoroutine = StartCoroutine(TypewriterCoroutine("고양이는 범위내에 일꾼들을 채찍질해줍니다"));
    yield return new WaitUntil(() => typewriterCoroutine == null);
    yield return new WaitForSeconds(1f);

    tutorialStep = 103;
    typewriterCoroutine = StartCoroutine(TypewriterCoroutine("고양이는 잡고 들어서 이동시킬수있으며 우클릭으로 앉힐 수 있씁니다"));
    yield return new WaitUntil(() => typewriterCoroutine == null);
    yield return new WaitForSeconds(1f);

    // ★ 여기서 '특수 탭에서 고양이 최초 선택'을 기다림
    waitingFirstCatPick = true;
    yield return new WaitUntil(() => firstCatPicked); // 이벤트가 오면 true

    // 패널 잠깐 숨김 → 0.5초 → (0,0)으로 이동 → 종료 3문장
    if (panelRectTransform != null)
    {
        panelRectTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        panelRectTransform.anchoredPosition = Vector2.zero;
        panelRectTransform.gameObject.SetActive(true);
    }

    // 종료 문구: 201~203 스텝 활용 (각 1초)
    tutorialStep = 201;
    typewriterCoroutine = StartCoroutine(TypewriterCoroutine("이제 튜토리얼이 종료되었습니다"));
    yield return new WaitUntil(() => typewriterCoroutine == null);
    yield return new WaitForSeconds(1f);

    tutorialStep = 202;
    typewriterCoroutine = StartCoroutine(TypewriterCoroutine("자신만의 피라미드를 쌓아보세요!"));
    yield return new WaitUntil(() => typewriterCoroutine == null);
    yield return new WaitForSeconds(1f);

    tutorialStep = 203;
    typewriterCoroutine = StartCoroutine(TypewriterCoroutine("감사합니다"));
    yield return new WaitUntil(() => typewriterCoroutine == null);
    yield return new WaitForSeconds(1f);

    // 튜토리얼 패널 OFF
    if (panelRectTransform != null)
    {
        panelRectTransform.gameObject.SetActive(false);
    }
}


    
    private void HandleLevelUpButtonPressed()
    {
        // ★ 수정: 구조물 카운트 체크 제거, 17~19 구간 + 1회만 허용
        if (hasStartedHireGuideOnce || hasShownWorkerHireOnce) return;
        if (tutorialStep != 17 && tutorialStep != 18 && tutorialStep != 19) return;

        hasStartedHireGuideOnce = true;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (panelRectTransform != null)
        {
            panelRectTransform.gameObject.SetActive(true);
            panelRectTransform.anchoredPosition = new Vector2(378f, 196f);
        }

        GoToStep(18);
    }



    // 일꾼이 실제로 추가(잡 업그레이드)되었을 때
    private void HandleWorkerHired()
    {
        // 이미 한 번 보여줬으면 무시
        if (hasShownWorkerHireOnce) return;

        // 튜토리얼 흐름상 18~19 단계에서만 노출
        if (tutorialStep < 18 || tutorialStep > 19) return;

        // 진행 중인 타자기 중단
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        // 패널 표시(필요 시 위치 고정 가능)
        if (panelRectTransform != null)
        {
            panelRectTransform.gameObject.SetActive(true);
            // panelRectTransform.anchoredPosition = new Vector2(378f, 196f);
        }

        // 문구 전환
        GoToStep(20);

        // 1회만 표시하도록 플래그 + 구독 해제
        hasShownWorkerHireOnce = true;
        TechEachUI.OnAnyWorkerHired -= HandleWorkerHired;
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
            case 9: 
                textToShow = "'권위'탭을 눌러서 권위 포인트를 사용해보세요.";
                break;
            case 10: 
                textToShow = "이곳에는 채찍의 능력을 올릴 수 있습니다.";
                break;
            case 11: 
                textToShow = "잘하셨습니다!";
                break;
            case 12:
                textToShow = "건물 탭에서는 건물을 업그레이드 할 수 있습니다.";
                break;
            case 13:
                textToShow = "건물을 업그레이드 하기 위해서는 돈이 필요합니다.";
                break;
            case 14:
                textToShow = "권위 레벨에 따라 건물이 해금됩니다.";
                break;
            case 15:
                textToShow = "잘하셨습니다!";
                break;
            case 16:
                textToShow = "돈을 벌어서 건물을 업그레이드 해 보세요!";
                break;
            case 17:
                textToShow = "새로운 건물을 열면 건물을 지어야 합니다";
                break;
            case 18:
                textToShow = "이제 새로운 일꾼을 고용할 수 있습니다";
                break;
            case 19:
                textToShow = "새로운 일꾼으로 일인당 징수금이나 초당금을 올릴 수 있습니다";
                break;
            case 20:
                textToShow = "일꾼을 추가했습니다!";
                break;
            case 21:
                textToShow = "이 버튼을 눌러서 금 생산 비율을 볼 수 있습니다.";
                break;
            case 22:
                textToShow = "이 버튼은 권위레벨이 레벨 20 이상 올랐을때 포인트를 사용하여 활성화 할 수 있습니다.";
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

        // 10번 문장 타이핑이 끝났고, 그 전에 포인트를 이미 썼다면 11로
        if (tutorialStep == 10 && authoritySpendPending)
        {
            authoritySpendPending = false;
            yield return new WaitForSeconds(1f);
            GoToStep(11);
            yield break; // 11로 넘어갔으면 종료
        }

        // 11번 문장 타이핑이 모두 끝났다면: 0.5초 후 패널 숨김 → 0.5초 후 이동해서 12로
        if (tutorialStep == 11)
        {
            StartCoroutine(AfterStep11_ShowBuildingHint());
            yield break; // 별도 코루틴이 처리
        }

        // 12번 문장 타이핑이 끝났다면: 1초 후 자동으로 13으로
        if (tutorialStep == 12)
        {
            int snapshot = tutorialStep;              // 스냅샷
            yield return new WaitForSeconds(1f);      // 여운
            if (tutorialStep == snapshot)             // 그 사이 스텝이 바뀌지 않았다면
            {
                GoToStep(13);
            }
            yield break;
        }
        if (tutorialStep == 13)
        {
            int snapshot = tutorialStep;              // 스냅샷
            yield return new WaitForSeconds(1f);      // 여운
            if (tutorialStep == snapshot)             // 그 사이 스텝이 바뀌지 않았다면
            {
                GoToStep(14);
            }
            yield break;
        }
        
        // ★ 14가 방금 끝났다면:
        if (tutorialStep == 14)
        {
            // 14 '끝나기 전에' 이미 업그레이드가 있었다면 1초 후 15로
            if (upgradedFlagBefore14Finish)
            {
                upgradedFlagBefore14Finish = false;
                yield return new WaitForSeconds(1f);
                GoToStep(15);
                yield break;
            }

            // 업그레이드가 아직 없었다면 여기서 종료.
            // 이후 업그레이드가 일어나는 순간 HandleStructureUpgraded()가 즉시 15로 넘겨줌.
            yield break;
        }
// 15번 문장 타이핑이 모두 끝났다면: 1초 유지 → 0.5초 패널 숨김 → (378,460)에서 다시 활성화 → 16으로
        if (tutorialStep == 15)
        {
            int snapshot = tutorialStep;              // 스냅샷
            yield return new WaitForSeconds(1f);      // 15번 문구 1초간 유지
            GoToStep(16);
            yield break;
        }

// 16번 문장 타이핑이 모두 끝났다면: 2초 후 패널 비활성화
        if (tutorialStep == 16)
        {
            int snapshot = tutorialStep;              // 경쟁상황 방지
            yield return new WaitForSeconds(2f);      // 16번 문구 2초간 유지

            if (tutorialStep == snapshot)             // 그 사이 스텝 변화 없으면
            {
                if (panelRectTransform != null)
                    panelRectTransform.gameObject.SetActive(false);
                // 튜토리얼 자체는 계속 활성화(필요 시 이후 단계 진행)
            }
            yield break;
        }
        if (tutorialStep == 18)
        {
            int snapshot = tutorialStep;          // 경쟁 상태 방지
            yield return new WaitForSeconds(1f);  // 여운
            if (tutorialStep == snapshot)         // 중간에 스텝이 바뀌지 않았다면
            {
                GoToStep(19);
            }
            yield break;
        }
        // 20번 문장 타이핑이 모두 끝났다면: 1초 후 패널 비활성화
        if (tutorialStep == 20)
        {
            int snapshot = tutorialStep;
            yield return new WaitForSeconds(1f); // 20 유지 1초
            if (tutorialStep != snapshot) yield break;

            if (panelRectTransform != null)
            {
                panelRectTransform.gameObject.SetActive(false); // 0.5초간 OFF
                yield return new WaitForSeconds(0.5f);
                panelRectTransform.anchoredPosition = new Vector2(378f, 460f);
                panelRectTransform.gameObject.SetActive(true);
            }

            // case 21 진입 및 버튼 클릭 감시 시작
            isWatchingRatioButton = true;
            GoToStep(21);
            yield break;
        }
        
        // TypewriterCoroutine(string text) 하단 분기들에 추가
        if (tutorialStep == 22)
        {
            int snapshot = tutorialStep;
            yield return new WaitForSeconds(5f);  // 5초 유지
            if (tutorialStep == snapshot && panelRectTransform != null)
            {
                panelRectTransform.gameObject.SetActive(false);
            }
            yield break;
        }

    }
    
    private void HandleStructureUpgraded()
    {
        structureUpgraded = true;
        structureUpgradeCount++; // ★ 추가

        // 14문구 '타자 중'이면, 끝난 뒤 1초 후 점프 플래그만 킨다
        if (tutorialStep == 14 && typewriterCoroutine != null)
        {
            upgradedFlagBefore14Finish = true;
            return;
        }

        // 14문구가 이미 화면에 다 찍힌 상태라면 즉시 축하(step 15)로 점프
        if (tutorialStep == 14 && typewriterCoroutine == null)
        {
            GoToStep(15);
        }
    }



    // 11 완료 후, 패널 숨김/이동/표시하고 12로 진입
    private IEnumerator AfterStep11_ShowBuildingHint()
    {
        // 0.5초 대기 (문장 여운)
        yield return new WaitForSeconds(0.5f);

        if (panelRectTransform != null)
            panelRectTransform.gameObject.SetActive(false);

        // 0.5초 추가 대기 후 위치 이동 + 표시
        yield return new WaitForSeconds(0.5f);

        if (panelRectTransform != null)
        {
            panelRectTransform.anchoredPosition = new Vector2(389f, 257f);
            panelRectTransform.gameObject.SetActive(true);
        }

        // 건물 탭 안내 문구로 진입
        GoToStep(12);
    }

// ★ 추가: 전역 1회 표시 가드
    private bool hasShownUpgradeHintOnce = false;

    private void HandleFirstLevelUpButtonShown(RectTransform buttonRect)
    {
        // 전역 1회만 처리
        if (hasShownUpgradeHintOnce) return;

        if (panelRectTransform == null || buttonRect == null) return;

        var panelParent = panelRectTransform.parent as RectTransform;
        if (panelParent == null) return;

        Vector3 worldCenter = buttonRect.TransformPoint(buttonRect.rect.center);
        Vector2 localPoint = panelParent.InverseTransformPoint(worldCenter);
        localPoint += new Vector2(100f, 0f);

        panelRectTransform.anchoredPosition = localPoint;
        panelRectTransform.gameObject.SetActive(true);

        GoToStep(17);

        // ★ 전역 플래그 ON + 더 이상 이벤트 받지 않도록 구독 해제
        hasShownUpgradeHintOnce = true;
        StructureApperance.OnFirstLevelUpButtonShown -= HandleFirstLevelUpButtonShown;
    }




}
