using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcquireGoldAmountUI : MonoBehaviour
{
    public Image imageGold;
    public TextMeshProUGUI textAmount;

    public Sprite criticalGoldImage;
    public Sprite normalGoldImage;
    public Sprite dropGoldImage;

    [SerializeField] float maxTime = 0.5f;

    // 금 획득량 표기 시작
    public void AcquireGold(long amount, Vector3 startPos, Vector3 endPos, Color color)
    {
        string sign = amount >= 0 ? "+" : ""; // 양수일 때만 + 표시
        textAmount.text = sign + FuncSystem.Format(amount);

        ModifySize();

        // 금 이미지 조정
        if (color == Color.red) // 크리티컬 또는 빼앗긴 금
        {
            imageGold.sprite = criticalGoldImage;
            textAmount.color = color;
        }
        else if (color == Color.green) // 일반 획득 금
        {
            imageGold.sprite = normalGoldImage;
            textAmount.color = color;
        }
        else if (color == Color.black) // 드롭 금
        {
            imageGold.sprite = dropGoldImage;
            textAmount.color = Color.green;
        }
        else if (color == Color.magenta) // 전갈에게 빼앗긴 금 (새로운 색상)
        {
            imageGold.sprite = dropGoldImage; // 임시로 드롭 골드 이미지 사용
            textAmount.color = color;
        }
        else if (color == Color.blue) // 주기적으로 얻는 금
        {
            imageGold.sprite = normalGoldImage; // 일반 골드 이미지 사용
            textAmount.color = color;
        }
        else // 기본값 (예: 흰색 클릭 골드)
        {
            imageGold.sprite = normalGoldImage;
            textAmount.color = Color.green; // 클릭 골드를 초록색으로 변경
        }

        StartCoroutine(AnimGold(startPos, endPos));
    }

    private void ModifySize()
    {
        float textWidth = textAmount.preferredWidth / 2f;
        float halfWidth = (textAmount.preferredWidth + 50f) / 2f;

        imageGold.transform.localPosition = new Vector3((-1) * halfWidth, imageGold.transform.localPosition.y, 0f);
        textAmount.transform.localPosition = new Vector3((-1) * halfWidth + 135f, textAmount.transform.localPosition.y, 0f);
    }

    // 금 획득량 애님 업데이트
    IEnumerator AnimGold(Vector3 startPos, Vector3 endPos)
    {
        Vector3 curPos = startPos;
        float curTime = 0f;

        // 위치 변경
        while (Vector3.Distance(curPos, endPos) > 0.05f)
        {
            curPos = startPos + (endPos - startPos) * (1f - (maxTime - curTime) / maxTime);
            gameObject.transform.position = curPos;
            curTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        CompleteAnimGold();
    }

    private void CompleteAnimGold()
    {
        ObjectPooler.Instance.ReturnObject(gameObject);
    }
}
