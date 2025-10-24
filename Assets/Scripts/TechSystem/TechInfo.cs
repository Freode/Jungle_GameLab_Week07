using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechInfo : MonoBehaviour
{
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDescription;
    public Image techIcon;
    public RectTransform infoTranform;

    private float modifyX = 0f;
    private float modifyY = 0f;

    private void Start()
    {
        gameObject.SetActive(false);

        modifyX = infoTranform.rect.width / 2f;
        modifyY = infoTranform.rect.height / 2f;
    }

    // 테크 정보 활성화
    public void OnActiveInfo(string name, string description, Sprite icon, Vector3 loc)
    {
        textName.text = name;
        textDescription.text = description;

        if (techIcon != null)
        {
            techIcon.sprite = icon;
        }

        // 1. 기본 위치 계산 (아이콘의 왼쪽에 표시)
        Vector3 desiredPos = new Vector3(loc.x - modifyX, loc.y - modifyY, 0);

        // 2. 툴팁 UI의 절반 넓이와 높이를 구합니다. (Pivot이 중앙이라고 가정)
        float halfWidth = infoTranform.rect.width / 2f;
        float halfHeight = infoTranform.rect.height / 2f;

        // 3. 툴팁의 중심점이 있을 수 있는 화면 상의 최소/최대 좌표를 계산합니다.
        float minX = halfWidth;
        float maxX = Screen.width - halfWidth;
        float minY = halfHeight;
        float maxY = Screen.height - halfHeight;

        // 4. Mathf.Clamp 함수로 툴팁의 위치가 화면 경계선을 넘지 않도록 강제로 고정합니다.
        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);

        // 5. 최종 계산된 위치를 적용합니다.
        gameObject.transform.position = desiredPos;
        gameObject.SetActive(true);
    }

    // 더 다양한 Info 출력
    public void OnActiveInfo(AreaType areaType, int currentLevel, int finalLevel, Sprite icon, Vector3 loc)
    {
        if (areaType == AreaType.Clear) return;

        IncreaseInfo increaseInfo = GameManager.instance.GetIncreaseGoldInfo(areaType);

        string name = FuncSystem.GetStructureName(areaType, currentLevel);

        long linearAmount = increaseInfo.clickTotalLinear * (100 + increaseInfo.clickRate) / 100;
        long periodAmount = increaseInfo.periodTotalLinear * (100 + increaseInfo.periodRate) / 100;

        string description = FuncSystem.GetStructureDescription(areaType, linearAmount, periodAmount, currentLevel, finalLevel);

        OnActiveInfo(name, description, icon, loc);
    }

    // 테크 정보 비활성화
    public void OnInactiveInfo()
    {
        gameObject.SetActive(false);
    }

}
