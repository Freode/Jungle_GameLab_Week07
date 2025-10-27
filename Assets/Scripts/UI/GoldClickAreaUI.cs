using System.Collections;
using TMPro;
using UnityEngine;

public class GoldClickAreaUI : MonoBehaviour
{
    public TextMeshProUGUI textCurrentGoldAmount;       // 현재 골드 소지량
    public TextMeshProUGUI textClickAmount;             // 현재 골드 클릭 시, 획득량
    public TextMeshProUGUI textPeriodAmount;            // 주기적으로 얻는 골드 양 출력
    public Transform startPosAcquireGold;               // 금 얻었을 때, 출력 창 시작 위치
    public Transform endPosAcquireGold;                 // 금 얻었을 때, 출력 창 종료 위치
    public GameObject acquireGoldAmountPrefab;          // 클릭으로 금 획득 시, 출력할 UI 프리팹

    private long _localCurrentGold = 0;
    private long _localClickGold = 0;
    private int _localAuthorityMultiplier = 1;
    private string _localAuthorityColor = "000000";

    private float _interval = 0.05f;
    private float _curTime = 0f;
    private float _endTime = 0f;
    private Coroutine _animCoroutine;

    private void Start()
    {
        GameManager.instance.OnCurrentGoldAmountChanged += StartModifyCurrentGoldAmount;
        GameManager.instance.OnClickIncreaseTotalAmountChanged += PrintClickGoldAmount;
        GameManager.instance.OnPeriodIncreaseAmountChanged += PrintPeriodGoldAmount;
        GameManager.instance.OnClickIncreaseGoldAmount += PrintIncreaseGoldAmountWhenClicked;
        GameManager.instance.OnAuthorityMultiplierUpdate += PrintCurrentAuthorityMultiplier;
        GameManager.instance.OnClickGoldMultiplyChanged += PrintClickGoldAmount;
        AuthorityManager.instance.OnUpdateAuthorityInPeriodGold += PrintPeriodGoldAmount;

        _localCurrentGold = GameManager.instance.GetCurrentGoldAmount();
        PrintCurrentGoldAmount(_localCurrentGold);
    }

    private void OnDestroy()
    {
        GameManager.instance.OnCurrentGoldAmountChanged -= StartModifyCurrentGoldAmount;
        GameManager.instance.OnClickIncreaseTotalAmountChanged -= PrintClickGoldAmount;
        GameManager.instance.OnPeriodIncreaseAmountChanged -= PrintPeriodGoldAmount;
        GameManager.instance.OnClickIncreaseGoldAmount -= PrintIncreaseGoldAmountWhenClicked;
        GameManager.instance.OnAuthorityMultiplierUpdate -= PrintCurrentAuthorityMultiplier;
        GameManager.instance.OnClickGoldMultiplyChanged -= PrintClickGoldAmount;
        AuthorityManager.instance.OnUpdateAuthorityInPeriodGold -= PrintPeriodGoldAmount;
    }

    private void Update()
    {
        _curTime += Time.deltaTime;
    }

    // 골드 양 업데이트
    IEnumerator UpdateLocalGoldAmount()
    {
        float startTime = _curTime;
        decimal _startGold = _localCurrentGold;

        while (_curTime <= _endTime)
        {
            if (GameManager.instance.GetIsGameOver())
                break;

            decimal dt = (decimal)(_curTime - startTime) / (decimal)(_endTime - startTime);

            long finalAmount = GameManager.instance.GetCurrentGoldAmount();

            decimal nextAmountF = _startGold + (finalAmount - _startGold) * dt;
            long nextAmount = (long)nextAmountF;
            PrintCurrentGoldAmount(nextAmount);


            // 골드 양이 선형적으로 증가하는 애니메이션
            yield return new WaitForSeconds(_interval);

        }
        // 최종 양 재지정
        PrintCurrentGoldAmount(GameManager.instance.GetCurrentGoldAmount());

        // 완료
        EndModifyCurrentGoldAmount();
    }

    // 현재 골드 양 업데이트 되었다고 호출하는 함수
    private void StartModifyCurrentGoldAmount()
    {
        // 값이 실제로도 변경되었으면 호출
        long amount = GameManager.instance.GetCurrentGoldAmount();
        if (amount == _localCurrentGold)
            return;

        _endTime = _curTime + 0.3f;

        if (_animCoroutine != null)
            return;

        _animCoroutine = StartCoroutine(UpdateLocalGoldAmount());
    }

    // 현재 골드 양을 모두 업데이트 했을 때의 함수
    private void EndModifyCurrentGoldAmount()
    {
        _animCoroutine = null;
    }

    // 한 번 클릭했을 때, 얻는 양의 금을 출력
    private void PrintIncreaseGoldAmountWhenClicked(long amount, Color color)
    {
        // 0원일 때는 무시
        if (amount == 0) 
            return;

        GameObject obj = ObjectPooler.Instance.SpawnObject(ObjectType.AcquireInfoUI);
        obj.transform.SetParent(transform, false);

        obj.TryGetComponent(out AcquireGoldAmountUI acquireComp);
        if (acquireComp == null)
            return;

        acquireComp.AcquireGold(amount, startPosAcquireGold.transform.position, endPosAcquireGold.transform.position, color);
    }

    // 현재 골드 양 출력
    private void PrintCurrentGoldAmount(long amount)
    {
        _localCurrentGold = amount;
        textCurrentGoldAmount.text = FuncSystem.Format(_localCurrentGold);
    }

    // 한 번 클릭 시, 얻는 골드 양 텍스트를 업데이트합니다.
    private void UpdateClickAmountText()
    {
        long baseAmount = GameManager.instance.GetBaseClickIncreaseTotalAmount();
        float bonusMultiplier = GameManager.instance.GetTotalClickBonusMultiplier();

        string bonusText = "";
        if (bonusMultiplier > 1f)
        {
            // TODO: 색상 처리를 AuthorityManager 또는 GameManager에서 가져오는 것이 더 이상적입니다.
            // bonusText = $" <color=#{_localAuthorityColor}>(x{bonusMultiplier:F0})</color>";
            bonusText = $" <color=#FFAC00>(x{bonusMultiplier:F0})</color>";
        }

        textClickAmount.text = FuncSystem.Format(baseAmount) + bonusText;
    }

    // 한 번 클릭 시, 얻는 골드 양 출력 (이제 UpdateClickAmountText 호출)
    private void PrintClickGoldAmount()
    {
        UpdateClickAmountText();
    }

    // 주기적으로 얻는 골드 양 출력
    private void PrintPeriodGoldAmount()
    {
        long amount = GameManager.instance.GetPeriodIncreaseTotalAmount();
        string periodText = FuncSystem.Format(amount);

        // 피버 타임일 경우, 초당 골드에도 배율 표시
        if (AuthorityManager.instance.IsFeverTime)
        {
            periodText += $"<color=#{_localAuthorityColor}>(x{AuthorityManager.instance.GetTotalFeverMultiplier():F0})</color>";
        }
        textPeriodAmount.text = periodText;
    }

    // 현재 권위에 따른 배수 수치 출력 (이제 UpdateClickAmountText 호출)
    private void PrintCurrentAuthorityMultiplier(int amount, Color color)
    {
        _localAuthorityMultiplier = amount;
        _localAuthorityColor = ColorUtility.ToHtmlStringRGB(color);
        UpdateClickAmountText();
    }
}
