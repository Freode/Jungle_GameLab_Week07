using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class CatGodHoverTip : MonoBehaviour
{
    [Header("표시할 캔버스/패널 루트 (자식 오브젝트)")]
    [SerializeField] private GameObject tipRoot;

    [Header("선택: TextMeshProUGUI 레퍼런스 (텍스트 갱신이 필요할 때)")]
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("선택: CanvasGroup 페이드 (없으면 단순 on/off)")]
    [SerializeField] private CanvasGroup tipCanvasGroup;
    [SerializeField] private float fadeDuration = 0.12f;

    [Header("옵션: Lift 중 자동 숨김")]
    [SerializeField] private bool hideWhileLifted = true;

    private bool _isHover;
    private float _fadeT;
    private CatGodMover _mover;

    private void Reset()
    {
        if (tipRoot == null)
        {
            var cg = GetComponentInChildren<Canvas>(true);
            if (cg != null) tipRoot = cg.gameObject;
        }
        if (tipCanvasGroup == null && tipRoot != null)
        {
            tipCanvasGroup = tipRoot.GetComponent<CanvasGroup>();
        }
        if (tipText == null && tipRoot != null)
        {
            tipText = tipRoot.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void Awake()
    {
        if (tipRoot == null)
        {
            var cg = GetComponentInChildren<Canvas>(true);
            if (cg != null) tipRoot = cg.gameObject;
        }
        _mover = GetComponent<CatGodMover>();

        // 시작은 숨김
        SetVisible(false, instant: true);

        // TextMeshPro가 입력 가로채지 않게
        if (tipText != null) tipText.raycastTarget = false;
    }

    private void Update()
    {
        // 선택: Lift 중엔 항상 숨김 유지
        if (hideWhileLifted && _mover != null && _mover.IsLifted())
        {
            if (IsVisible) SetVisible(false, instant: true);
            return;
        }
    }

    private void OnMouseEnter()
    {
        _isHover = true;
        Show();
    }

    private void OnMouseExit()
    {
        _isHover = false;
        Hide();
    }

    private void OnDisable()
    {
        _isHover = false;
        SetVisible(false, instant: true);
    }

    public void SetTipText(string text)
    {
        if (tipText != null) tipText.text = text;
    }

    // === 외부 제어용 공개 API 추가 ===
    public bool IsVisible
    {
        get
        {
            if (tipCanvasGroup != null) return tipCanvasGroup.alpha > 0.001f;
            return tipRoot != null && tipRoot.activeSelf;
        }
    }

    public void Show(bool instant = false)
    {
        SetVisible(true, instant);
    }

    public void Hide(bool instant = false)
    {
        SetVisible(false, instant);
    }

    // 내부 표시 처리 (기존 로직 유지)
    private void SetVisible(bool show, bool instant)
    {
        if (tipRoot == null) return;

        if (tipCanvasGroup == null)
        {
            tipRoot.SetActive(show);
            return;
        }

        StopAllCoroutines();
        if (instant || fadeDuration <= 0f)
        {
            tipCanvasGroup.alpha = show ? 1f : 0f;
            tipRoot.SetActive(show);
        }
        else
        {
            if (show && !tipRoot.activeSelf) tipRoot.SetActive(true);
            StartCoroutine(FadeCanvas(show ? 1f : 0f));
        }
    }

    private System.Collections.IEnumerator FadeCanvas(float target)
    {
        float start = tipCanvasGroup.alpha;
        _fadeT = 0f;
        while (_fadeT < fadeDuration)
        {
            _fadeT += Time.deltaTime;
            float t = Mathf.Clamp01(_fadeT / fadeDuration);
            tipCanvasGroup.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        tipCanvasGroup.alpha = target;

        if (Mathf.Approximately(target, 0f))
        {
            if (tipRoot != null) tipRoot.SetActive(false);
        }
    }
}
