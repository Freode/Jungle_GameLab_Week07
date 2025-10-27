using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TechEachUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button buttonBG;
    public Sprite unlockIcon;
    public Image imageIcon;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textCost;
    public TextMeshProUGUI textLevel;
    public RectTransform totalRectTransform;
    public UnityEngine.Color baseColor;             // 기본 색상

    public event System.Action<string, string, Sprite, Vector3> OnActiveInfo;
    public event System.Action OnInactiveInfo;

    private TechState techState;        // 데이터 원본과 상태 저장
    private float upperY = 5000f;
    private float leftX = 5000f;
    private bool isInteractTechInfoUI = false;  // 테크 정보 UI와 상호작용 여부
    private bool isMouseHolding = false;        // 마우스 클릭 유지 상태
    private float nextUpgradeInterval = 0.07f;  // 마우스 클릭 유지 시, 다음 업그레이드가 될 때까지의 시간 텀
    private Coroutine upgradeIntervalCoroutine; // 마우스 클릭 유지 시, 다음 업그레이드까지 실행될 코루틴

    private void Start()
    {
        leftX = gameObject.transform.position.x - totalRectTransform.rect.width / 2f - 5f;

        InitMouseClick();
    }

    private void OnDestroy()
    {
        DestroyMouseClick();
    }

    // 데이터 등록
    public void RegisterState(TechState techState)
    {
        this.techState = techState;

        // 버튼 활성화 여부 설정
        GameManager.instance.OnCurrentGoldAmountChanged += OnCheckTechActive;

        // === 수정 필요 ===
        if (techState.techData.techKind == TechKind.Job)
            PeopleManager.Instance.OnAreaPeopleCountChanged += CurrentCapacityChange;

        if (techState.techData.areaType == AreaType.Barrack)
            GameManager.instance.OnClickGoldMultiplyChanged += ChangeMultiplyValueInClickGold;

        // 아직 잠겨 있는 상태
        if (techState.lockState == LockState.Block)
            TechViewer.instance.CheckUnlockPreTech(techState.techData);
        // 이미 열 수 있는 상태면, 바로 해제
        else 
            OnCheckTechActive();
    }

    // 상태 제거하는 함수
    public void RemoveState()
    {
        UnlockUI();
        if (techState == null)
            return;

        // 모두 초기화
        buttonBG.interactable = false;
        textCost.color = baseColor;

        // 모두 제거
        GameManager.instance.OnCurrentGoldAmountChanged -= OnCheckTechActive;

        if (techState.techData.areaType == AreaType.Barrack)
            GameManager.instance.OnClickGoldMultiplyChanged -= ChangeMultiplyValueInClickGold;

        // === 수정 필요 ===
        if (techState.techData.techKind == TechKind.Job)
            PeopleManager.Instance.OnAreaPeopleCountChanged -= CurrentCapacityChange;

        techState = null;
    }

    // 비용이 충분하지 않다면, 비활성화
    // 현재 수용량이 최대 수용량보다 적어서 개발 가능한지 확인
    public void OnCheckTechActive()
    {
        long amount = GameManager.instance.GetCurrentGoldAmount();
        // 선행 조건이 다 해결되지 않았다면, 물음표 상태로 표시
        if (techState.lockState == LockState.Block)
            return;

        imageIcon.sprite = techState.techData.techIcon;
        textName.text = techState.techData.techName;

        PrintCost();
        PrintLevelOrCapacity();

        // If structure level up is pending, disable this button.
        if (techState.techData.techKind == TechKind.Structure)
        {
            var structure = StructuresController.Instance.GetStructureApperance(techState.techData);
            if (structure != null && structure.IsLevelUpPending)
            {
                buttonBG.interactable = false;
                textCost.color = UnityEngine.Color.red;
                textCost.text = "건설이 필요합니다!";
                return;
            }
        }

        // 비활성화 또는 수용량 여유가 있는 경우, 만렙이 아닌 경우
        if (amount < techState.requaireAmount || techState.CheckCapacity() == false || techState.isMaxLevel())
        {
            buttonBG.interactable = false;
            textCost.color = UnityEngine.Color.red;
        }
        // 활성화
        else
        {
            buttonBG.interactable = true;
            textCost.color = UnityEngine.Color.green;
        }
    }

    // 잠겨 있을 때, UI 상태를 물음표로 변경
    public void UnlockUI()
    {
        imageIcon.sprite = unlockIcon;
        textName.text = "????";
        textCost.text = "????";
        textLevel.text = "??";
    }

    // 현재 노드의 해금 여부를 반환
    public LockState GetTechUnlock()
    {
        return techState.lockState;
    }

    // 현재 노드의 해금 가능하다고 설정
    public void SetTechUnlock()
    {
        if(techState.lockState == LockState.Block)
            techState.lockState = LockState.CanUnlock;
    }

    // 최대 수용량(유사 최대 레벨) 증가
    public void IncreaseMaxCapacity(int amount)
    {
        techState.maxCapacity += amount;
        OnCheckTechActive();
    }

    [ContextMenu("Do Something")]
    public void ModifyTest()
    {
        ModifyCurrentCapacity(-1);
    }

    // 현재 수용량 변경
    public void ModifyCurrentCapacity(int amount)
    {
        // techState.curCapacity += amount;
        PrintLevelOrCapacity();
        OnCheckTechActive();
    }

    // 현재 수용량(인원수) 변경
    private void CurrentCapacityChange()
    {
        techState.curCapacity = PeopleManager.Instance.Count(techState.techData.areaType);
        OnCheckTechActive(); // 잉여 인력이 변경되면서 추가 === 수정 필요 ===
        PrintLevelOrCapacity();

        // 인원 수 변경된 것에 따라 다시 테크 UI 적용
        if (isInteractTechInfoUI)
            PrintTechInfo();
    }

    // 클릭 당 배수가 변경될 때마다, 호출
    private void ChangeMultiplyValueInClickGold()
    {
        if (isInteractTechInfoUI)
            PrintTechInfo();
    }

    // 레벨업 확인
    private void CheckTechLevelUp()
    {
        // 비용 충분한지 확인
        long goldAmount = GameManager.instance.GetCurrentGoldAmount();
        if (goldAmount < techState.requaireAmount)
            return;

        OperateTechLevelUp();
    }

    // 레벨업 
    private void OperateTechLevelUp()
    {
        // 게임 클리어 버튼 누르면, 더 이상 작동 x
        if (techState.techData.isClearTech)
            buttonBG.interactable = false;

        long minusAmount = -1 * techState.requaireAmount;
        techState.LevelUp();

        if (techState.techData.techKind == TechKind.Special && techState.currentLevel == 1)
        {
            CatGodManager.TrySpawnCatGod(techState.techData.catGodType);
        }

        // 외형 변경
        GameManager.instance.ModifyStructureLevel(techState.techData, techState.currentLevel); ;

        // 현재 금액 감소
        GameManager.instance.AddCurrentGoldAmount(minusAmount);

        // 효과 적용
        foreach (var effect in techState.techData.effects)
        {
            effect.ApplyTechEffect();
        }

        // 업데이트
        PrintCost();
        PrintLevelOrCapacity();
        PrintTechInfo();

        // 다음 기술의 선행 기술들 확인
        foreach (var nextTech in techState.techData.postTeches)
        {
            TechViewer.instance.CheckUnlockPreTech(nextTech);
        }

        //// 구조물 정보 업데이트
        //if (techState.techData.techKind == TechKind.Structure)
        //    TechViewer.instance.ActiveStructureInfo(techState);
    }

    // 업그레이드 비용 출력
    private void PrintCost()
    {
        //필요수량이 0이 아닐경우 무직 -1 텍스트 출력
        if (techState.requaireAmount > 0)
            textCost.text = "금 " + FuncSystem.Format(techState.requaireAmount);
        else
            textCost.text = "무직 1";

    }

    // 레벨 또는 현재 수용량 출력
    private void PrintLevelOrCapacity()
    {
        if (techState.lockState == LockState.Block)
            return;

        string value = techState.techData.isUsingLevel ? techState.currentLevel.ToString() : techState.curCapacity.ToString();
        textLevel.text = value;
    }

    // 마우스 올려 놓기
    public void OnPointerEnter(PointerEventData eventData)
    {
        isInteractTechInfoUI = true;
        PrintTechInfo();
    }

    // 마우스가 빠져 나감
    public void OnPointerExit(PointerEventData eventData)
    {
        isInteractTechInfoUI = false;
        OnInactiveInfo?.Invoke();

        if (techState.techData.techKind == TechKind.Structure)
            TechViewer.instance.InactiveStructureInfo();
    }

    // 설명 문서 출력
    private void PrintTechInfo()
    {
        // 위치 초기화가 되지 않았을 때만 진행
        if (upperY == 5000f)
        {
            Vector3[] corners = new Vector3[4];
            totalRectTransform.GetWorldCorners(corners);
            upperY = corners[1].y;
            leftX = corners[1].x;
        }

        // 테크 정보 위치 설정
        Vector3 loc = new Vector3(leftX, upperY, 0);

        // 테크 정보 데이터 설정
        string techName;
        string techDescription;
        Sprite techIcon;
        if (techState.lockState == LockState.Block)
        {
            techName = "????";
            techDescription = "아직 확인할 수 없습니다.";
            techIcon = unlockIcon;
        }
        else
        {
            techName = techState.techData.techName;
            techDescription = SetDescriptionContent();
            techIcon = techState.techData.techIcon;
        }

        OnActiveInfo?.Invoke(techName, techDescription, techIcon, loc);

        //// 구조물 정보 출력
        //if (techState.techData.techKind == TechKind.Structure)
        //    TechViewer.instance.ActiveStructureInfo(techState);
    }


    // 설명 문서 작성
    private string SetDescriptionContent()
    {
        // 기존 설명
        string description = techState.techData.techDescription + "\n";

        // 현재 단계 효과 계산
        IncreaseInfo increaseInfo = GameManager.instance.GetIncreaseGoldInfo(techState.techData.areaType);

        long curLinearAmount = increaseInfo.clickTotalLinear * (100 + increaseInfo.clickRate) / 100;
        long curPeriodAmount = increaseInfo.periodTotalLinear * (100 + increaseInfo.periodRate) / 100;

        // 다음 단계 효과 계산
        TechTotalUpgradeAmount next = techState.CalculateNextEffectAmount(increaseInfo);

        if (techState.techData.printTech.isAcquireClickGold || techState.techData.printTech.isAcquirePeriodGold)
        {
            // === 클릭 시 금 얻는 효과 출력 ===
            long nextClickLinear = increaseInfo.clickTotalLinear + next.clickLinearAmount;
            long nextClickRate = increaseInfo.clickRate + next.clickRateAmount;

            long resultClickAmount = nextClickLinear * (100 + nextClickRate) / 100;
            // 클릭당 금 변경점
            if (techState.techData.printTech.isAcquireClickGold)
            {
                string clickLine;
                if (curLinearAmount != resultClickAmount)
                    clickLine = $"클릭당 금:<color=#00FF00>{FuncSystem.Format(curLinearAmount)}</color>▶<color=#00FF00>{FuncSystem.Format(resultClickAmount)}</color>\n";
                else
                    clickLine = $"클릭당 금:{FuncSystem.Format(curLinearAmount)}▶{FuncSystem.Format(resultClickAmount)}\n";

                description += clickLine;
            }

            // === 주기적으로 세금 얻는 효과 출력 ===
            int peopleCount = PeopleManager.Instance.Count(techState.techData.areaType);

            long nextPeriodLinear = increaseInfo.periodTotalLinear + next.periodLinearAmount + peopleCount * next.periodLinearAmountAddition;
            long nextPeriodRate = increaseInfo.periodRate + next.periodRateAmount;

            long resultPeriodAmount = (nextPeriodLinear) * (100 + nextPeriodRate) / 100;

            if (techState.techData.printTech.isAcquirePeriodGold)
            {
                // 초당 금 변경점
                string periodLine;
                if (curPeriodAmount != resultPeriodAmount)
                    periodLine = $"초당 금:<color=#00FF00>{FuncSystem.Format(curPeriodAmount)}</color>▶<color=#00FF00>{FuncSystem.Format(resultPeriodAmount)}</color>\n";
                else
                    periodLine = $"초당 금:{FuncSystem.Format(curPeriodAmount)}▶{FuncSystem.Format(resultPeriodAmount)}\n";

                description += periodLine;

                long curTotalPeriodAmount = GameManager.instance.GetPeriodIncreaseTotalAmount();
                
                decimal curTotalPeriodPercent = 0;
                decimal nextTotalPeriodPercent = 0;

                if (curTotalPeriodAmount != 0)
                    curTotalPeriodPercent = (decimal)curPeriodAmount / (decimal)curTotalPeriodAmount * 100;
                else
                    curTotalPeriodPercent = 0;

                long nextTotalPeriodAmount = resultPeriodAmount - curPeriodAmount + curTotalPeriodAmount;
                if (nextTotalPeriodAmount != 0)
                    nextTotalPeriodPercent = (decimal)resultPeriodAmount / (decimal)(nextTotalPeriodAmount) * 100;
                else
                    nextTotalPeriodPercent = 0;

                // 초당 금 총 지분
                string periodTechPercentLine;
                if (curPeriodAmount != resultPeriodAmount)
                    periodTechPercentLine = $"금 생산 지분:<color=#00FF00>{curTotalPeriodPercent.ToString("F2")}</color>%▶<color=#00FF00>{nextTotalPeriodPercent.ToString("F2")}</color>%\n";
                else
                    periodTechPercentLine = $"금 생산 지분:{curTotalPeriodPercent.ToString("F2")}%▶{nextTotalPeriodPercent.ToString("F2")}%\n";

                description += periodTechPercentLine;
            }
        }

        // 잉여 인력 생성 주기 효과 출력
        if (techState.techData.printTech.isReducePeoplePeriod)
        {
            float curRespawnPeriod = GameManager.instance.GetRespawnTime();

            string respawnLine;
            if (curRespawnPeriod != next.respawnTime)
                respawnLine = $"무직 생성 주기:<color=#00FF00>{curRespawnPeriod.ToString("F3")}</color>s▶<color=#00FF00))>{next.respawnTime.ToString("F3")}</color>s\n";
            else
                respawnLine = $"무직 생성 주기:{curRespawnPeriod.ToString("F3")}s▶{next.respawnTime.ToString("F3")}s\n";

            description += respawnLine;
        }

        // 피라미드 진척도 출력
        if(techState.techData.printTech.isPyramid)
        {
            string pyramidLine;

            pyramidLine = $"진척도:<color=#00FF00>{techState.currentLevel}</color>/{techState.techData.maxLevel}▶<color=#00FF00>{techState.currentLevel + 1}</color>/{techState.techData.maxLevel}";
            description += pyramidLine;
        }

        // 자동 클릭으로 획득하는 금의 양 출력
        if(techState.techData.printTech.isAutoClickGold)
        {
            string autoClickGold = string.Empty;
            long curGoldClickAmount = GameManager.instance.GetTotalClickGoldAmount();
            long curAutoClickCount = GameManager.instance.GetAutoClickCount();
            float curAutoClickInterval = GameManager.instance.GetAutoClickInterval();
            long curAutoGoldAmount = (long)(((decimal)curGoldClickAmount * (decimal)curAutoClickCount) / (decimal)curAutoClickInterval);
            GameManager.instance.curAutoGoldAmount = curAutoGoldAmount;

            long nextAutoClickCount = curAutoClickCount + next.autoClickCount;
            float nextAutoClickInterval = Mathf.Max(curAutoClickInterval + next.autoClickInterval, 0.999f);
            long nextAutoGoldAmount = (long)(((decimal)curGoldClickAmount * (decimal)nextAutoClickCount) / (decimal)nextAutoClickInterval);

            if(curAutoClickCount != nextAutoClickCount)
                autoClickGold += $"자동 클릭 횟수:<color=#00FF00>{curAutoClickCount}</color>▶<color=#00FF00>{nextAutoClickCount}</color>\n";

            if (curAutoClickInterval != nextAutoClickInterval)
                autoClickGold += $"자동 클릭 주기:<color=#00FF00>{curAutoClickInterval:F2}s</color>▶<color=#00FF00>{nextAutoClickInterval:F2}s</color>\n";

            autoClickGold += $"자동 클릭으로 초당 금 획득량\n<color=#00FF00>{FuncSystem.Format(curAutoGoldAmount)}</color>▶<color=#00FF00>{FuncSystem.Format(nextAutoGoldAmount)}</color>\n";
            description += autoClickGold;
        }

        // 주기적으로 얻는 금에 대한 전체 효과
        if(techState.techData.printTech.isAcquireTotalPeriodGold)
        {
            string periodTotalGold = string.Empty;

            Dictionary<AreaType, TechTotalUpgradeAmount> increaseTotalAmount = techState.CalculateNextEffectAmountServeralType();

            long nextPeriodTotalGold = 0;
            foreach(var data in increaseTotalAmount)
            {
                IncreaseInfo info = GameManager.instance.GetIncreaseGoldInfo(data.Key);
                long count = PeopleManager.Instance.Count(data.Key);

                nextPeriodTotalGold += count * (info.clickEachLinear + info.clickEachLinearAddition + data.Value.periodLinearAmountAddition) * (100 + info.periodRate + data.Value.periodRateAmount) / 100;
            }

            long curPeriodTotalGold = GameManager.instance.GetPeriodBaseIncreaseTotalAmount();

            periodTotalGold = $"초당 금:<color=#00FF00>{FuncSystem.Format(curPeriodTotalGold)}</color>▶<color=#00FF00>{FuncSystem.Format(nextPeriodTotalGold)}</color>\n";
            description += periodTotalGold;
        }

        // 아이템 효과
        if(techState.techData.printTech.isItem)
        {
            string durationLine = string.Empty;
            string multiplier = string.Empty;

            float curCamelBonusDuration = CamelEventSystem.instance.GetBonusDuration();
            float curCamelBonusMultiplier = CamelEventSystem.instance.GetBonusMultiplier();

            float nextCamelBonusDuration = curCamelBonusDuration + next.camelBonusDurationLinear;
            float nextCamelBonusMultiplier = curCamelBonusMultiplier + next.camelBonusMultiplierLinear;

            if(curCamelBonusMultiplier != nextCamelBonusMultiplier)
                multiplier = $"아이템 획득 시, 배수 : <color=#00FF00>x{curCamelBonusMultiplier:F0}</color>▶<color=#00FF00>x{nextCamelBonusMultiplier:F0}</color>\n";
            if (curCamelBonusDuration != nextCamelBonusDuration)
                durationLine = $"아이템 유지 시간 : <color=#00FF00>{curCamelBonusDuration:F2}s</color>▶<color=#00FF00>{nextCamelBonusDuration:F2}s</color>\n";

            description += multiplier;
            description += durationLine;
        }

        // 권위에 대한 효과
        if(techState.techData.printTech.isFever)
        {
            string feverLine = string.Empty;
            float curFever = AuthorityManager.instance.GetTotalFeverMultiplier();
            float nextFever = curFever + next.feverAmount;

            feverLine = $"피버 때, 배수:<color=#00FF00>x{curFever:F0}</color>▶<color=#00FF00>x{nextFever:F0}</color>\n";
            description += feverLine;
        }

        return description;
    }

    // 마우스 클릭 중 + 마우스 클릭 시작
    private void OnClickStart()
    {
        // 이미 실행 중인 코루틴 제거
        if (upgradeIntervalCoroutine != null)
        {
            StopCoroutine(upgradeIntervalCoroutine);
            upgradeIntervalCoroutine = null;
        }
        
        if(isMouseHolding == false)
        {
            GameLogger.Instance.click.AddUpgradeClick();
        }
        isMouseHolding = true;
        upgradeIntervalCoroutine = StartCoroutine(OperateUpgradeContinue());
    }

    // 마우스 클릭 해제
    private void OnClickEnd()
    {
        isMouseHolding = false;

        if (upgradeIntervalCoroutine != null)
        {
            StopCoroutine(upgradeIntervalCoroutine);
            upgradeIntervalCoroutine = null;
        }
    }

    // 마우스를 계속 누를 때, 업그레이드 지속
    IEnumerator OperateUpgradeContinue()
    {
        float curTime = 0f;
        bool isUpgradeSuccess = false;
        while (isMouseHolding && buttonBG.interactable)
        {
            curTime += Time.deltaTime;
            if(curTime >= nextUpgradeInterval)
            {
                CheckTechLevelUp();
                isUpgradeSuccess = true;
                break;
            }
            yield return null;
        }

        // 업그레이드 성공했으니 코루틴 시작
        if (isUpgradeSuccess)
            upgradeIntervalCoroutine = StartCoroutine(OperateUpgradeContinue());
        // 업그레이드 실패했으면, 바로 반환
        else
            upgradeIntervalCoroutine = null;
    }

    // 마우스 클릭 시작 및 끝과 호환
    private void InitMouseClick()
    {
        buttonBG.TryGetComponent(out TechEachButton techEachButton);
        if (techEachButton == null) return;

        techEachButton.OnClickStart += OnClickStart;
        techEachButton.OnClickEnd += OnClickEnd;
    }

    // 마우스 클릭 시작 및 끝을 제거
    private void DestroyMouseClick()
    {
        buttonBG.TryGetComponent(out TechEachButton techEachButton);
        if (techEachButton == null) return;

        techEachButton.OnClickStart -= OnClickStart;
        techEachButton.OnClickEnd -= OnClickEnd;
    }
}
