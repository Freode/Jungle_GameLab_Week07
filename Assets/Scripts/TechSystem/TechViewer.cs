using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechViewer : MonoBehaviour
{
    public static TechViewer instance;
    public GameObject techInfo;
    public GameObject strcutureInfo;
    public TextMeshProUGUI textPeopleCount;
    public TextMeshProUGUI textTabName;                     // Tab 이름 변경
    public int maxTechUIsCount = 9;                         // Tech UI가 출력될 총 칸 수
    public Button buttonTabStructure;                       // 기술 탭 버튼
    public Button buttonTabJob;                             // 징집 탭 버튼
    public Button buttonTabSpecial;                         // 특수 탭 버튼
    public Button buttonTabPower;                           // 권능 탭 버튼

    public List<GameClearEffectInStructure> OnPreviousStructureCompletes;   // 이전 구조물 완성
    public TechData completePyramidTechData;                                // 피라미드 완성 테크 데이터

    public GameObject techUIInToVerticalLayer;              // Tech UI를 vertical layer에 추가할 곳
    [SerializeField] private GameObject uiPrefab;
    public List<TechKindInfo> techInfoes; // 테크 기본 정보들

    private TechKind curTechKind;                                                   // 현재 테크 텝 상태
    private Dictionary<TechKind, Dictionary<TechData, TechState>> techStates;       // 테크별 상태
    private Dictionary<TechKind, int> techKindIdx;                                  // 테크 유형별 번호
    private List<TechEachUI> techEachUIs;                                           // 테크 UI

    // === 수정 필요 ===
    private Dictionary<TechData, GameObject> techObjects;    // 구조물 할당

    // 탭 하이라이트 관련
    private RainbowButtonEffect structureTabEffect;
    private RainbowButtonEffect jobTabEffect;
    private RainbowButtonEffect specialTabEffect;
    private RainbowButtonEffect powerTabEffect;
    
    // 탭 위치 컨트롤러
    private TabPositionController structureTabPosition;
    private TabPositionController jobTabPosition;
    private TabPositionController specialTabPosition;
    private TabPositionController powerTabPosition;

    void Awake()
    {
        instance = this;
        techStates = new Dictionary<TechKind, Dictionary<TechData, TechState>>();
        techKindIdx = new Dictionary<TechKind, int>();
        techEachUIs = new List<TechEachUI>();
    }

    void Start()
    {
        InitUI();
        InitTechData();
        InitTabEffects();
        ChangeTechTab(TechKind.Structure);

        buttonTabStructure.onClick.AddListener(OnClickStructureTab);
        buttonTabJob.onClick.AddListener(OnClickJobTab);
        buttonTabSpecial.onClick.AddListener(OnClickSpecialTab);
        buttonTabPower.onClick.AddListener(OnClickPowerTab);

        // 금광 노예 버튼 하이라이트
        HighlightMinerButton();
    }

    // 특정 테크의 사전 테크가 모두 해제되었는지 확인
    public void CheckUnlockPreTech(TechData techData)
    {
        // 현재 타입과 동일한지 확인
        if (curTechKind != techData.techKind)
            return;

        foreach (var preTech in techData.preTeches)
        {
            // 아직/다시 잠겨 있다면
            if (techStates[preTech.techKind][preTech].lockState == LockState.Block)
            {
                TechState lockTechState = techStates[techData.techKind][techData];
                techEachUIs[lockTechState.curTechUIIdx].SetTechLock();
                techEachUIs[lockTechState.curTechUIIdx].OnTechInactive();

                // 권능 포인트
                if (techData.isUseAuthroityPoint)
                    techEachUIs[lockTechState.curTechUIIdx].SetButtonStateInAuthorityPoint();

                return;
            }

            if (techStates[preTech.techKind][preTech].lockState == LockState.CanUnlock)
                return;

            // 나중에 TechData techData, TechData or 다른것 condition 으로 다양한 조건 추가 가능하도록 확장 가능
            // 최고 레벨이 도달하지 않는 경우는 무시
            //if (techStates[preTech.techKind][preTech].currentLevel < preTech.maxLevel)
            //    return;
        }

        // 해금 가능하다고 업데이트
        TechState changeTechState = techStates[techData.techKind][techData];
        techEachUIs[changeTechState.curTechUIIdx].SetTechUnlock();
        techEachUIs[changeTechState.curTechUIIdx].OnCheckTechActive();
    }

    void InitUI()
    {
        for (int i = 0; i < maxTechUIsCount; i++)
        {
            CreateTechEachUI();
        }

        GameManager.instance.OnMaxCapacityUpgrade += MaxCapacityUpgrade;
        GameManager.instance.OnCurrentCapacityChanged += ModifyCurrentCapacity;
        PeopleManager.Instance.OnAreaPeopleCountChanged += PrintRemainAmountByTab;
        GameManager.instance.OnAuthorityLevelStackChanged += PrintRemainAmountByTab;

        if (OnPreviousStructureCompletes.Count > 0)
        {
            foreach(var OnPreviousStructureComplete in OnPreviousStructureCompletes)
            {
                OnPreviousStructureComplete.OnEvent += CompletePreviousTechByOutside;
            }
        }
    }

    private void OnDestroy()
    {
        GameManager.instance.OnMaxCapacityUpgrade -= MaxCapacityUpgrade;
        GameManager.instance.OnCurrentCapacityChanged -= ModifyCurrentCapacity;
        PeopleManager.Instance.OnAreaPeopleCountChanged -= PrintRemainAmountByTab;
        GameManager.instance.OnAuthorityLevelStackChanged -= PrintRemainAmountByTab;

        if (OnPreviousStructureCompletes.Count > 0)
        {
            foreach (var OnPreviousStructureComplete in OnPreviousStructureCompletes)
            {
                OnPreviousStructureComplete.OnEvent -= CompletePreviousTechByOutside;
            }
        }
    }

    // 테크 데이터들 초기화
    void InitTechData()
    {
        int idx = 0;
        foreach (var techInfo in techInfoes)
        {
            // 유형을 먼저 추가
            TechKind techKind = techInfo.techKind;
            techStates.Add(techKind, new Dictionary<TechData, TechState>());
            techKindIdx.Add(techKind, idx);
            idx++;

            int count = 0;
            // 유형에 따른 테크 데이터를 추가
            foreach (var techData in techInfo.techDatas)
            {
                TechState techState = new TechState(techData);
                techState.curTechUIIdx = count;
                techStates[techKind].Add(techData, techState);
                count++;
            }
        }
    }

    // 현재 탭에 대한 테크들만 출력
    public void ChangeTechTab(TechKind techKind)
    {
        if(curTechKind == techKind) return;

        // 모든 탭의 위치를 조절
        // 선택된 탭은 튀어나오고, 나머지는 원래 위치로
        if (structureTabPosition != null)
            if (techKind == TechKind.Structure)
                structureTabPosition.SlideOut();
            else
                structureTabPosition.ResetPosition();

        if (jobTabPosition != null)
            if (techKind == TechKind.Job)
                jobTabPosition.SlideOut();
            else
                jobTabPosition.ResetPosition();

        if (specialTabPosition != null)
            if (techKind == TechKind.Special)
                specialTabPosition.SlideOut();
            else
                specialTabPosition.ResetPosition();

        if (powerTabPosition != null)
            if (techKind == TechKind.Power)
                powerTabPosition.SlideOut();
            else
                powerTabPosition.ResetPosition();

        SetTabName(techKind);
        ChangeTabsAlphaValue(techKind);
        curTechKind = techKind;
        PrintRemainAmountByTab();
        int activeNum = techStates[techKind].Count;
        int kindIdx = techKindIdx[techKind];
        for (int i = 0; i < maxTechUIsCount; i++)
        {
            techEachUIs[i].RemoveState();
            // 활성화
            if (i < activeNum)
            {
                techEachUIs[i].gameObject.SetActive(true);
                TechData curTechData = techInfoes[kindIdx].techDatas[i];
                techEachUIs[i].RegisterState(techStates[techKind][curTechData]);

            }
            // 비활성화
            else
            {
                techEachUIs[i].gameObject.SetActive(false);
            }
        }
    }

    // 각 테크를 담을 수 있는 UI 만들기
    void CreateTechEachUI()
    {
        GameObject eachUI = Instantiate(uiPrefab, techUIInToVerticalLayer.transform);

        eachUI.TryGetComponent(out TechEachUI techEachUI);
        if (techEachUI is null) return;

        techEachUIs.Add(techEachUI);

        // 안내 UI와 연결
        techInfo.TryGetComponent(out TechInfo techInfoComp);
        techEachUI.OnActiveInfo += techInfoComp.OnActiveInfo;
        techEachUI.OnInactiveInfo += techInfoComp.OnInactiveInfo;
    }

    // 특정 테크의 수용량(유사 최대 레벨) 업그레이드
    void MaxCapacityUpgrade(TechData techType, int amount)
    {
        if (curTechKind != techType.techKind)
            return;

        TechState curTechState = techStates[techType.techKind][techType];
        techEachUIs[curTechState.curTechUIIdx].IncreaseMaxCapacity(amount);
    }

    // 특정 테크의 현재 수용량 변경
    void ModifyCurrentCapacity(TechData techType, int amount)
    {
        if (curTechKind != techType.techKind)
            return;

        TechState curTechState = techStates[techType.techKind][techType];
        techEachUIs[curTechState.curTechUIIdx].ModifyCurrentCapacity(amount);
    }

    // 잉여 인력 / 포인트 출력
    void PrintRemainAmountByTab()
    {
        int amount = 0;
        switch(curTechKind)
        {
            case TechKind.Power:
                amount = GameManager.instance.GetAuthroityLevelUpStack();
                textPeopleCount.text = amount + "P 남음";
                break;

            default:
                amount = PeopleManager.Instance.Count(AreaType.Normal);
                textPeopleCount.text = "무직 : " + amount;
                break;
        }
    }

    // 기술 탭 클릭
    void OnClickStructureTab()
    {
        GameLogger.Instance.click.AddUpgradeClick();
        DeactivateTabHighlight(TechKind.Structure);
        ChangeTechTab(TechKind.Structure);
    }

    // 징집 탭 클릭
    void OnClickJobTab()
    {
        GameLogger.Instance.click.AddUpgradeClick();
        DeactivateTabHighlight(TechKind.Job);
        ChangeTechTab(TechKind.Job);
    }

    // 특수 탭 클릭
    void OnClickSpecialTab()
    {
        GameLogger.Instance.click.AddUpgradeClick();
        DeactivateTabHighlight(TechKind.Special);
        ChangeTechTab(TechKind.Special);
    }

    // 권능 탭 클릭
    void OnClickPowerTab()
    {
        GameLogger.Instance.click.AddUpgradeClick();
        DeactivateTabHighlight(TechKind.Power);
        ChangeTechTab(TechKind.Power);
    }

    // 탭 이름 출력
    void SetTabName(TechKind techKind)
    {
        switch (techKind)
        {
            case TechKind.None:
                textTabName.text = "없음";
                break;

            case TechKind.Structure:
                textTabName.text = "건물";
                break;

            case TechKind.Job:
                textTabName.text = "일꾼";
                break;

            case TechKind.Special:
                textTabName.text = "특수";
                break;

            case TechKind.Power:
                textTabName.text = "권위";
                break;

            default:
                textTabName.text = "없음";
                break;
        }
    }

    // 구조물 정보 UI 출력
    public void ActiveStructureInfo(TechState techState)
    {
        strcutureInfo.TryGetComponent(out TechInfo techInfo);
        if (techInfo == null) return;

        int currentLevel = techState.currentLevel;
        int finalLevel = techState.techData.maxLevel;

        techInfo.OnActiveInfo(techState.techData.areaType, currentLevel, finalLevel, null, new Vector3(1920f, 0f, 0f));
    }

    // 구조물 정보 UI 비출력
    public void InactiveStructureInfo()
    {
        strcutureInfo.TryGetComponent(out TechInfo techInfo);
        if (techInfo == null) return;

        techInfo.OnInactiveInfo();
    }

    // 버튼 alpha 값을 조정
    private void IncreaseButtonAlpha(Button button, bool isIncrease)
    {
        ColorBlock colors = button.colors;
        Color normal = colors.normalColor;
        normal.a = isIncrease ? 1.0f : 0.85f;

        colors.normalColor = normal;
        button.colors = colors;
    }

    // 탭 alpha 값 전체 변경
    private void ChangeTabsAlphaValue(TechKind techKind)
    {
        switch (techKind)
        {
            case TechKind.None:
                break;

            case TechKind.Structure:
                IncreaseButtonAlpha(buttonTabStructure, true);
                IncreaseButtonAlpha(buttonTabJob, false);
                IncreaseButtonAlpha(buttonTabSpecial, false);
                IncreaseButtonAlpha(buttonTabPower, false);
                break;

            case TechKind.Job:
                IncreaseButtonAlpha(buttonTabStructure, false);
                IncreaseButtonAlpha(buttonTabJob, true);
                IncreaseButtonAlpha(buttonTabSpecial, false);
                IncreaseButtonAlpha(buttonTabPower, false);
                break;

            case TechKind.Special:
                IncreaseButtonAlpha(buttonTabStructure, false);
                IncreaseButtonAlpha(buttonTabJob, false);
                IncreaseButtonAlpha(buttonTabSpecial, true);
                IncreaseButtonAlpha(buttonTabPower, false);
                break;

            case TechKind.Power:
                IncreaseButtonAlpha(buttonTabStructure, false);
                IncreaseButtonAlpha(buttonTabJob, false);
                IncreaseButtonAlpha(buttonTabSpecial, false);
                IncreaseButtonAlpha(buttonTabPower, true);
                break;
        }
    }

    // 이전 테크가 완료되었다고 알림 (피라미드 완성 포함)
    private void CompletePreviousTechByOutside(TechKind techKind, TechData techData)
    {
        // 피라미드 완성
        if(techKind == TechKind.None && techData == completePyramidTechData)
            Debug.Log("Complete");

        techStates[techKind][techData].lockState = LockState.Complete;
        foreach(TechData nextTech in techData.postTeches)
        {
            CheckUnlockPreTech(nextTech);
        }
    }

    // 이전 테크가 다시 비활성화되었다고 알림
    public void SetPreviousTechIsIncomplete(TechKind techKind, TechData techData)
    {
        techStates[techKind][techData].lockState = LockState.Block;

        foreach(TechData nextTech in techData.postTeches)
        {
            CheckUnlockPreTech(nextTech);
        }
    }

    // 탭 효과 초기화
    private void InitTabEffects()
    {
        // 각 버튼에 RainbowButtonEffect 컴포넌트 추가 또는 가져오기
        structureTabEffect = buttonTabStructure.gameObject.GetComponent<RainbowButtonEffect>();
        if (structureTabEffect == null)
            structureTabEffect = buttonTabStructure.gameObject.AddComponent<RainbowButtonEffect>();

        jobTabEffect = buttonTabJob.gameObject.GetComponent<RainbowButtonEffect>();
        if (jobTabEffect == null)
            jobTabEffect = buttonTabJob.gameObject.AddComponent<RainbowButtonEffect>();

        specialTabEffect = buttonTabSpecial.gameObject.GetComponent<RainbowButtonEffect>();
        if (specialTabEffect == null)
            specialTabEffect = buttonTabSpecial.gameObject.AddComponent<RainbowButtonEffect>();

        powerTabEffect = buttonTabPower.gameObject.GetComponent<RainbowButtonEffect>();
        if (powerTabEffect == null)
            powerTabEffect = buttonTabPower.gameObject.AddComponent<RainbowButtonEffect>();

        // 각 탭의 위치 컨트롤러 초기화
        structureTabPosition = buttonTabStructure.gameObject.GetComponent<TabPositionController>();
        if (structureTabPosition == null)
            structureTabPosition = buttonTabStructure.gameObject.AddComponent<TabPositionController>();

        jobTabPosition = buttonTabJob.gameObject.GetComponent<TabPositionController>();
        if (jobTabPosition == null)
            jobTabPosition = buttonTabJob.gameObject.AddComponent<TabPositionController>();

        specialTabPosition = buttonTabSpecial.gameObject.GetComponent<TabPositionController>();
        if (specialTabPosition == null)
            specialTabPosition = buttonTabSpecial.gameObject.AddComponent<TabPositionController>();

        powerTabPosition = buttonTabPower.gameObject.GetComponent<TabPositionController>();
        if (powerTabPosition == null)
            powerTabPosition = buttonTabPower.gameObject.AddComponent<TabPositionController>();
    }

    /// <summary>
    /// 특정 탭의 하이라이트 효과 활성화
    /// </summary>
    public void ActivateTabHighlight(TechKind techKind)
    {
        RainbowButtonEffect targetEffect = GetTabEffect(techKind);
        if (targetEffect != null && !targetEffect.IsEffectActive())
        {
            targetEffect.ActivateEffect();
            Debug.Log($"{techKind} 탭 하이라이트 활성화");
        }
    }

    /// <summary>
    /// 특정 탭의 하이라이트 효과 비활성화
    /// </summary>
    private void DeactivateTabHighlight(TechKind techKind)
    {
        RainbowButtonEffect targetEffect = GetTabEffect(techKind);
        if (targetEffect != null && targetEffect.IsEffectActive())
        {
            targetEffect.DeactivateEffect();
            Debug.Log($"{techKind} 탭 하이라이트 비활성화");
        }
    }

    /// <summary>
    /// TechKind에 해당하는 RainbowButtonEffect 가져오기
    /// </summary>
    private RainbowButtonEffect GetTabEffect(TechKind techKind)
    {
        switch (techKind)
        {
            case TechKind.Structure:
                return structureTabEffect;
            case TechKind.Job:
                return jobTabEffect;
            case TechKind.Special:
                return specialTabEffect;
            case TechKind.Power:
                return powerTabEffect;
            default:
                return null;
        }
    }

    /// <summary>
    /// 게임 시작 시 금광 노예 버튼에 무지개 효과 적용
    /// </summary>
    private void HighlightMinerButton()
    {
        // Job 탭에서 Gold AreaType을 가진 TechData 찾기
        if (!techStates.ContainsKey(TechKind.Job))
            return;

        foreach (var kvp in techStates[TechKind.Job])
        {
            TechData techData = kvp.Key;
            TechState techState = kvp.Value;

            // Gold(금광 노예) 영역인 경우
            if (techData.areaType == AreaType.Gold)
            {
                // 현재 Job 탭이 활성화되어 있으므로 해당 UI에 접근 가능
                if (techState.curTechUIIdx >= 0 && techState.curTechUIIdx < techEachUIs.Count)
                {
                    TechEachUI targetUI = techEachUIs[techState.curTechUIIdx];
                    
                    // RainbowButtonTrigger 추가 또는 가져오기
                    RainbowButtonTrigger trigger = targetUI.buttonBG.gameObject.GetComponent<RainbowButtonTrigger>();
                    if (trigger == null)
                    {
                        trigger = targetUI.buttonBG.gameObject.AddComponent<RainbowButtonTrigger>();
                    }

                    // 효과 활성화 (2초 후 자동 비활성화 설정 가능)
                    trigger.ActivateEffect();
                    
                    Debug.Log($"금광 노예 버튼 하이라이트 활성화: {techData.techName}");
                }
                break; // 찾았으므로 종료
            }
        }
    }

    // 다시 잠금
    private void SetLock()
    {

    }
}
