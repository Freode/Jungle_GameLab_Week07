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
    
    // 이미지가 골드 타겟(마커)까지 날아갈 시간
    [SerializeField] float imageFlyTime = 0.22f;

    // 씬(같은 Canvas)에 있는 골드 타겟 마커(예: GoldTargetMarker의 RectTransform)
    [SerializeField] RectTransform goldTargetMarker;

    // 금 획득량 표기 시작
    public void AcquireGold(long amount, Vector3 startPos, Vector3 endPos, Color color)
    {
        // 이전 재생 중 애니메이션 정리
        StopAllCoroutines();

        // 재사용 대비: UI 보이게
        if (imageGold) imageGold.gameObject.SetActive(true);
        if (textAmount) textAmount.gameObject.SetActive(true);

        string sign = amount >= 0 ? "+" : "";
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
        
        // 텍스트는 기존처럼 루트(프리팹) 이동 애니메이션 수행
        StartCoroutine(AnimGold(startPos, endPos, () =>
        {
            // 텍스트 먼저 끄기
            if (textAmount) textAmount.gameObject.SetActive(false);
        }));

        // 이미지는 마커까지 빠르게 따로 이동 후 즉시 반환
        StartCoroutine(FlyImageToMarker());
    }

    private void ModifySize()
    {
        float textWidth = textAmount.preferredWidth / 2f;
        float halfWidth = (textAmount.preferredWidth + 50f) / 2f;

        imageGold.transform.localPosition = new Vector3((-1) * halfWidth, imageGold.transform.localPosition.y, 0f);
        textAmount.transform.localPosition = new Vector3((-1) * halfWidth + 135f, textAmount.transform.localPosition.y, 0f);
    }

    // 텍스트용 루트 이동(기존 로직 유지) + 완료 콜백
    IEnumerator AnimGold(Vector3 startPos, Vector3 endPos, System.Action onComplete = null)
    {
        Vector3 curPos = startPos;
        float curTime = 0f;

        while (Vector3.Distance(curPos, endPos) > 0.05f)
        {
            curPos = startPos + (endPos - startPos) * (1f - (maxTime - curTime) / maxTime);
            transform.position = curPos;
            curTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 프리팹 반환은 이미지 쪽에서 처리하므로 여기서는 텍스트만 마무리
        onComplete?.Invoke();
    }

    IEnumerator FlyImageToMarker()
    {
        // 마커 자동 탐색(없으면 즉시 반환)
        if (goldTargetMarker == null)
        {
            var marker = FindObjectOfType<GoldTargetMarker>();
            if (marker != null) goldTargetMarker = marker.GetComponent<RectTransform>();
        }

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null || imageGold == null || goldTargetMarker == null)
        {
            CompleteAnimGold();
            yield break;
        }

        RectTransform imgRT = imageGold.rectTransform;
        Vector3 start = imgRT.position;
        Vector3 end   = goldTargetMarker.position;

// ★ 원래 RectTransform 상태 모두 저장
        Transform  originalParent       = imgRT.parent;
        int        originalSiblingIndex = imgRT.GetSiblingIndex();
        Vector3    originalLocalPos     = imgRT.localPosition;
        Vector2    originalAnchoredPos  = imgRT.anchoredPosition;
        Vector3    originalScale        = imgRT.localScale;
        Quaternion originalRot          = imgRT.localRotation;
        Vector2    originalAnchorMin    = imgRT.anchorMin;
        Vector2    originalAnchorMax    = imgRT.anchorMax;
        Vector2    originalPivot        = imgRT.pivot;

// 캔버스 루트로 이동(월드좌표 유지)
        imgRT.SetParent(canvas.transform, true);

        float t = 0f;
        float dur = Mathf.Max(0.0001f, imageFlyTime);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / dur);
            imgRT.position = Vector3.Lerp(start, end, p);
            yield return null;
        }

// ★ 원래 부모로 복귀 + 상태 복원(월드좌표 유지하지 않음)
        if (originalParent != null)
        {
            imgRT.SetParent(originalParent, false); // false: 로컬 기준으로 붙이기
            imgRT.anchorMin       = originalAnchorMin;
            imgRT.anchorMax       = originalAnchorMax;
            imgRT.pivot           = originalPivot;
            imgRT.localRotation   = originalRot;
            imgRT.localScale      = originalScale;
            imgRT.anchoredPosition= originalAnchoredPos; // anchored 우선 복원
            imgRT.localPosition   = originalLocalPos;    // (상황에 따라 anchored만으로도 충분)
            imgRT.SetSiblingIndex(originalSiblingIndex);
        }

        CompleteAnimGold();

    }


    private void CompleteAnimGold()
    {
        ObjectPooler.Instance.ReturnObject(gameObject);
    }
}
