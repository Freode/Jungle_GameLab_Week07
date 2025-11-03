using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


public class AuthorityInfoUI : MonoBehaviour
{
    // 레벨
    // 게이지 바

    public TextMeshProUGUI textLevel;           // 레벨 텍스트
    public Image imageExpBack;                  // 게이지 뒷배경
    public Image imageExpFront;                 // 게이지 앞배경
    public TextMeshProUGUI textExpValue;        // 게이지 비율 및 수치
    public TextMeshProUGUI textAuthorityPoint;  // 권위 포인트
    public List<AuthorityLevel> requirements;   // 권위 경험치

    public BaseStructureEffect authorityLevelUpEffect;  // 권위 레벨이 상승했을 때, 기본적으로 부여하는 테크 데이터

    public ParticleSystem levelUpParticle;    // 레벨업 파티클

    public BaseTechEffect announceAuthroityTab;// 레벨 업으로 권위 탭 강조

    [SerializeField] bool isDebug = false;  // 디버깅 모드


    private long _curExp = 0;               // 현재 경험치
    private int _level = 1;                 // 레벨
    private long _multiple = 100;           // 경험치 배수

    public int Level { get => _level; private set => _level = value; }

    void Start()
    {
        AddInfinityLevel(100);
        UpdateAuthorityExperience();
        GameManager.instance.OnAuthorityLevelStackChanged += PrintAuthorityPoint;
    }

    private void Update()
    {
        if (isDebug)
            IncreaseAuthorityExp(100);
    }

    private void OnDestroy()
    {
        GameManager.instance.OnAuthorityLevelStackChanged -= PrintAuthorityPoint;
    }

    // 권위 게이지 업데이트
    private void UpdateAuthorityExperience()
    {
        // 만렙 달성
        if(_level >= requirements.Count)
        {
            textExpValue.text = "MAX LEVEL";
            imageExpFront.transform.localScale = new Vector3(0f, 1f, 1f);
            return;
        }

        long maxExp = requirements[_level - 1].requireExp;
        decimal expRate = (decimal)_curExp / (decimal)maxExp;

        textExpValue.text = $"{FuncSystem.Format(_curExp)}/{FuncSystem.Format(maxExp)}({expRate * 100:F2}%)";
        imageExpFront.transform.localScale = new Vector3((float)expRate, 1f, 1f);

        if (_curExp >= maxExp)
            IncreaseAuthroityLevel();
    }

    // 레벨 업
    private void IncreaseAuthroityLevel()
    {
        _curExp -= requirements[_level - 1].requireExp;

        // 효과 발동
        ++_level;
        requirements[_level - 1].unlockTech?.ApplyTechEffect();
        requirements[_level - 1].unlockTab?.ApplyTechEffect();
        announceAuthroityTab?.ApplyTechEffect();

        textLevel.text = $"Lv. {_level.ToString("D3")}";
        UpdateAuthorityExperience();
        authorityLevelUpEffect?.ApplyTechEffect();
        GameManager.instance.AuthorityLevelUp();
        levelUpParticle.Play();
    }

    // 권위 경험치 변경
    public void IncreaseAuthorityExp(long amount)
    {
        _curExp += (amount * _multiple / 100);
        UpdateAuthorityExperience();
    }

    // 권위 경험치 배수 증가
    public void IncreaseExpMutliple(long amount)
    {
        _multiple += amount;
    }

    // 권위 경험치 배수 가져오기
    public long GetExpMultiplier()
    {
        return _multiple;
    }

    private void AddInfinityLevel(int maxLevel)
    {
        long exp = requirements[requirements.Count - 1].requireExp;
        for(int i = requirements.Count; i < maxLevel; i++)
        {
            exp = (exp * 108) / 100;
            requirements.Add(AuthorityLevel.Create(exp));
        }
    }

    private void PrintAuthorityPoint()
    {
        textAuthorityPoint.text = $"남는 권위 포인트 : {GameManager.instance.GetAuthroityLevelUpStack()}P";
    }
}
