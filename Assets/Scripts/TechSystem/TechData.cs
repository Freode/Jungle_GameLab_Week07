using UnityEngine;
using System;
using System.Collections.Generic;

public enum LockState
{
    Block = 0,
    CanUnlock,
    Complete,
};

public enum TechKind
{
    None = 0,
    Structure,
    Job,
    Special,
    Special_Previous,
    Power,
};

// 데이터를 저장하는 클래스
[CreateAssetMenu(fileName = "TechData", menuName = "Scriptable Objects/TechData")]
public class TechData : ScriptableObject
{
    public TechKind techKind;               // 테크 유형 (좀 더 큰 단위)
    public AreaType areaType;               // 존재할 구역 타입(나중에 수정)
    public string techName;                 // 기술 이름
    public string techNamePrint;            // 출력용 기술 이름
    public string techDescription;          // 기술 설명
    public Sprite techIcon;                 // 기술 아이콘
    public long baseRequiredGold;            // 기본 요구 바이트 (레벨 1)
    public float increaseGoldValue;         // 레벨 당 증가하는 골드 양
    public int maxLevel;                    // 최대 레벨 - 변경 불가능
    public int baseCapacity;                // 기본 수용량 (레벨 1)
    public bool isUsingLevel;               // 레벨을 사용하는지 여부
    public bool isClearTech;                // 클리어 테크
    public Vector3 localPos;                // 연구 위치
    public List<TechData> preTeches;        // 선행 기술 목록
    public List<TechData> postTeches;       // 다음 기술 목록
    public List<BaseTechEffect> effects;    // 해금 시, 적용할 효과 목록
    public TechPrintUpgradeKind printTech;  // 효과 출력할 테크 종류
    public int reduceIncreaseGoldValueLevel;// 레벨 당 증가하는 골드 양을 줄이는 레벨 단위
    public float reduceIncreaseGoldValue;   // 레벨 당 증가하는 골드 양을 줄이는 양
    public CatGodType catGodType;           // 소환할 고양이 신 종류
}

// 상태를 저장하는 클래스
[System.Serializable] 
public class TechState
{
    public TechData techData;               // 어떤 기술의 상태인지 원본 참조
    public int currentLevel;                // 현재 레벨
    public int curCapacity;                 // 현재 수용량
    public float curIncreaseGoldValue;      // 현재 골드 증가량
    public int maxCapacity;                 // 최대 수용량
    public LockState lockState;             // 연구 가능 상태
    public long requaireAmount = 0;         // 요구하는 양
    public int curTechUIIdx;                // 현재 UI의 위치
    private Dictionary<AreaType, TechTotalUpgradeAmount> _effectDict;   // 업그레이드 수치 저장

    // 생성자
    public TechState(TechData data)
    {
        techData = data;
        currentLevel = 0;
        curCapacity = 0;
        curIncreaseGoldValue = data.increaseGoldValue;
        maxCapacity = data.baseCapacity;
        lockState = LockState.Block;
        requaireAmount = data.baseRequiredGold;
    }

    // 레벨업 적용
    public void LevelUp()
    {
        lockState = LockState.Complete;
        // ++curCapacity; 수정 필요
        ++currentLevel;

        if (techData.reduceIncreaseGoldValueLevel != 0 && techData.reduceIncreaseGoldValue != 0)
        {
            if (currentLevel % techData.reduceIncreaseGoldValueLevel == 0)
                curIncreaseGoldValue = Math.Max(curIncreaseGoldValue - techData.reduceIncreaseGoldValue, 1.01f);
        }

        requaireAmount = (long)Math.Floor((decimal)requaireAmount * (decimal)curIncreaseGoldValue);
    }

