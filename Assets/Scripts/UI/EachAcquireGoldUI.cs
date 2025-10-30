using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EachAcquireGoldUI : MonoBehaviour
{

    // 0. 이름
    // 1. 얻는 금의 양(단위 환산)
    // 2. 비중
    // 3. 

    public Image ImageIcon;                 // 테크 아이콘 출력
    public TextMeshProUGUI textTechName;        // 테크 이름 출력
    public TextMeshProUGUI textAcqurieGold;     // 금 획득 출력
    public TextMeshProUGUI textRateGold;        // 금 비율 출력

    [Header("Temporary")]
    public Color barColor;
    public Image imgaeBarBack;

    private AreaType _areaType;               // 출력할 정보

    private void Start()
    {
        GameManager.instance.OnPeriodIncreaseAmountChanged += PrintData;
    }

    private void OnDestroy()
    {
        GameManager.instance.OnPeriodIncreaseAmountChanged -= PrintData;
    }

    public void Init(AreaType areaType)
    {
        _areaType = areaType;
        textTechName.text = FuncSystem.ModifySpecialToArea(areaType, "");
        UpdateIcon();
        PrintData();
    }

    void PrintData()
    {
        IncreaseInfo increaseInfo = GameManager.instance.GetIncreaseGoldInfo(_areaType);

        long curPeriodAmount = increaseInfo.periodTotalLinear * (100 + increaseInfo.periodRate) / 100;
        long curTotalPeriodAmount = GameManager.instance.GetPeriodIncreaseTotalAmount();
        if (curTotalPeriodAmount == 0) curTotalPeriodAmount = 1;

        decimal curTotalPeriodRate = (decimal)curPeriodAmount / (decimal)curTotalPeriodAmount;
        decimal curTotalPeriodPercent = curTotalPeriodRate * 100;
        if (_areaType == AreaType.Total)
        {
            curPeriodAmount = GameManager.instance.GetPeriodIncreaseTotalAmount();
            if(curPeriodAmount != 0)
                curTotalPeriodPercent = 100;

            curTotalPeriodPercent = curTotalPeriodRate * 100;
        }

        // 단위 시간당 기본 생산량 표시
        textAcqurieGold.text = $"<color=#00FF00>{FuncSystem.Format(curPeriodAmount)}</color>";

        // 백분율 표시
        textRateGold.text = $"<color=#00FF00>{curTotalPeriodPercent:F2}%</color>";

        // 게이지바 업데이트
        imgaeBarBack.transform.localScale = new Vector3((float)curTotalPeriodRate, imgaeBarBack.transform.localScale.y, imgaeBarBack.transform.localScale.z);

        // 아이콘 설정
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (TechViewer.instance != null && TechViewer.instance.techInfoes != null)
        {
            foreach (var techInfo in TechViewer.instance.techInfoes)
            {
                foreach (var techData in techInfo.techDatas)
                {
                    if (techData.areaType == _areaType)
                    {
                        ImageIcon.sprite = techData.techIcon;
                        ImageIcon.gameObject.SetActive(true);
                        return;
                    }
                }
            }
        }
    }
}
