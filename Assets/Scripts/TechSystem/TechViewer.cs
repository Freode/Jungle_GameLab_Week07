using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public List<GameClearEffectInStructure> OnPreviousStructureCompletes;   // 이전 구조물 완성
    public TechData completePyramidTechData;                                // 피라미드 완성 테크 데이터

    public GameObject techUIInToVerticalLayer;              // Tech UI를 vertical layer에 추가할 곳
    [SerializeField] private GameObject uiPrefab;
    [SerializeField] private List<TechKindInfo> techInfoes; // 테크 기본 정보들

    private TechKind curTechKind;                                                   // 현재 테크 텝 상태
    private Dictionary<TechKind, Dictionary<TechData, TechState>> techStates;       // 테크별 상태
    private Dictionary<TechKind, int> techKindIdx;                                  // 테크 유형별 번호
    private List<TechEachUI> techEachUIs;                                           // 테크 UI

    // === 수정 필요 ===
    private Dictionary<TechData, GameObject> techObjects;    // 구조물 할당

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
        ChangeTechTab(TechKind.Job);

        buttonTabStructure.onClick.AddListener(OnClickStructureTab);
        buttonTabJob.onClick.AddListener(OnClickJobTab);
        buttonTabSpecial.onClick.AddListener(OnClickSpecialTab);
    }

    // 특정 테크의 사전 테크가 모두 해제되었는지 확인
    public void CheckUnlockPreTech(TechData techData)
    {
        // 현재 타입과 동일한지 확인
        if (curTechKind != techData.techKind)
            return;

        foreach (var preTech in techData.preTeches)
        {
            if (techStates[preTech.techKind][preTech].lockState != LockState.Complete)
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
        PeopleManager.Instance.OnAreaPeopleCountChanged += PrintRemainPeople;

        if(OnPreviousStructureCompletes.Count > 0)
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
        PeopleManager.Instance.OnAreaPeopleCountChanged -= PrintRemainPeople;

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

        SetTabName(techKind);
        ChangeTabsAlphaValue(techKind);

        curTechKind = techKind;
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

    // 잉여 인력 출력
    void PrintRemainPeople()
    {
        int amount = PeopleManager.Instance.Count(AreaType.Normal);
        textPeopleCount.text = "무직 : " + amount;
    }

    // 기술 탭 클릭
    void OnClickStructureTab()
    {
        GameLogger.Instance.click.AddUpgradeClick();
        ChangeTechTab(TechKind.Structure);
    }

    // 징집 탭 클릭
    void OnClickJobTab()
    {
        GameLogger.Instance.click.AddUpgradeClick();
        ChangeTechTab(TechKind.Job);
    }

    // 특수 탭 클릭
    void OnClickSpecialTab()
    {
        GameLogger.Instance.click.AddUpgradeClick();
        ChangeTechTab(TechKind.Special);
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
                break;

            case TechKind.Job:
                IncreaseButtonAlpha(buttonTabStructure, false);
                IncreaseButtonAlpha(buttonTabJob, true);
                IncreaseButtonAlpha(buttonTabSpecial, false);
                break;

            case TechKind.Special:
                IncreaseButtonAlpha(buttonTabStructure, false);
                IncreaseButtonAlpha(buttonTabJob, false);
                IncreaseButtonAlpha(buttonTabSpecial, true);
                break;
        }
    }

    // 이전 테크가 완료되었다고 알림 (피라미드 완성 포함)
    private void CompletePreviousTechByOutside(TechData techData)
    {
        // 피라미드 완성
        if(techData == completePyramidTechData)
            Debug.Log("Complete");

        techStates[TechKind.None][techData].lockState = LockState.Complete;
        foreach(TechData nextTech in techData.postTeches)
        {
            CheckUnlockPreTech(nextTech);
        }
    }
}
