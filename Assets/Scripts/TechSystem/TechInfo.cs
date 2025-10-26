using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechInfo : MonoBehaviour
{
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDescription;
    public Image techIcon;
    public RectTransform infoTranform;

    [Header("Positioning")]
    [SerializeField] private AnchorMode anchorMode = AnchorMode.RelativeToLoc; // InfoUI 표시 기준
    [SerializeField] private Vector2 additionalOffset = Vector2.zero;          // 세부 오프셋(픽셀)

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

        // 1) 기준 위치 계산
        // - RelativeToLoc: 호출자가 준 좌표(loc)의 좌상단에 맞춰 배치(기존 동작)
        // - Screen* 모드: 화면 특정 코너 기준으로 배치
        // - RelativeToMouse: 마우스 위치 기준 배치
        Vector3 desiredPos;
        float halfWidth = infoTranform.rect.width / 2f;
        float halfHeight = infoTranform.rect.height / 2f;

        switch (anchorMode)
        {
            case AnchorMode.ScreenBottomLeft:
                desiredPos = new Vector3(halfWidth, halfHeight, 0);
                break;
            case AnchorMode.ScreenBottomRight:
                desiredPos = new Vector3(Screen.width - halfWidth, halfHeight, 0);
                break;
            case AnchorMode.ScreenTopLeft:
                desiredPos = new Vector3(halfWidth, Screen.height - halfHeight, 0);
                break;
            case AnchorMode.ScreenTopRight:
                desiredPos = new Vector3(Screen.width - halfWidth, Screen.height - halfHeight, 0);
                break;
            case AnchorMode.RelativeToMouse:
                desiredPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                break;
            case AnchorMode.RelativeToLoc:
            default:
                // 호출자가 준 좌표 주변(좌상단 정렬 느낌)으로 표시
                desiredPos = new Vector3(loc.x - modifyX, loc.y - modifyY, 0);
                break;
        }

        // 2) 추가 오프셋 적용
        desiredPos += new Vector3(additionalOffset.x, additionalOffset.y, 0f);

        // 3) 툴팁의 중심점이 있을 수 있는 화면 상의 최소/최대 좌표 계산
        float minX = halfWidth;
        float maxX = Screen.width - halfWidth;
        float minY = halfHeight;
        float maxY = Screen.height - halfHeight;

        // 4) 화면 경계선 넘지 않도록 클램프
        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);

        // 5) 최종 적용
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

public enum AnchorMode
{
    RelativeToLoc,      // 호출자 loc 기준(현행 기본 동작)
    RelativeToMouse,    // 마우스 위치 기준
    ScreenBottomLeft,   // 화면 좌하단
    ScreenBottomRight,  // 화면 우하단
    ScreenTopLeft,      // 화면 좌상단
    ScreenTopRight      // 화면 우상단
}