    // 여러 레벨 한 번에 올리기 시도
    public (int actualLevels, long totalCost) TryMultiLevelUp(int targetLevels, long currentGold)
    {
        if (targetLevels <= 0 || isMaxLevel()) 
            return (0, 0);

        int possibleLevels = 0;
        long totalRequiredGold = 0;
        long tempRequireAmount = requaireAmount;
        float tempIncreaseValue = curIncreaseGoldValue;
        
        // 건물인 경우 다음 진화 레벨 찾기
        int nextEvolutionLevel = int.MaxValue;
        if (techData.techKind == TechKind.Structure)
        {
            var structure = StructuresController.Instance.GetStructureApperance(techData);
            if (structure != null)
            {
                // 현재 진화가 필요한 상태면 중단
                if (structure.IsLevelUpPending)
                    return (0, 0);

                // 현재 레벨보다 높은 가장 가까운 진화 레벨 찾기
                var levelAppearances = structure.levelAppearances;
                if (levelAppearances != null)
                {
                    foreach (var appearance in levelAppearances)
                    {
                        if (appearance.level > currentLevel)
                        {
                            nextEvolutionLevel = appearance.level;
                            break;
                        }
                    }
                }
            }
        }
        
        // 일꾼인 경우 현재 가용한 무직자 수 확인
        int availableWorkers = 0;
        if (techData.techKind == TechKind.Job)
        {
            availableWorkers = PeopleManager.Instance.Count(AreaType.Normal);
            if (availableWorkers <= 0)
                return (0, 0); // 가용 무직자가 없으면 업그레이드 불가
        }

        // 가능한 레벨업 횟수와 필요한 총 골드 계산
        for (int i = 0; i < targetLevels; i++)
        {
            // 최대 레벨 체크
            if (currentLevel + i >= techData.maxLevel && techData.maxLevel != 0)
                break;
                
            // 다음 레벨이 진화 레벨보다 커지면 중단
            int nextLevel = currentLevel + i + 1;
            if (nextLevel > nextEvolutionLevel)
                break;

            // 일꾼인 경우 가용 무직자 수 체크
            if (techData.techKind == TechKind.Job)
            {
                if (i + 1 > availableWorkers) // 이미 할당한 수 + 1이 가용 무직자 수보다 크면 중단
                    break;
            }

            // 누적 비용이 현재 보유 골드를 초과하면 중단
            if (totalRequiredGold + tempRequireAmount > currentGold)
                break;

            totalRequiredGold += tempRequireAmount;
            possibleLevels++;

            // 다음 레벨의 요구 골드 계산
            if (techData.reduceIncreaseGoldValueLevel != 0 && techData.reduceIncreaseGoldValue != 0)
            {
                if ((currentLevel + i + 1) % techData.reduceIncreaseGoldValueLevel == 0)
                    tempIncreaseValue = Math.Max(tempIncreaseValue - techData.reduceIncreaseGoldValue, 1.01f);
            }
            tempRequireAmount = (long)Math.Floor((decimal)tempRequireAmount * (decimal)tempIncreaseValue);
        }

        // 실제 레벨업 적용
        for (int i = 0; i < possibleLevels; i++)
        {
            LevelUp();
        }

        return (possibleLevels, totalRequiredGold);
    }

    // 현재 수용량이 최대 수용량보다 적은지 확인
    public bool CheckCapacity()
    {
        // 바로 활성화 가능으로 판단
        if (techData.isUsingLevel)
            return true;

        // 잉여 인원이 있는지 확인 ==== 수정 필요 ====
        if (PeopleManager.Instance.Count(AreaType.Normal) == 0)
            return false;

        // 최대 레벨인 경우
        if (isMaxLevel())
            return false;

        return curCapacity < maxCapacity;
    }

    // 최대 레벨인 경우, 더 이상 업그레이드 불가능
    public bool isMaxLevel()
    {
        return (techData.maxLevel != 0 && currentLevel >= techData.maxLevel);
    }

    // 해당 레벨이 진화가 필요한 레벨인지 확인
    private bool IsEvolutionLevel(int level)
    {
        // 건물마다 진화 레벨이 다를 수 있으므로, 필요에 따라 아래 조건을 수정하세요
        return level % 15 == 0;  // 15, 30, 45, ... 레벨에서 진화 필요
    }

