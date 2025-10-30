using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllAcquireGoldUI : MonoBehaviour
{
    public GameObject verticalLayer;        //
    public GameObject eachAcquireGoldUIPrefab;

    public TextMeshProUGUI textClickGold;       // 클릭 금
    public TextMeshProUGUI textPeriodGold;      // 초당 금
    public TextMeshProUGUI textCurrentGold;     // 현재 금
    public Button InteractButton;
    public TextMeshProUGUI textButton;


    public List<AreaType> areaTypes = new List<AreaType>();

    private VerticalLayoutGroup _verticalLayerGroup;
    private Vector3 _startPos;
    private Vector3 _endPos;
    private Coroutine _cortoutine;
    private float _interval = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.instance.OnCurrentGoldAmountChanged += PrintCurrentGold;
        GameManager.instance.OnPeriodIncreaseAmountChanged += PrintPeriodGold;
        GameManager.instance.OnClickIncreaseTotalAmountChanged += PrintClickGold;
        InteractButton.onClick.AddListener(OnButtonClick);

        verticalLayer.TryGetComponent(out VerticalLayoutGroup verticalLayoutGroup);
        _verticalLayerGroup = verticalLayoutGroup;

        _startPos = transform.position;
        _endPos = transform.position + new Vector3(0f, -330f, 0f);
        CreateEachUI();
        PrintCurrentGold();
        PrintPeriodGold();
        PrintClickGold();
    }

    private void OnDestroy()
    {
        GameManager.instance.OnCurrentGoldAmountChanged -= PrintCurrentGold;
        GameManager.instance.OnPeriodIncreaseAmountChanged -= PrintPeriodGold;
        GameManager.instance.OnClickIncreaseTotalAmountChanged -= PrintClickGold;
    }

    void CreateEachUI()
    {
        foreach (AreaType areaType in areaTypes)
        {
            GameObject eachGoldUI = Instantiate(eachAcquireGoldUIPrefab, _verticalLayerGroup.transform);
            Debug.Log("1111");

            eachGoldUI.TryGetComponent(out EachAcquireGoldUI eachUI);
            if (eachUI == null) return;

            eachUI.Init(areaType);
        }
    }

    void PrintCurrentGold()
    {
        decimal amount = GameManager.instance.GetCurrentGoldAmount();
        textCurrentGold.text = $"금 소유량\n<color=#00FF00>{FuncSystem.Format(amount)}</color>";
    }

    void PrintPeriodGold()
    {
        decimal amount = GameManager.instance.GetPeriodIncreaseTotalAmount();
        string periodText = $"초당 금\n<color=#00FF00>{FuncSystem.Format(amount)}</color>";

        // 피버 타임일 경우, 초당 골드에도 배율 표시
        //if (AuthorityManager.instance.IsFeverTime)
        //{
        //    periodText += $"<color=#00FF00>(x{AuthorityManager.instance.GetTotalFeverMultiplier():F0})</color>";
        //}

        textPeriodGold.text = periodText;
    }

    void PrintClickGold()
    {
        decimal amount = GameManager.instance.GetBaseClickIncreaseTotalAmount();
        textClickGold.text = $"1인당 징수금\n<color=#00FF00>{FuncSystem.Format(amount)}</color>";
    }

    void OnButtonClick()
    {
        if (textButton.text == "▲")
            OpenUI();
        else
            CloseUI();
    }

    void OpenUI()
    {
        textButton.text = "▼";

        if (_cortoutine != null)
            StopCoroutine(_cortoutine);

        _cortoutine = StartCoroutine(OpenUI(false));

        GameLogger.Instance.Log("CheckGoldUI", "Open");
    }

    void CloseUI()
    {
        textButton.text = "▲";

        if (_cortoutine != null)
            StopCoroutine(_cortoutine);

        _cortoutine = StartCoroutine(OpenUI(true));

        GameLogger.Instance.Log("CheckGoldUI", "Close");
    }

    // 열리고 닫히는 애니메이션
    IEnumerator OpenUI(bool isOpen)
    {
        Vector3 target = (isOpen ? _endPos : _startPos);
        float timer = 0f;

        while(Vector3.Distance(transform.position, target) > 0.01f)
        {
            timer += Time.deltaTime;
            float dt = timer / _interval;
            transform.position = Vector3.Lerp(transform.position, target, dt);
            yield return new WaitForFixedUpdate();
        }
    }
}
