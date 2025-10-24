using System.Collections;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
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
    public void AcquireGold(string amount, Vector3 startPos, Vector3 endPos, Color color)
    {
        textAmount.text = "+" + amount;

        ModifySize();

        // 금 이미지 조정
        if (color == Color.red)
        {
            imageGold.sprite = criticalGoldImage;
            textAmount.color = color;
        }
        else if (color == Color.green)
        {
            imageGold.sprite = normalGoldImage;
            textAmount.color = color;
        }
        else if (color == Color.black)
        {
            imageGold.sprite = dropGoldImage;
            textAmount.color = Color.green;
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
