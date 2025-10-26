using System.Collections.Generic;
using System;
using UnityEngine;

// 증가량 구조체
public class IncreaseInfo
{
    public long clickEachLinear = 0;        // 1인당 얻는 선형적임 세금
    public long clickEachLinearAddition = 0;// 1인당 얻는 선형적인 추가 세금
    public long clickTotalLinear = 0;       // 총 얻는 선형적인 세금
    public long clickRate = 0;
    public long periodEachLinear = 0;
    public long periodEachLinearrAddition = 0;// 주기적으로 얻는 선형적인 추가 세금
    public long periodTotalLinear = 0;
    public long periodRate = 0;
};

// 구조체 레벨에 따른 변화
[System.Serializable]
public class LevelAppearance
{
    public int level;
    public Sprite sprite;
    public Vector3 scale = Vector3.one;
    public List<BaseStructureEffect> effects;
}

[Serializable]
public class StructureData
{
    public TechData techData;
    public GameObject areaStructure;
}


// 테크 유형에 따른 배열
[System.Serializable]
public struct TechKindInfo
{
    public TechKind techKind;
    public List<TechData> techDatas;
}

// 테크 업그레이드 유형 출력
[System.Serializable]
public struct TechPrintUpgradeKind
{
    public bool isAcquireClickGold;
    public bool isAcquirePeriodGold;
    public bool isReducePeoplePeriod;
    public bool isPyramid;
    public bool isAutoClickGold;
}

// 테크 효과 값이 담긴 구조체
[System.Serializable]
public struct TechTotalUpgradeAmount
{
    public long clickLinearAmount;
    public long clickRateAmount;
    public long periodLinearAmount;
    public long periodRateAmount;
    public float respawnTime;
    public long autoClickCount;
    public float autoClickInterval;
}