    // 다음 단계가 적용된 효과 계산
    public TechTotalUpgradeAmount CalculateNextEffectAmount(IncreaseInfo increaseInfo)
    {
        TechTotalUpgradeAmount amount = default;

        foreach(BaseTechEffect effect in techData.effects)
        {
            if (effect is AddPeriodIncreaseGoldAmountLinearEffect periodLinearEffect)
                amount.periodLinearAmount += periodLinearEffect.amount;
            else if (effect is AddPeriodIncreaseGoldAmountRateEffect periodRateEffect)
                amount.periodRateAmount += periodRateEffect.amount;
            else if (effect is AddClickIncreaseGoldAmountLinearEffect clickLinearEffect)
                amount.clickLinearAmount += clickLinearEffect.amount;
            else if (effect is AddClickIncreaseGoldAmountRateEffect clickRateEffect)
                amount.clickRateAmount += clickRateEffect.amount;
            else if (effect is AddRespawnUselessPeopleEffect respawnPeopleEffect)
                amount.respawnTime = GameManager.instance.GetNextRespwanTime(respawnPeopleEffect.amount);
            else if (effect is AddAutoClickCountEffect autoClickCountEffect)
                amount.autoClickCount = autoClickCountEffect.amount;
            else if (effect is AddAutoClickIntervalEffect autoClickIntervalEffect)
                amount.autoClickInterval = autoClickIntervalEffect.amount;
            else if (effect is AddPerriodIncreaseGoldAmountLinearAdditionalEffect periodLinearAdditionalEffect)
                amount.periodLinearAmountAddition = periodLinearAdditionalEffect.amount;
            else if (effect is AddFeverMultiplierEffect feverMultiplierEffect)
                amount.feverAmount = feverMultiplierEffect.amount;
            else if (effect is AddCamelBonusDurationLinearEffect camelBonusDurationLinearEffect)
                amount.camelBonusDurationLinear = camelBonusDurationLinearEffect.amount;
            else if (effect is AddCamelBonusMultiplierLinearEffect camelBonusDurationMultiplierEffect)
                amount.camelBonusMultiplierLinear = camelBonusDurationMultiplierEffect.amount;
            else if (effect is AddFeverCountEffect feverCountEffect)
                amount.feverCount = feverCountEffect.amount;
            else if (effect is AddCamelBonusSpawnPercEffect camelBonusSpawnPrecEffect)
                amount.camelBonusPrec = camelBonusSpawnPrecEffect.amount;
        }

        return amount;
    }

    // 다음 단계의 여러 타입에 영향이 적용된 효과 계산
    public Dictionary<AreaType, TechTotalUpgradeAmount> CalculateNextEffectAmountServeralType()
    {
        if (_effectDict != null)
            return _effectDict;

        // Dictionary 생성
        _effectDict = new Dictionary<AreaType, TechTotalUpgradeAmount>();
        
        foreach(AreaType areaType in Enum.GetValues(typeof(AreaType)))
        {
            _effectDict.Add(areaType, new TechTotalUpgradeAmount());
        }

        // 데이터 효과 추가
        foreach (BaseTechEffect effect in techData.effects)
        {
            if (effect is AddPeriodIncreaseGoldAmountLinearEffect periodLinearEffect)
            {
                AreaType tempAreaType = periodLinearEffect.type;
                TechTotalUpgradeAmount total = _effectDict[tempAreaType];
                total.periodLinearAmount += periodLinearEffect.amount;
                _effectDict[tempAreaType] = total;
            }
                
            else if (effect is AddPeriodIncreaseGoldAmountRateEffect periodRateEffect)
            {
                AreaType tempAreaType = periodRateEffect.type;
                TechTotalUpgradeAmount total = _effectDict[tempAreaType];
                total.periodLinearAmount += periodRateEffect.amount;
                _effectDict[tempAreaType] = total;
            }

            else if (effect is AddPerriodIncreaseGoldAmountLinearAdditionalEffect periodLinearAdditionalEffect)
            {
                AreaType tempAreaType = periodLinearAdditionalEffect.areaType;
                TechTotalUpgradeAmount total = _effectDict[tempAreaType];
                total.periodLinearAmount += periodLinearAdditionalEffect.amount;
                _effectDict[tempAreaType] = total;
            }
        }

        return _effectDict;
    }
}
