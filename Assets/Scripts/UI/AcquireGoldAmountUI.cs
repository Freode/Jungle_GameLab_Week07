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

    // 이미지가 날아가기 전에 잠시 머무는 시간(실시간 기준)
    [SerializeField] float imageHoldTime = 0.2f;
    
    // 씬(같은 Canvas)에 있는 골드 타겟 마커(예: GoldTargetMarker의 RectTransform)
    [SerializeField] RectTransform goldTargetMarker;
    
    // 이미지 위치 미세 조정(텍스트가 보일 때)
    [SerializeField] private Vector2 imageOffsetWithText = new Vector2(12f, -8f);
// 이미지 위치 미세 조정(텍스트 없이 이미지만 나올 때)
    [SerializeField] private Vector2 imageOffsetImageOnly = new Vector2(12f, -8f);
    
    [Header("Font Size")]
    [SerializeField] private float defaultFontSize = 36f; // 인스펙터로 기본값 관리

    private Vector3 _imageBaseLocalPos;
    private Vector3 _textBaseLocalPos;
    
    void Awake()
    {
        if (imageGold != null) _imageBaseLocalPos = imageGold.transform.localPosition;
        if (textAmount != null)
        {
            _textBaseLocalPos = textAmount.transform.localPosition;
            if (defaultFontSize <= 0f) defaultFontSize = textAmount.fontSize; // 안전장치
        }
    }
    
    // ★ 오브젝트 풀 재사용 시 누적 방지 초기화
    private void ResetUI()
    {
        if (imageGold != null)
        {
            var t = imageGold.transform;
            t.localPosition = _imageBaseLocalPos;
            t.localRotation = Quaternion.identity;
            t.localScale    = Vector3.one;
        }

        if (textAmount != null)
        {
            var t = textAmount.transform;
            t.localPosition = _textBaseLocalPos;
            t.localRotation = Quaternion.identity;
            textAmount.gameObject.SetActive(false);
            textAmount.fontSize = defaultFontSize; // 폰트 크기 리셋
        }
    }


    // 금 획득량 표기 시작
// 시그니처 변경: showImage 추가 (기본값 true)
    public void AcquireGold(
        long amount,
        Vector3 startPos,
        Vector3 endPos,
        Color color,
        bool showText = true,
        bool showImage = true,
        float? overrideFontSize = null   // <-- 추가
    )
    {
        StopAllCoroutines();
        ResetUI();

        bool hideImage = (amount < 0) || !showImage;

        if (imageGold) imageGold.gameObject.SetActive(!hideImage);

        if (showText && textAmount)
        {
            textAmount.gameObject.SetActive(true);

            // 전갈 전용 등에서 크기 오버라이드
            if (overrideFontSize.HasValue && overrideFontSize.Value > 0f)
                textAmount.fontSize = overrideFontSize.Value;

            string sign = amount >= 0 ? "+" : "";
            textAmount.text = sign + FuncSystem.Format(amount);

            // 글자 크기 적용 후 폭 재계산
            ModifySize();
        }
        else if (textAmount)
        {
            textAmount.gameObject.SetActive(false);
            if (!hideImage)
            {
                var imgRT = imageGold.rectTransform;
                imgRT.anchoredPosition = imageOffsetImageOnly;
            }
        }

    // 색상/스프라이트 설정 (이미지 숨김이어도 텍스트 색상은 적용)
    if (color == Color.red)
    {
        if (!hideImage) imageGold.sprite = criticalGoldImage;
        if (showText) textAmount.color = color;
    }
    else if (color == Color.green)
    {
        if (!hideImage) imageGold.sprite = normalGoldImage;
        if (showText) textAmount.color = color;
    }
    else if (color == Color.black)
    {
        if (!hideImage) imageGold.sprite = dropGoldImage;
        if (showText) textAmount.color = Color.green;
    }
    else if (color == Color.magenta)
    {
        if (!hideImage) imageGold.sprite = dropGoldImage;
        if (showText) textAmount.color = color;
    }
    else if (color == Color.blue)
    {
        if (!hideImage) imageGold.sprite = normalGoldImage;
        if (showText) textAmount.color = color;
    }
    else
    {
        if (!hideImage) imageGold.sprite = normalGoldImage;
        if (showText) textAmount.color = Color.green;
    }

    // 텍스트 떠다니는 애니메이션
    StartCoroutine(AnimGold(startPos, endPos, () =>
    {
        if (showText && textAmount) textAmount.gameObject.SetActive(false);

        // ★ 이미지가 없으면 여기서 프리팹 반환까지 마무리
        if (hideImage) CompleteAnimGold();
    }));

    // ★ 이미지가 있을 때만 마커로 날아가게
    if (!hideImage)
        StartCoroutine(FlyImageToMarker());
}



    private void ModifySize()
    {
        float halfWidth = (textAmount.preferredWidth + 50f) * 0.5f;

        imageGold.transform.localPosition = new Vector3(
            (-1f) * halfWidth + imageOffsetWithText.x,
            _imageBaseLocalPos.y + imageOffsetWithText.y,   // ★ 기준값 사용
            _imageBaseLocalPos.z
        );

        textAmount.transform.localPosition = new Vector3(
            (-1f) * halfWidth + 135f,
            _textBaseLocalPos.y,                             // ★ 기준값 사용
            _textBaseLocalPos.z
        );
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

    // ★ 먼저 캔버스 루트로 분리(월드좌표 유지)해서 대기 동안 루트 이동 영향 안 받도록
    imgRT.SetParent(canvas.transform, true);

    // ★ 대기(실시간 기준)
    if (imageHoldTime > 0f)
        yield return new WaitForSecondsRealtime(imageHoldTime);

    // ★ 대기 후 현재 위치를 시작점으로 다시 확정
    Vector3 start = imgRT.position;
    Vector3 end   = goldTargetMarker.position;

    // 비행
    float t = 0f;
    float dur = Mathf.Max(0.0001f, imageFlyTime);
    while (t < dur)
    {
        t += Time.unscaledDeltaTime; // 일시정지 무시
        float p = Mathf.Clamp01(t / dur);
        imgRT.position = Vector3.Lerp(start, end, p);
        yield return null;
    }

    // 원래 부모로 복귀 + 상태 복원(월드좌표 유지하지 않음)
    if (originalParent != null)
    {
        imgRT.SetParent(originalParent, false);
        imgRT.anchorMin        = originalAnchorMin;
        imgRT.anchorMax        = originalAnchorMax;
        imgRT.pivot            = originalPivot;
        imgRT.localRotation    = originalRot;
        imgRT.localScale       = originalScale;
        imgRT.anchoredPosition = originalAnchoredPos;
        imgRT.localPosition    = originalLocalPos;
        imgRT.SetSiblingIndex(originalSiblingIndex);
    }

    // 전체 프리팹 반환
    CompleteAnimGold();
}



private void CompleteAnimGold()
{
    if (GoldVFXSpawnScheduler.Instance != null)
        GoldVFXSpawnScheduler.Instance.RequestReturn(gameObject);
    else
        ObjectPooler.Instance.ReturnObject(gameObject); // 폴백
}

}
